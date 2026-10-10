using System.ComponentModel.DataAnnotations;

namespace SistemaMedicamentos.Models;

// Sede (punto de entrega) donde se dispensan los medicamentos.
public class Sede
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la sede es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Sede")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    // Teléfono colombiano: máximo 10 números (fijo 7 / celular 10).
    [StringLength(10, ErrorMessage = "El teléfono debe tener máximo 10 números.")]
    [RegularExpression(@"^\d{7,10}$", ErrorMessage = "El teléfono debe contener solo números (7 a 10 dígitos).")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Display(Name = "Activa")]
    public bool Activa { get; set; } = true;

    // Relación uno a muchos: una sede atiende muchas entregas.
    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
}
