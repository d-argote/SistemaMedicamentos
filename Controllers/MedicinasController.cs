using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Data;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Controllers;

[Authorize]
public class MedicinasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public MedicinasController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;
        var q = _context.Medicinas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            q = q.Where(m => m.Nombre.ToLower().Contains(termino) || m.Laboratorio.ToLower().Contains(termino));
        }
        return View(await q.OrderBy(m => m.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var medicina = await _context.Medicinas
            .Include(m => m.Entregas).ThenInclude(e => e.Paciente)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (medicina == null) return NotFound();
        return View(medicina);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create() => View(new Medicina { FechaVencimiento = DateTime.Today.AddYears(1) });

    // Model binding + manejo de archivos (imagen de la medicina).
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(Medicina medicina, IFormFile? imagen)
    {
        if (_context.Medicinas.Any(m => m.Nombre.ToLower() == medicina.Nombre.ToLower()))
            ModelState.AddModelError(nameof(medicina.Nombre), "Ya existe una medicina con ese nombre.");
        if (!ModelState.IsValid) return View(medicina);
        if (imagen != null && imagen.Length > 0)
            medicina.ImagenPath = await GuardarImagenAsync(imagen);
        _context.Add(medicina);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Medicina registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var medicina = await _context.Medicinas.FindAsync(id);
        if (medicina == null) return NotFound();
        return View(medicina);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, Medicina medicina, IFormFile? imagen)
    {
        if (id != medicina.Id) return NotFound();
        if (_context.Medicinas.Any(m => m.Nombre.ToLower() == medicina.Nombre.ToLower() && m.Id != medicina.Id))
            ModelState.AddModelError(nameof(medicina.Nombre), "Ya existe otra medicina con ese nombre.");
        if (!ModelState.IsValid) return View(medicina);
        try
        {
            if (imagen != null && imagen.Length > 0)
            {
                EliminarArchivo(medicina.ImagenPath);
                medicina.ImagenPath = await GuardarImagenAsync(imagen);
            }
            _context.Update(medicina);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Medicina actualizada correctamente.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Medicinas.Any(m => m.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var medicina = await _context.Medicinas.FindAsync(id);
        if (medicina == null) return NotFound();
        return View(medicina);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var medicina = await _context.Medicinas
            .Include(m => m.Entregas)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (medicina == null) return NotFound();
        if (medicina.Entregas.Any())
        {
            TempData["Error"] = "No se puede eliminar: la medicina tiene entregas registradas.";
            return RedirectToAction(nameof(Index));
        }
        EliminarArchivo(medicina.ImagenPath);
        _context.Medicinas.Remove(medicina);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Medicina eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Manejo de archivos: guarda la imagen en wwwroot/uploads/medicinas
    private async Task<string> GuardarImagenAsync(IFormFile archivo)
    {
        var carpeta = Path.Combine(_env.WebRootPath, "uploads", "medicinas");
        Directory.CreateDirectory(carpeta);
        var nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        var ruta = Path.Combine(carpeta, nombre);
        using var stream = new FileStream(ruta, FileMode.Create);
        await archivo.CopyToAsync(stream);
        return $"/uploads/medicinas/{nombre}";
    }

    private void EliminarArchivo(string? rutaRelativa)
    {
        if (string.IsNullOrEmpty(rutaRelativa)) return;
        var ruta = Path.Combine(_env.WebRootPath, rutaRelativa.TrimStart('/'));
        if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
    }
}
