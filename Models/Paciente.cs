using System.ComponentModel.DataAnnotations;

namespace SistemaMedicamentos.Models;

public class Paciente : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    // Cédula colombiana: entre 6 y 10 dígitos numéricos.
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(10, MinimumLength = 6, ErrorMessage = "La cédula debe tener entre 6 y 10 dígitos.")]
    [RegularExpression(@"^\d{6,10}$", ErrorMessage = "La cédula debe contener solo números (6 a 10 dígitos).")]
    [Display(Name = "Cédula")]
    public string Dni { get; set; } = string.Empty;

    // Teléfono colombiano: máximo 10 números (fijo 7 / celular 10).
    [StringLength(10, ErrorMessage = "El teléfono debe tener máximo 10 números.")]
    [RegularExpression(@"^\d{7,10}$", ErrorMessage = "El teléfono debe contener solo números (7 a 10 dígitos).")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(150)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime FechaNacimiento { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Today;

    // Relación uno a muchos: un paciente tiene muchas entregas.
    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    [Display(Name = "Paciente")]
    public string NombreCompleto => $"{Nombres} {Apellidos}";

    // Validación mínima: la fecha de nacimiento no puede ser futura ni irreal.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaNacimiento.Date > DateTime.Today)
            yield return new ValidationResult(
                "La fecha de nacimiento no puede ser futura.",
                new[] { nameof(FechaNacimiento) });
        if (FechaNacimiento.Date < DateTime.Today.AddYears(-120))
            yield return new ValidationResult(
                "La fecha de nacimiento no es válida (máximo 120 años).",
                new[] { nameof(FechaNacimiento) });
    }
}
