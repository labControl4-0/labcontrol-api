using System.ComponentModel.DataAnnotations;

namespace LabControlApi.DTOs.Device
{
    public class RegisterDeviceDto
    {
        [Required(ErrorMessage = "O token do dispositivo é obrigatório.")]
        public string DeviceToken { get; set; } = string.Empty;

        public string? Platform { get; set; } // android, ios, web

        public string? DeviceName { get; set; }
    }
}

