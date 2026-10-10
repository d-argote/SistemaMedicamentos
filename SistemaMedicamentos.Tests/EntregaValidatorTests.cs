using SistemaMedicamentos.Models;
using SistemaMedicamentos.Services;

namespace SistemaMedicamentos.Tests;

// Pruebas unitarias sobre lógica de negocio (patrón AAA: Arrange, Act, Assert).
public class EntregaValidatorTests
{
    private readonly EntregaValidator _validator = new();

    private static Entrega NuevaEntrega() => new()
    {
        PacienteId = 1,
        MedicinaId = 1,
        FechaRecepcion = new DateTime(2026, 10, 1),
        FechaEntrega = new DateTime(2026, 10, 5),
        Cantidad = 10,
        Estado = EstadoEntrega.Pendiente
    };

    [Fact]
    public void Validar_FechaEntregaAnteriorARecepcion_EsInvalida()
    {
        // Arrange
        var entrega = NuevaEntrega();
        entrega.FechaEntrega = new DateTime(2026, 9, 30);

        // Act
        var (ok, error) = _validator.Validar(entrega, stockDisponible: 100, fechaVencimientoMedicina: new DateTime(2027, 1, 1));

        // Assert
        Assert.False(ok);
        Assert.Contains("anterior", error);
    }

    [Fact]
    public void Validar_CantidadCero_EsInvalida()
    {
        // Arrange
        var entrega = NuevaEntrega();
        entrega.Cantidad = 0;

        // Act
        var (ok, _) = _validator.Validar(entrega, stockDisponible: 100, fechaVencimientoMedicina: new DateTime(2027, 1, 1));

        // Assert
        Assert.False(ok);
    }

    [Fact]
    public void Validar_StockInsuficiente_EsInvalida()
    {
        // Arrange
        var entrega = NuevaEntrega();
        entrega.Cantidad = 50;

        // Act
        var (ok, error) = _validator.Validar(entrega, stockDisponible: 10, fechaVencimientoMedicina: new DateTime(2027, 1, 1));

        // Assert
        Assert.False(ok);
        Assert.Contains("Stock", error);
    }

    [Fact]
    public void Validar_MedicinaVencidaEnFechaEntrega_EsInvalida()
    {
        // Arrange
        var entrega = NuevaEntrega();

        // Act
        var (ok, _) = _validator.Validar(entrega, stockDisponible: 100, fechaVencimientoMedicina: new DateTime(2026, 10, 2));

        // Assert
        Assert.False(ok);
    }

    [Fact]
    public void Validar_EntregaDuplicada_EsInvalida()
    {
        // Arrange
        var entrega = NuevaEntrega();

        // Act
        var (ok, error) = _validator.Validar(entrega, stockDisponible: 100,
            fechaVencimientoMedicina: new DateTime(2027, 1, 1), yaExiste: true);

        // Assert
        Assert.False(ok);
        Assert.Contains("existe", error);
    }

    [Fact]
    public void Validar_EntregaCorrecta_EsValida()
    {
        // Arrange
        var entrega = NuevaEntrega();

        // Act
        var (ok, error) = _validator.Validar(entrega, stockDisponible: 100,
            fechaVencimientoMedicina: new DateTime(2027, 1, 1));

        // Assert
        Assert.True(ok);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void DescontarStock_RestaCorrectamente()
    {
        // Arrange
        int stock = 100;

        // Act
        var nuevo = _validator.DescontarStock(stock, 10);

        // Assert
        Assert.Equal(90, nuevo);
    }

    [Fact]
    public void DescontarStock_SinStock_LanzaExcepcion()
    {
        // Arrange
        var validator = new EntregaValidator();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => validator.DescontarStock(5, 10));
    }
}
