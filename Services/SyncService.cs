using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LabControlApi.Data;
using LabControlApi.DTOs.Sync;
using LabControlApi.Models;
using LabControlApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabControlApi.Services
{
    public class SyncService : ISyncService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SyncService> _logger;

        public SyncService(AppDbContext context, ILogger<SyncService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SyncBatchResponseDto> ProcessBatchAsync(Guid userId, SyncBatchRequestDto batchDto)
        {
            var response = new SyncBatchResponseDto
            {
                ServerTimestamp = DateTime.UtcNow,
                TotalProcessed = batchDto.Operations.Count
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var op in batchDto.Operations)
                {
                    // 1. Verificação de Idempotência (Replay Protection)
                    var existingLog = await _context.SyncOperationLogs
                        .FirstOrDefaultAsync(l => l.UserId == userId && l.ClientOperationId == op.ClientOperationId);

                    if (existingLog != null)
                    {
                        response.Results.Add(new SyncOperationResultDto
                        {
                            ClientOperationId = op.ClientOperationId,
                            Status = "ALREADY_PROCESSED",
                            Message = "Operação já havia sido processada anteriormente com sucesso.",
                            EntityId = existingLog.EntityId
                        });
                        response.SuccessCount++;
                        continue;
                    }

                    // 2. Processar a Operação Individual
                    var opResult = await ProcessSingleOperationAsync(userId, op);
                    response.Results.Add(opResult);

                    if (opResult.Status == "SUCCESS") response.SuccessCount++;
                    else if (opResult.Status == "CONFLICT") response.ConflictCount++;
                    else response.ErrorCount++;

                    // 3. Registrar Log de Idempotência
                    var log = new SyncOperationLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ClientOperationId = op.ClientOperationId,
                        EntityType = op.EntityType,
                        EntityId = opResult.EntityId,
                        Action = op.Action.ToUpperInvariant(),
                        Status = opResult.Status,
                        ProcessedAt = DateTime.UtcNow
                    };

                    _context.SyncOperationLogs.Add(log);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar lote de sincronização para o usuário {UserId}", userId);
                throw;
            }

            return response;
        }

        private async Task<SyncOperationResultDto> ProcessSingleOperationAsync(Guid userId, SyncOperationItemDto op)
        {
            var entityType = op.EntityType.ToLowerInvariant();
            var action = op.Action.ToUpperInvariant();

            return entityType switch
            {
                "machineevent" or "event" => await HandleMachineEventSyncAsync(userId, op, action),
                "machine" => await HandleMachineSyncAsync(userId, op, action),
                "plant" => await HandlePlantSyncAsync(userId, op, action),
                _ => new SyncOperationResultDto
                {
                    ClientOperationId = op.ClientOperationId,
                    Status = "ERROR",
                    Message = $"Tipo de entidade '{op.EntityType}' não suportada para sincronização offline."
                }
            };
        }

        private async Task<SyncOperationResultDto> HandleMachineEventSyncAsync(Guid userId, SyncOperationItemDto op, string action)
        {
            if (action == "CREATE")
            {
                if (!op.Data.HasValue)
                {
                    return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "Dados ausentes para criação de evento." };
                }

                var root = op.Data.Value;
                if (!root.TryGetProperty("machineId", out var midProp) || !Guid.TryParse(midProp.GetString(), out var machineId))
                {
                    return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "MachineId inválido ou ausente." };
                }

                // Validar propriedade da máquina
                var machine = await _context.Machines.Include(m => m.Plant).FirstOrDefaultAsync(m => m.Id == machineId && m.Plant.UserId == userId);
                if (machine == null)
                {
                    return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "Máquina não encontrada ou não pertence ao usuário." };
                }

                var eventId = op.EntityId ?? Guid.NewGuid();
                var machineEvent = new MachineEvent
                {
                    Id = eventId,
                    MachineId = machineId,
                    EventType = root.TryGetProperty("eventType", out var et) ? et.GetString() ?? "OFFLINE_EVENT" : "OFFLINE_EVENT",
                    Message = root.TryGetProperty("message", out var msg) ? msg.GetString() ?? string.Empty : string.Empty,
                    CreatedAt = op.ClientTimestamp
                };

                _context.MachineEvents.Add(machineEvent);
                return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "SUCCESS", EntityId = eventId, Message = "Evento sincronizado e criado com sucesso." };
            }

            if (action == "UPDATE" || action == "RESOLVE")
            {
                if (!op.EntityId.HasValue)
                {
                    return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "EntityId obrigatório para atualizar evento." };
                }

                var ev = await _context.MachineEvents
                    .Include(e => e.Machine)
                        .ThenInclude(m => m.Plant)
                    .FirstOrDefaultAsync(e => e.Id == op.EntityId.Value && e.Machine.Plant.UserId == userId);

                if (ev == null)
                {
                    return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "Evento não encontrado ou sem permissão." };
                }

                // Detecção de conflito: se o evento já foi resolvido no servidor antes do cliente
                if (ev.ResolvedAt.HasValue && ev.ResolvedAt.Value > op.ClientTimestamp)
                {
                    return new SyncOperationResultDto
                    {
                        ClientOperationId = op.ClientOperationId,
                        Status = "CONFLICT",
                        EntityId = ev.Id,
                        Message = "Conflito: Evento já havia sido resolvido no servidor em data posterior.",
                        CurrentServerState = new { ev.Id, ev.ResolvedAt, ev.Message, ev.EventType }
                    };
                }

                ev.ResolvedAt = op.ClientTimestamp;
                _context.MachineEvents.Update(ev);
                return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "SUCCESS", EntityId = ev.Id, Message = "Evento marcado como resolvido via sincronização." };
            }

            return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = $"Ação '{action}' não suportada para eventos." };
        }

        private async Task<SyncOperationResultDto> HandleMachineSyncAsync(Guid userId, SyncOperationItemDto op, string action)
        {
            if (!op.EntityId.HasValue)
            {
                return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "EntityId obrigatório para operações em máquinas." };
            }

            var machine = await _context.Machines
                .Include(m => m.Plant)
                .FirstOrDefaultAsync(m => m.Id == op.EntityId.Value && m.Plant.UserId == userId);

            if (machine == null)
            {
                return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "Máquina não encontrada ou sem permissão." };
            }

            if (action == "UPDATE")
            {
                // Detecção de Conflito de Concorrência
                if (machine.UpdatedAt > op.ClientTimestamp)
                {
                    return new SyncOperationResultDto
                    {
                        ClientOperationId = op.ClientOperationId,
                        Status = "CONFLICT",
                        EntityId = machine.Id,
                        Message = "Conflito: A máquina foi alterada no servidor mais recentemente do que no dispositivo offline.",
                        CurrentServerState = new { machine.Id, machine.Name, machine.Status, machine.PosX, machine.PosY, machine.UpdatedAt }
                    };
                }

                if (op.Data.HasValue)
                {
                    var root = op.Data.Value;
                    if (root.TryGetProperty("status", out var st)) machine.Status = st.GetString() ?? machine.Status;
                    if (root.TryGetProperty("posX", out var px) && px.TryGetDouble(out var posX)) machine.PosX = posX;
                    if (root.TryGetProperty("posY", out var py) && py.TryGetDouble(out var posY)) machine.PosY = posY;
                    machine.UpdatedAt = DateTime.UtcNow;

                    _context.Machines.Update(machine);
                }

                return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "SUCCESS", EntityId = machine.Id, Message = "Máquina atualizada via sincronização." };
            }

            return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = $"Ação '{action}' não suportada para máquinas via sync." };
        }

        private async Task<SyncOperationResultDto> HandlePlantSyncAsync(Guid userId, SyncOperationItemDto op, string action)
        {
            if (action == "UPDATE" && op.EntityId.HasValue)
            {
                var plant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == op.EntityId.Value && p.UserId == userId);
                if (plant == null)
                {
                    return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = "Planta não encontrada." };
                }

                if (plant.UpdatedAt > op.ClientTimestamp)
                {
                    return new SyncOperationResultDto
                    {
                        ClientOperationId = op.ClientOperationId,
                        Status = "CONFLICT",
                        EntityId = plant.Id,
                        Message = "Conflito: A planta foi alterada no servidor.",
                        CurrentServerState = new { plant.Id, plant.Name, plant.UpdatedAt }
                    };
                }

                if (op.Data.HasValue)
                {
                    var root = op.Data.Value;
                    if (root.TryGetProperty("name", out var n)) plant.Name = n.GetString() ?? plant.Name;
                    if (root.TryGetProperty("description", out var d)) plant.Description = d.GetString() ?? plant.Description;
                    plant.UpdatedAt = DateTime.UtcNow;
                    _context.Plants.Update(plant);
                }

                return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "SUCCESS", EntityId = plant.Id, Message = "Planta atualizada com sucesso." };
            }

            return new SyncOperationResultDto { ClientOperationId = op.ClientOperationId, Status = "ERROR", Message = $"Ação '{action}' não suportada para plantas via sync." };
        }
    }
}

