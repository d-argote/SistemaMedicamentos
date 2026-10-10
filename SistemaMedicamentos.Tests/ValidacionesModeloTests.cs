using System.ComponentModel.DataAnnotations;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Tests;

// Valida las Data Annotations + IValidatableObject de los modelos (patrón AAA).
public class ValidacionesModeloTests
{
    private static IList<ValidationResult> Validar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(modelo);
        Validator.TryValidateObject(modelo, contexto, resultados, validateAllProperties: true);
        return resultados;
    }

    private static Paciente PacienteValido() => new()
    {
        Nombres = "Carlos",
        Apellidos = "Ruiz Díaz",
        Dni = "1012345678",
        Telefono = "3001234567",
        FechaNacimiento = new DateTime(1995, 4, 20)
    };

    private static Medicina MedicinaValida() => new()
    {
        Nombre = "Loratadina 10mg",
        Laboratorio = "Genfar",
        Stock = 100,
        FechaVencimiento = DateTime.Today.AddYears(1)
    };

    [Fact]
    public void Paciente_CedulaCorta_EsInvalida()
    {
        // Arrange
        var paciente = PacienteValido();
        paciente.Dni = "12345"; // 5 dígitos, mínimo 6

        // Act
        var errores = Validar(paciente);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.Dni)));
    }

    [Fact]
    public void Paciente_Cedula10DigitosYTelefono10Digitos_EsValida()
    {
        // Arrange
        var paciente = PacienteValido(); // cédula 10 dígitos, teléfono 10 dígitos

        // Act
        var errores = Validar(paciente);

        // Assert
        Assert.Empty(errores);
    }

    [Fact]
    public void Paciente_Telefono11Digitos_EsInvalido()
    {
        // Arrange
        var paciente = PacienteValido();
        paciente.Telefono = "30012345678"; // 11 números, máximo 10

        // Act
        var errores = Validar(paciente);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.Telefono)));
    }

    [Fact]
    public void Paciente_FechaNacimientoFutura_EsInvalida()
    {
        // Arrange
        var paciente = PacienteValido();
        paciente.FechaNacimiento = DateTime.Today.AddDays(1);

        // Act
        var errores = Validar(paciente);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.FechaNacimiento)));
    }

    [Fact]
    public void Medicina_Vencida_EsInvalida()
    {
        // Arrange
        var medicina = MedicinaValida();
        medicina.FechaVencimiento = DateTime.Today.AddDays(-1);

        // Act
        var errores = Validar(medicina);

        // Assert
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Medicina.FechaVencimiento)));
    }

    [Fact]
    public void Medicina_Vigente_EsValida()
    {
        // Arrange
        var medicina = MedicinaValida();

        // Act
        var errores = Validar(medicina);

        // Assert
        Assert.Empty(errores);
    }
}
