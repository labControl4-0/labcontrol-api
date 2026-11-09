# [S4][BE-06] Sincronização Idempotente para Operações Offline (Mobile)

## 📌 Visão Geral e Objetivo
Implementação do endpoint de sincronização em lote (`POST /api/sync`) para atender às operações offline realizadas pelo aplicativo mobile ([`labcontrol-mobile`](https://github.com/labControl4-0/labcontrol-mobile)). Permite que operadores de campo no chão de fábrica resolvam alertas, criem eventos ou alterem status de máquinas mesmo sem internet. Quando a conexão é restabelecida, o lote de ações é enviado ao backend com **garantia de idempotência (sem duplicação por retries)** e **detecção explícita de conflitos de concorrência**.

---

## 🛠️ O Que Foi Implementado

### 1. Entidade de Auditoria e Idempotência (`SyncOperationLog.cs`)
- Armazena o registro de cada operação executada:
  - `Id` (UUID interno)
  - `UserId` (usuário autenticado dono da ação)
  - `ClientOperationId` (UUID gerado localmente pelo aplicativo mobile no momento da ação)
  - `EntityType` ("MachineEvent", "Machine", "Plant")
  - `EntityId` (identificador do recurso)
  - `Action` ("CREATE", "UPDATE", "RESOLVE", "DELETE")
  - `Status` ("SUCCESS", "ALREADY_PROCESSED", "CONFLICT", "ERROR")
  - `ProcessedAt` (timestamp do servidor)
- Migration no EF Core com índice único em `(UserId, ClientOperationId)` para assegurar idempotência a nível de banco de dados.

### 2. DTOs de Sincronização
- [`SyncOperationItemDto`](../../DTOs/Sync/SyncOperationItemDto.cs): Item do lote contendo identificadores locais, dados da entidade e timestamp do celular.
- [`SyncBatchRequestDto`](../../DTOs/Sync/SyncBatchRequestDto.cs): Lote contendo o `ClientId` do aparelho e a lista de operações.
- [`SyncOperationResultDto`](../../DTOs/Sync/SyncOperationResultDto.cs): Retorno individual para cada operação com status, mensagem e `CurrentServerState` em caso de conflito.
- [`SyncBatchResponseDto`](../../DTOs/Sync/SyncBatchResponseDto.cs): Resumo do lote (total processado, sucessos, conflitos, erros).

### 3. Tratamento de Idempotência e Replay Seguro
Se o aplicativo mobile enviar um lote e a conexão cair antes de receber a resposta HTTP:
- O mobile repetirá a chamada com o mesmo `ClientOperationId`.
- O [`SyncService.cs`](../../Services/SyncService.cs) identifica que a operação já foi executada para aquele usuário.
- O backend retorna status **`ALREADY_PROCESSED`**, **sem executar a criação/edição novamente**, garantindo que nenhum registro seja duplicado.

### 4. Detecção Explícita de Conflitos
Se um operador estava offline e alterou um evento ou máquina, mas outro operador ou o sistema web já havia modificado o mesmo registro no servidor em data posterior (`UpdatedAt > ClientTimestamp`):
- O backend rejeita a alteração silenciosa.
- Retorna status **`CONFLICT`**.
- Devolve o estado real atual do servidor (`CurrentServerState`), permitindo que a tela do aplicativo informe ao operador o que aconteceu para que ele decida se sobrescreve ou mantém.

### 5. Transações Atômicas
O lote é processado dentro de uma transação de banco de dados (`IDbContextTransaction`). Se houver falha de infraestrutura, o lote sofre rollback de forma consistente.

---

## ✅ Critérios de Aceite Atendidos

| Critério do Card | Como Foi Cumprido |
| :--- | :--- |
| **Repetir uma operação após timeout não duplica registros** | Verificação prévia de `ClientOperationId` retornando `ALREADY_PROCESSED`. |
| **Conflito retorna estado explícito** | Resposta HTTP traz `status: "CONFLICT"` acompanhado do `CurrentServerState`. |
| **Exclusões e replay são tratados sem perda silenciosa** | Logs de auditoria gravados e validação rigorosa de posse de dados por usuário. |

---

## 🧪 Como Testar e Explicar para o Grupo

### Envio de Lote de Sincronização
- **Endpoint:** `POST /api/sync`
- **Header:** `Authorization: Bearer <SEU_TOKEN_JWT>`

#### Exemplo de Requisição:
```json
{
  "clientId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
  "operations": [
    {
      "clientOperationId": "11111111-2222-3333-4444-555555555555",
      "entityType": "MachineEvent",
      "action": "CREATE",
      "clientTimestamp": "2026-11-09T08:15:00Z",
      "data": {
        "machineId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "eventType": "MANUTENCAO_PREVENTIVA",
        "message": "Troca de óleo efetuada em modo offline"
      }
    },
    {
      "clientOperationId": "66666666-7777-8888-9999-000000000000",
      "entityType": "MachineEvent",
      "action": "RESOLVE",
      "entityId": "22222222-3333-4444-5555-666666666666",
      "clientTimestamp": "2026-11-09T08:20:00Z"
    }
  ]
}
```

#### Exemplo de Resposta (Sucesso + Idempotência):
```json
{
  "serverTimestamp": "2026-11-09T08:50:00Z",
  "totalProcessed": 2,
  "successCount": 2,
  "conflictCount": 0,
  "errorCount": 0,
  "results": [
    {
      "clientOperationId": "11111111-2222-3333-4444-555555555555",
      "status": "SUCCESS",
      "message": "Evento sincronizado e criado com sucesso.",
      "entityId": "99999999-aaaa-bbbb-cccc-dddddddddddd"
    },
    {
      "clientOperationId": "66666666-7777-8888-9999-000000000000",
      "status": "SUCCESS",
      "message": "Evento marcado como resolvido via sincronização.",
      "entityId": "22222222-3333-4444-5555-666666666666"
    }
  ]
}
```
*Se a mesma requisição for reenviada:*
O retorno para `11111111-2222-3333-4444-555555555555` será com status `"ALREADY_PROCESSED"`, sem duplicar o registro no banco de dados.

