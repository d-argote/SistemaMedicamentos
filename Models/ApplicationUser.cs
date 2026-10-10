using Microsoft.AspNetCore.Identity;

namespace SistemaMedicamentos.Models;

// Usuario de la aplicación (Identity). Separa autenticación del dominio.
public class ApplicationUser : IdentityUser
{
    public string? NombreCompleto { get; set; }
}
