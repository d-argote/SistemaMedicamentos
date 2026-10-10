using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaMedicamentos.Models;

public class Medicina : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Medicina")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 100000, ErrorMessage = "El stock debe estar entre 0 y 100000.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "El laboratorio es obligatorio.")]
    [StringLength(100)]
    public string Laboratorio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateTime FechaVencimiento { get; set; }

    [Display(Name = "Requiere receta")]
    public bool RequiereReceta { get; set; }

    // Ruta del archivo de imagen subido a wwwroot/uploads/medicinas
    [Display(Name = "Imagen")]
    public string? ImagenPath { get; set; }

    // Relación uno a muchos: una medicina se entrega muchas veces.
    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    [NotMapped]
    [Display(Name = "¿Vencida?")]
    public bool Vencida => FechaVencimiento.Date < DateTime.Today;

    // Validación mínima: no se registran medicinas ya vencidas.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaVencimiento.Date < DateTime.Today)
            yield return new ValidationResult(
                "La fecha de vencimiento debe ser futura (no se registran medicinas vencidas).",
                new[] { nameof(FechaVencimiento) });
    }
}
