using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Data;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Controllers;

[Authorize]
public class SedesController : Controller
{
    private readonly AppDbContext _context;

    public SedesController(AppDbContext context) => _context = context;

    // GET: Sedes (ambos roles pueden listar y ver detalle)
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;
        var q = _context.Sedes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            q = q.Where(s => s.Nombre.ToLower().Contains(termino) || s.Ciudad.ToLower().Contains(termino));
        }
        return View(await q.OrderBy(s => s.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var sede = await _context.Sedes
            .Include(s => s.Entregas).ThenInclude(e => e.Paciente)
            .Include(s => s.Entregas).ThenInclude(e => e.Medicina)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sede == null) return NotFound();
        return View(sede);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create() => View(new Sede());

    // Model binding: recibe Sede del formulario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(Sede sede)
    {
        if (_context.Sedes.Any(s => s.Nombre.ToLower() == sede.Nombre.ToLower()))
            ModelState.AddModelError(nameof(sede.Nombre), "Ya existe una sede con ese nombre.");
        if (!ModelState.IsValid) return View(sede);
        _context.Add(sede);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Sede registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var sede = await _context.Sedes.FindAsync(id);
        if (sede == null) return NotFound();
        return View(sede);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, Sede sede)
    {
        if (id != sede.Id) return NotFound();
        if (_context.Sedes.Any(s => s.Nombre.ToLower() == sede.Nombre.ToLower() && s.Id != sede.Id))
            ModelState.AddModelError(nameof(sede.Nombre), "Ya existe otra sede con ese nombre.");
        if (!ModelState.IsValid) return View(sede);
        try
        {
            _context.Update(sede);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Sede actualizada correctamente.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Sedes.Any(s => s.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var sede = await _context.Sedes.FindAsync(id);
        if (sede == null) return NotFound();
        return View(sede);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sede = await _context.Sedes
            .Include(s => s.Entregas)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sede == null) return NotFound();
        if (sede.Entregas.Any())
        {
            TempData["Error"] = "No se puede eliminar: la sede tiene entregas registradas.";
            return RedirectToAction(nameof(Index));
        }
        _context.Sedes.Remove(sede);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Sede eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
