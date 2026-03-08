using System;
using System.ComponentModel.DataAnnotations;
using SistemaNominaADC.Entidades;

namespace SistemaNominaADC.Entidades.DTO
{
    public class EmpleadoDTO
    {
        public int IdEmpleado { get; set; }
        public string? IdentityUserId { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no debe exceder 20 caracteres.")]
        [RegularExpression(ValidacionPatrones.CedulaNumerica, ErrorMessage = "La cédula debe contener solo dígitos (9 a 20).")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no debe exceder 200 caracteres.")]
        [RegularExpression(ValidacionPatrones.NombreGeneral, ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
        public string NombreCompleto { get; set; } = string.Empty;

        public DateTime FechaIngreso { get; set; }
        public decimal SalarioBase { get; set; }
        public int IdPuesto { get; set; }
        public string? NombrePuesto { get; set; }
        public int IdEstado { get; set; }
        public string? NombreEstado { get; set; }
    }
}
