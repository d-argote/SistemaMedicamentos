using System.ComponentModel.DataAnnotations;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Tests;

// Validaciones de la entidad Sede (patrón AAA).
public class SedeTests
{
    private static IList<ValidationResult> Validar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, validateAllProperties: true);
        return resultados;
    }

    private static Sede SedeValida() => new()
    {
        Nombre = "Sede Central Bogotá",
        Direccion = "Calle 45 # 12-30",
        Ciudad = "Bogotá",
        Telefono = "6012345678",
        Activa = true
    };

    [Fact]
    public void Sede_Completa_EsValida()
    {
        // Arrange
        var sede = SedeValida();

        // Act
        var errores = Validar(sede);

        // Assert
        Assert.Empty(errores);
    }

    [Fact]
    public void Sede_SinNombre_EsInvalida()
    {
        // Arrange
        var sede = SedeValida();
        sede.Nombre = string.Empty;

        // Act
        var errores = Validar(sede);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Sede.Nombre)));
    }

    [Fact]
    public void Sede_Telefono11Digitos_EsInvalido()
    {
        // Arrange
        var sede = SedeValida();
        sede.Telefono = "60123456789"; // 11 números, máximo 10

        // Act
        var errores = Validar(sede);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Sede.Telefono)));
    }

    [Fact]
    public void Entrega_SinSede_EsInvalida()
    {
        // Arrange
        var entrega = new Entrega
        {
            PacienteId = 1,
            MedicinaId = 1,
            SedeId = 0, // sin seleccionar
            FechaRecepcion = DateTime.Today,
            FechaEntrega = DateTime.Today,
            Cantidad = 1
        };

        // Act
        var errores = Validar(entrega);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Entrega.SedeId)));
    }
}
