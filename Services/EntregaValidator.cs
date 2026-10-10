using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Services;

// Lógica de negocio de entregas. Se prueba con pruebas unitarias (patrón AAA).
public class EntregaValidator
{
    // Valida reglas: fechas coherentes, cantidad válida, stock suficiente,
// medicina no vencida y no duplicar entrega (mismo paciente+medicina+fecha).
    public (bool EsValida, string Error) Validar(
        Entrega entrega,
        int stockDisponible,
        DateTime fechaVencimientoMedicina,
        bool yaExiste = false)
    {
        if (entrega.FechaEntrega.Date < entrega.FechaRecepcion.Date)
            return (false, "La fecha de entrega no puede ser anterior a la fecha de recepción.");

        if (entrega.Cantidad <= 0)
            return (false, "La cantidad debe ser mayor a cero.");

        if (entrega.Cantidad > stockDisponible)
            return (false, $"Stock insuficiente. Disponible: {stockDisponible}.");

        if (entrega.FechaEntrega.Date > fechaVencimientoMedicina.Date)
            return (false, "La medicina estará vencida en la fecha de entrega.");

        if (yaExiste)
            return (false, "Ya existe una entrega igual (mismo paciente, medicina y fecha de entrega).");

        return (true, string.Empty);
    }

    // Descuenta stock al confirmar una entrega.
    public int DescontarStock(int stockActual, int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));
        if (cantidad > stockActual) throw new InvalidOperationException("Stock insuficiente.");
        return stockActual - cantidad;
    }
}
