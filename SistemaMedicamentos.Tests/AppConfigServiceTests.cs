using Microsoft.Extensions.Configuration;
using Moq;
using SistemaMedicamentos.Services;

namespace SistemaMedicamentos.Tests;

// Pruebas del Singleton + uso de mocks (Moq) para IConfiguration.
public class AppConfigServiceTests
{
    [Fact]
    public void Instance_SiempreRetornaLaMismaInstancia()
    {
        // Arrange + Act
        var a = AppConfigService.Instance;
        var b = AppConfigService.Instance;

        // Assert
        Assert.Same(a, b);
    }

    [Fact]
    public void Configure_LeeNombreYVersion_ConMockDeConfiguration()
    {
        // Arrange: mock de IConfiguration (Moq)
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["AppConfig:NombreApp"]).Returns("App de Prueba");
        mock.Setup(c => c["AppConfig:Version"]).Returns("9.9");

        var service = AppConfigService.Instance;

        // Act
        service.Configure(mock.Object);

        // Assert
        Assert.Equal("App de Prueba", service.NombreApp);
        Assert.Equal("9.9", service.Version);
        Assert.Contains("9.9", service.Descripcion);
    }
}
