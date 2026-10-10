using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Data;

// DbContext con Identity + entidades del dominio. Usa SQLite.
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medicina> Medicinas => Set<Medicina>();
    public DbSet<Sede> Sedes => Set<Sede>();
    public DbSet<Entrega> Entregas => Set<Entrega>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Paciente 1 - N Entregas
        builder.Entity<Entrega>()
            .HasOne(e => e.Paciente)
            .WithMany(p => p.Entregas)
            .HasForeignKey(e => e.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Medicina 1 - N Entregas
        builder.Entity<Entrega>()
            .HasOne(e => e.Medicina)
            .WithMany(m => m.Entregas)
            .HasForeignKey(e => e.MedicinaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sede 1 - N Entregas
        builder.Entity<Entrega>()
            .HasOne(e => e.Sede)
            .WithMany(s => s.Entregas)
            .HasForeignKey(e => e.SedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Paciente>()
            .HasIndex(p => p.Dni)
            .IsUnique();
    }
}
