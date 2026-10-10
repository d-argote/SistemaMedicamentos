using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Data;

// Datos semilla para pruebas: roles, usuarios, sedes, carga masiva
// (105 pacientes, 105 medicinas y 55 entregas) con datos colombianos.
// La generación es determinística (Random con semilla fija) y solo
// completa lo que falte, sin duplicar lo ya existente.
public static class SeedData
{
    public const string RolAdmin = "Administrador";
    public const string RolUsuario = "Usuario";

    public const string AdminEmail = "admin@medicamentos.local";
    public const string AdminPassword = "Admin123*";
    public const string UserEmail = "usuario@medicamentos.local";
    public const string UserPassword = "User123*";

    private const int TotalPacientes = 105;
    private const int TotalMedicinas = 105;
    private const int TotalEntregas = 55;

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Aplica migraciones pendientes (crea la BD SQLite si no existe).
        await context.Database.MigrateAsync();

        // Roles
        foreach (var rol in new[] { RolAdmin, RolUsuario })
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        // Usuario administrador
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                NombreCompleto = "Administrador",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, AdminPassword);
            await userManager.AddToRoleAsync(admin, RolAdmin);
        }

        // Usuario común
        var usuario = await userManager.FindByEmailAsync(UserEmail);
        if (usuario == null)
        {
            usuario = new ApplicationUser
            {
                UserName = UserEmail,
                Email = UserEmail,
                NombreCompleto = "Usuario de prueba",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(usuario, UserPassword);
            await userManager.AddToRoleAsync(usuario, RolUsuario);
        }

        var azar = new Random(42);

        // Pacientes base
        if (!await context.Pacientes.AnyAsync())
        {
            context.Pacientes.AddRange(
                new Paciente { Nombres = "María", Apellidos = "Torres Quispe", Dni = "12345678", Telefono = "3001234567", Direccion = "Av. Salud 123", FechaNacimiento = new DateTime(1990, 5, 10), FechaRegistro = DateTime.Today.AddDays(-30) },
                new Paciente { Nombres = "Juan", Apellidos = "Pérez López", Dni = "87654321", Telefono = "3109876543", Direccion = "Jr. Farmacia 456", FechaNacimiento = new DateTime(1985, 8, 22), FechaRegistro = DateTime.Today.AddDays(-20) },
                new Paciente { Nombres = "Ana", Apellidos = "García Ríos", Dni = "11223344", Telefono = "3204567890", Direccion = "Calle Botica 789", FechaNacimiento = new DateTime(2000, 1, 15), FechaRegistro = DateTime.Today.AddDays(-10) }
            );
            await context.SaveChangesAsync();
        }
        await CompletarPacientesAsync(context, azar);

        // Medicinas base
        if (!await context.Medicinas.AnyAsync())
        {
            context.Medicinas.AddRange(
                new Medicina { Nombre = "Paracetamol 500mg tabletas", Descripcion = "Analgésico y antipirético. Caja x 100 tabletas.", Stock = 500, Laboratorio = "Genfar", FechaVencimiento = DateTime.Today.AddYears(2), RequiereReceta = false },
                new Medicina { Nombre = "Amoxicilina 500mg cápsulas", Descripcion = "Antibiótico. Caja x 21 cápsulas.", Stock = 200, Laboratorio = "Lafrancol", FechaVencimiento = DateTime.Today.AddYears(1), RequiereReceta = true },
                new Medicina { Nombre = "Ibuprofeno 400mg tabletas", Descripcion = "Antiinflamatorio. Caja x 60 tabletas.", Stock = 350, Laboratorio = "Tecnoquímicas", FechaVencimiento = DateTime.Today.AddMonths(18), RequiereReceta = false }
            );
            await context.SaveChangesAsync();
        }
        await CompletarMedicinasAsync(context, azar);

        // Sedes de entrega (Ids fijos: las entregas existentes quedan en la sede 1)
        if (!await context.Sedes.AnyAsync())
        {
            context.Sedes.AddRange(
                new Sede { Id = 1, Nombre = "Sede Central Bogotá", Direccion = "Calle 45 # 12-30", Ciudad = "Bogotá", Telefono = "6012345678", Activa = true },
                new Sede { Id = 2, Nombre = "Sede Norte Medellín", Direccion = "Carrera 70 # 44-15", Ciudad = "Medellín", Telefono = "6044567890", Activa = true },
                new Sede { Id = 3, Nombre = "Sede Sur Cali", Direccion = "Av. Roosevelt # 25-40", Ciudad = "Cali", Telefono = "6023456789", Activa = true }
            );
            await context.SaveChangesAsync();
        }

        // Entregas base
        if (!await context.Entregas.AnyAsync())
        {
            var p1 = await context.Pacientes.FirstAsync();
            var p2 = await context.Pacientes.Skip(1).FirstAsync();
            var m1 = await context.Medicinas.FirstAsync();
            var m2 = await context.Medicinas.Skip(1).FirstAsync();

            context.Entregas.AddRange(
                new Entrega { PacienteId = p1.Id, MedicinaId = m1.Id, SedeId = 1, FechaRecepcion = DateTime.Today.AddDays(-5), FechaEntrega = DateTime.Today, Cantidad = 10, Estado = EstadoEntrega.Entregado, Observaciones = "Entrega de prueba 1", UserId = admin.Id },
                new Entrega { PacienteId = p2.Id, MedicinaId = m2.Id, SedeId = 2, FechaRecepcion = DateTime.Today.AddDays(-2), FechaEntrega = DateTime.Today.AddDays(2), Cantidad = 21, Estado = EstadoEntrega.Pendiente, Observaciones = "Entrega de prueba 2", UserId = admin.Id }
            );
            await context.SaveChangesAsync();
        }
        await CompletarEntregasAsync(context, azar, admin.Id);
    }

    // Completa hasta 105 pacientes con cédulas y celulares colombianos únicos.
    private static async Task CompletarPacientesAsync(AppDbContext context, Random azar)
    {
        var faltan = TotalPacientes - await context.Pacientes.CountAsync();
        if (faltan <= 0) return;

        var cedulas = new HashSet<string>(await context.Pacientes.Select(p => p.Dni).ToListAsync());
        string[] nombres = { "Carlos", "Lucía", "Andrés", "Sofía", "Diego", "Valentina", "Felipe", "Camila", "Santiago", "Isabella", "Mateo", "Daniela", "Sebastián", "Alejandra", "Nicolás", "Paola", "Cristian", "Juliana", "David", "Carolina", "Jorge", "Natalia", "Ricardo", "Fernanda" };
        string[] apellidos = { "García", "Rodríguez", "Martínez", "Hernández", "López", "González", "Pérez", "Sánchez", "Ramírez", "Cruz", "Flores", "Gómez", "Morales", "Reyes", "Díaz", "Mendoza", "Torres", "Ruiz", "Aguilar", "Castillo", "Ortiz", "Chávez", "Rojas", "Vargas" };
        string[] vias = { "Calle", "Carrera", "Avenida", "Diagonal", "Transversal" };
        string[] ciudades = { "Bogotá", "Medellín", "Cali", "Barranquilla", "Cartagena", "Bucaramanga" };

        var nuevos = new List<Paciente>();
        int intentos = 0;
        while (nuevos.Count < faltan && intentos < faltan * 20)
        {
            intentos++;
            int digitos = azar.Next(8, 11); // cédula de 8 a 10 dígitos
            var cedula = azar.Next(1, 10).ToString() + new string(Enumerable.Range(0, digitos - 1).Select(_ => (char)('0' + azar.Next(0, 10))).ToArray());
            if (!cedulas.Add(cedula)) continue;

            int edad = azar.Next(18, 86);
            nuevos.Add(new Paciente
            {
                Nombres = nombres[azar.Next(nombres.Length)],
                Apellidos = $"{apellidos[azar.Next(apellidos.Length)]} {apellidos[azar.Next(apellidos.Length)]}",
                Dni = cedula,
                Telefono = "3" + new string(Enumerable.Range(0, 9).Select(_ => (char)('0' + azar.Next(0, 10))).ToArray()),
                Direccion = $"{vias[azar.Next(vias.Length)]} {azar.Next(1, 100)} # {azar.Next(1, 60)}-{azar.Next(1, 99)}, {ciudades[azar.Next(ciudades.Length)]}",
                FechaNacimiento = DateTime.Today.AddYears(-edad).AddDays(-azar.Next(0, 365)),
                FechaRegistro = DateTime.Today.AddDays(-azar.Next(0, 120))
            });
        }
        context.Pacientes.AddRange(nuevos);
        await context.SaveChangesAsync();
    }

    // Completa hasta 105 medicinas con nombres únicos y vencimiento futuro.
    private static async Task CompletarMedicinasAsync(AppDbContext context, Random azar)
    {
        var faltan = TotalMedicinas - await context.Medicinas.CountAsync();
        if (faltan <= 0) return;

        var existentes = new HashSet<string>(await context.Medicinas.Select(m => m.Nombre.ToLower()).ToListAsync());
        (string Base, string Uso)[] bases =
        {
            ("Paracetamol", "Analgésico y antipirético"), ("Amoxicilina", "Antibiótico"), ("Ibuprofeno", "Antiinflamatorio"),
            ("Loratadina", "Antihistamínico"), ("Omeprazol", "Protector gástrico"), ("Metformina", "Antidiabético"),
            ("Losartán", "Antihipertensivo"), ("Atorvastatina", "Hipolipemiante"), ("Azitromicina", "Antibiótico"),
            ("Cetirizina", "Antialérgico"), ("Diclofenaco", "Antiinflamatorio"), ("Naproxeno", "Analgésico"),
            ("Esomeprazol", "Protector gástrico"), ("Salbutamol", "Broncodilatador"), ("Prednisona", "Corticoide"),
            ("Ciprofloxacino", "Antibiótico"), ("Fluconazol", "Antimicótico"), ("Albendazol", "Antiparasitario"),
            ("Aspirina", "Analgésico y antiagregante"), ("Clotrimazol", "Antimicótico tópico"), ("Hidrocortisona", "Corticoide tópico"),
            ("Enalapril", "Antihipertensivo"), ("Furosemida", "Diurético"), ("Clonazepam", "Ansiolítico"),
            ("Tramadol", "Analgésico opioide"), ("Insulina glargina", "Antidiabético"), ("Salmeterol", "Broncodilatador"),
            ("Doxiciclina", "Antibiótico")
        };
        string[] dosis = { "100mg", "200mg", "250mg", "400mg", "500mg", "1g" };
        string[] formas = { "tabletas", "cápsulas", "jarabe", "ampollas", "gotas" };
        string[] laboratorios = { "Genfar", "Tecnoquímicas", "Lafrancol", "Procaps", "La Santé", "Chalver", "Colmed", "Vitalis", "Siegfried", "Scandinavia Pharma" };

        var combinaciones = (from b in bases from d in dosis from f in formas select (b, d, f))
            .OrderBy(_ => azar.Next()).ToList();

        var nuevas = new List<Medicina>();
        foreach (var (b, d, f) in combinaciones)
        {
            if (nuevas.Count >= faltan) break;
            var nombre = $"{b.Base} {d} {f}";
            if (!existentes.Add(nombre.ToLower())) continue;
            nuevas.Add(new Medicina
            {
                Nombre = nombre,
                Descripcion = $"{b.Uso}. Caja x {azar.Next(10, 121)} {f}.",
                Stock = azar.Next(50, 1001),
                Laboratorio = laboratorios[azar.Next(laboratorios.Length)],
                FechaVencimiento = DateTime.Today.AddMonths(azar.Next(6, 37)),
                RequiereReceta = azar.NextDouble() < 0.4
            });
        }
        context.Medicinas.AddRange(nuevas);
        await context.SaveChangesAsync();
    }

    // Completa hasta 55 entregas válidas (recepción <= entrega, sin duplicados).
    private static async Task CompletarEntregasAsync(AppDbContext context, Random azar, string? adminId)
    {
        var faltan = TotalEntregas - await context.Entregas.CountAsync();
        if (faltan <= 0) return;

        var pacienteIds = await context.Pacientes.Select(p => p.Id).ToListAsync();
        var medicinaIds = await context.Medicinas.Select(m => m.Id).ToListAsync();
        var sedeIds = await context.Sedes.Select(s => s.Id).ToListAsync();
        var claves = new HashSet<string>(await context.Entregas
            .Select(e => e.PacienteId + "-" + e.MedicinaId + "-" + e.FechaEntrega.ToString()).ToListAsync());

        var nuevas = new List<Entrega>();
        int intentos = 0;
        while (nuevas.Count < faltan && intentos < faltan * 30)
        {
            intentos++;
            var recepcion = DateTime.Today.AddDays(-azar.Next(0, 46));
            var entregaFecha = recepcion.AddDays(azar.Next(0, 9));
            var pid = pacienteIds[azar.Next(pacienteIds.Count)];
            var mid = medicinaIds[azar.Next(medicinaIds.Count)];
            if (!claves.Add($"{pid}-{mid}-{entregaFecha}")) continue;

            EstadoEntrega estado = entregaFecha.Date <= DateTime.Today
                ? (azar.NextDouble() < 0.85 ? EstadoEntrega.Entregado : EstadoEntrega.Cancelado)
                : (azar.NextDouble() < 0.9 ? EstadoEntrega.Pendiente : EstadoEntrega.Cancelado);

            nuevas.Add(new Entrega
            {
                PacienteId = pid,
                MedicinaId = mid,
                SedeId = sedeIds[azar.Next(sedeIds.Count)],
                FechaRecepcion = recepcion,
                FechaEntrega = entregaFecha,
                Cantidad = azar.Next(1, 31),
                Estado = estado,
                Observaciones = azar.NextDouble() < 0.3 ? "Entrega de prueba masiva" : null,
                UserId = adminId
            });
        }
        context.Entregas.AddRange(nuevas);
        await context.SaveChangesAsync();
    }
}
