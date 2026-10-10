using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Data;
using SistemaMedicamentos.Models;
using SistemaMedicamentos.Services;

namespace SistemaMedicamentos.Controllers;

[Authorize]
public class EntregasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly EntregaValidator _validator;

    public EntregasController(AppDbContext context, IWebHostEnvironment env, EntregaValidator validator)
    {
        _context = context;
        _env = env;
        _validator = validator;
    }

    // Lista con LINQ (incluye paciente, medicina y sede).
    public async Task<IActionResult> Index(string? estado)
    {
        var q = _context.Entregas
            .Include(e => e.Paciente)
            .Include(e => e.Medicina)
            .Include(e => e.Sede)
            .AsQueryable();
        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoEntrega>(estado, out var est))
            q = q.Where(e => e.Estado == est);
        ViewBag.EstadoActual = estado;
        return View(await q.OrderBy(e => e.FechaEntrega).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entrega = await _context.Entregas
            .Include(e => e.Paciente)
            .Include(e => e.Medicina)
            .Include(e => e.Sede)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entrega == null) return NotFound();
        return View(entrega);
    }

    // Ambos roles pueden registrar entregas.
    [Authorize(Roles = "Administrador,Usuario")]
    public IActionResult Create()
    {
        CargarListas();
        return View(new Entrega { FechaRecepcion = DateTime.Today, FechaEntrega = DateTime.Today });
    }

    // Model binding + validación de negocio + archivo (comprobante).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Usuario")]
    public async Task<IActionResult> Create(Entrega entrega, IFormFile? comprobante)
    {
        var medicina = await _context.Medicinas.FindAsync(entrega.MedicinaId);
        if (medicina == null) ModelState.AddModelError(nameof(entrega.MedicinaId), "Medicina no válida.");
        else
        {
            var yaExiste = await _context.Entregas.AnyAsync(e =>
                e.PacienteId == entrega.PacienteId &&
                e.MedicinaId == entrega.MedicinaId &&
                e.FechaEntrega == entrega.FechaEntrega);
            var (ok, error) = _validator.Validar(entrega, medicina.Stock, medicina.FechaVencimiento, yaExiste);
            if (!ok) ModelState.AddModelError(string.Empty, error);
        }
        if (!await _context.Sedes.AnyAsync(s => s.Id == entrega.SedeId))
            ModelState.AddModelError(nameof(entrega.SedeId), "Sede no válida.");

        if (!ModelState.IsValid)
        {
            CargarListas(entrega.PacienteId, entrega.MedicinaId, entrega.SedeId);
            return View(entrega);
        }

        if (comprobante != null && comprobante.Length > 0)
            entrega.ComprobantePath = await GuardarComprobanteAsync(comprobante);
        entrega.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Add(entrega);
        // Descuenta stock al registrar.
        medicina!.Stock = _validator.DescontarStock(medicina.Stock, entrega.Cantidad);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Entrega registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entrega = await _context.Entregas.FindAsync(id);
        if (entrega == null) return NotFound();
        CargarListas(entrega.PacienteId, entrega.MedicinaId, entrega.SedeId);
        return View(entrega);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, Entrega entrega, IFormFile? comprobante)
    {
        if (id != entrega.Id) return NotFound();
        var medicina = await _context.Medicinas.FindAsync(entrega.MedicinaId);
        if (medicina == null) ModelState.AddModelError(nameof(entrega.MedicinaId), "Medicina no válida.");
        else
        {
            var (ok, error) = _validator.Validar(entrega, medicina.Stock + entrega.Cantidad, medicina.FechaVencimiento);
            if (!ok) ModelState.AddModelError(string.Empty, error);
        }
        if (!await _context.Sedes.AnyAsync(s => s.Id == entrega.SedeId))
            ModelState.AddModelError(nameof(entrega.SedeId), "Sede no válida.");
        if (!ModelState.IsValid)
        {
            CargarListas(entrega.PacienteId, entrega.MedicinaId, entrega.SedeId);
            return View(entrega);
        }
        try
        {
            if (comprobante != null && comprobante.Length > 0)
            {
                EliminarArchivo(entrega.ComprobantePath);
                entrega.ComprobantePath = await GuardarComprobanteAsync(comprobante);
            }
            _context.Update(entrega);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Entrega actualizada correctamente.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Entregas.Any(e => e.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entrega = await _context.Entregas
            .Include(e => e.Paciente).Include(e => e.Medicina).Include(e => e.Sede)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entrega == null) return NotFound();
        return View(entrega);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entrega = await _context.Entregas.FindAsync(id);
        if (entrega == null) return NotFound();
        EliminarArchivo(entrega.ComprobantePath);
        _context.Entregas.Remove(entrega);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Entrega eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Descarga del comprobante (manejo de archivos). Ambos roles.
    [Authorize(Roles = "Administrador,Usuario")]
    public async Task<IActionResult> DescargarComprobante(int id)
    {
        var entrega = await _context.Entregas.FindAsync(id);
        if (entrega == null || string.IsNullOrEmpty(entrega.ComprobantePath)) return NotFound();
        var ruta = Path.Combine(_env.WebRootPath, entrega.ComprobantePath.TrimStart('/'));
        if (!System.IO.File.Exists(ruta)) return NotFound();
        var bytes = await System.IO.File.ReadAllBytesAsync(ruta);
        return File(bytes, "application/octet-stream", Path.GetFileName(ruta));
    }

    // Endpoint JSON para Full Calendar (también en DashboardController).
    [HttpGet]
    public async Task<IActionResult> EventosJson()
    {
        var eventos = await _context.Entregas
            .Include(e => e.Paciente).Include(e => e.Medicina)
            .Select(e => new
            {
                id = e.Id,
                title = $"{e.Medicina!.Nombre} → {e.Paciente!.NombreCompleto} ({e.Sede!.Nombre})",
                start = e.FechaEntrega.ToString("yyyy-MM-dd"),
                end = e.FechaEntrega.ToString("yyyy-MM-dd"),
                url = Url.Action("Details", "Entregas", new { id = e.Id }),
                color = e.Estado == EstadoEntrega.Entregado ? "#1e8e5a" : e.Estado == EstadoEntrega.Pendiente ? "#1b4f72" : "#a93226"
            }).ToListAsync();
        return Json(eventos);
    }

    private void CargarListas(object? selPaciente = null, object? selMedicina = null, object? selSede = null)
    {
        ViewBag.PacienteId = new SelectList(_context.Pacientes.OrderBy(p => p.Apellidos), "Id", "NombreCompleto", selPaciente);
        ViewBag.MedicinaId = new SelectList(_context.Medicinas.OrderBy(m => m.Nombre), "Id", "Nombre", selMedicina);
        ViewBag.SedeId = new SelectList(_context.Sedes.Where(s => s.Activa).OrderBy(s => s.Nombre), "Id", "Nombre", selSede);
    }

    private async Task<string> GuardarComprobanteAsync(IFormFile archivo)
    {
        var carpeta = Path.Combine(_env.WebRootPath, "uploads", "comprobantes");
        Directory.CreateDirectory(carpeta);
        var nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        var ruta = Path.Combine(carpeta, nombre);
        using var stream = new FileStream(ruta, FileMode.Create);
        await archivo.CopyToAsync(stream);
        return $"/uploads/comprobantes/{nombre}";
    }

    private void EliminarArchivo(string? rutaRelativa)
    {
        if (string.IsNullOrEmpty(rutaRelativa)) return;
        var ruta = Path.Combine(_env.WebRootPath, rutaRelativa.TrimStart('/'));
        if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
    }
}
