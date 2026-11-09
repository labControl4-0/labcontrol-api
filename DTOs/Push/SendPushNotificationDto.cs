using System;
using System.ComponentModel.DataAnnotations;

namespace LabControlApi.DTOs.Push
{
    public class SendPushNotificationDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string Category { get; set; } = "ALERTA"; // Ex: ALERTA, MANUTENCAO, SISTEMA

        public Guid? AvisoId { get; set; }

        public string Origin { get; set; } = "LabControl-API"; // Ex: MQTT, MachineMonitor, Manual

        public object? AdditionalData { get; set; }
    }
}

