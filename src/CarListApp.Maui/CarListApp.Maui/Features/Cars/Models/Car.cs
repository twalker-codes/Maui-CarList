using System.ComponentModel.DataAnnotations;

namespace CarListApp.Maui.Features.Cars.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required]
        public string Make { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Required]
        public string Vin { get; set; } = string.Empty;
    }
} 