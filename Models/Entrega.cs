using System.ComponentModel.DataAnnotations;

namespace SistemaMedicamentos.Models;

public enum EstadoEntrega
{
    Pendiente,
    Entregado,
    Cancelado
}

public class Entrega
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Seleccione un paciente.")]
    [Display(Name = "Paciente")]
    public int PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    [Required(ErrorMessage = "Seleccione una medicina.")]
    [Display(Name = "Medicina")]
    public int MedicinaId { get; set; }
    public Medicina? Medicina { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una sede de entrega.")]
    [Display(Name = "Sede de entrega")]
    public int SedeId { get; set; }
    public Sede? Sede { get; set; }

    [Required(ErrorMessage = "La fecha de recepción es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de recepción")]
    public DateTime FechaRecepcion { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La fecha de entrega es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de entrega")]
    public DateTime FechaEntrega { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000.")]
    public int Cantidad { get; set; } = 1;

    [Required]
    public EstadoEntrega Estado { get; set; } = EstadoEntrega.Pendiente;

    [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    public string? Observaciones { get; set; }

    // Ruta del comprobante subido a wwwroot/uploads/comprobantes
    [Display(Name = "Comprobante")]
    public string? ComprobantePath { get; set; }

    // Usuario que registró la entrega (Identity)
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
