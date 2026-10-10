using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Data;
using SistemaMedicamentos.Models;

namespace SistemaMedicamentos.Controllers;

[Authorize]
public class PacientesController : Controller
{
    private readonly AppDbContext _context;

    public PacientesController(AppDbContext context) => _context = context;

    // GET: Pacientes (ambos roles pueden listar y ver detalle)
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;
        var q = _context.Pacientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            q = q.Where(p => p.Nombres.ToLower().Contains(termino) || p.Apellidos.ToLower().Contains(termino) || p.Dni.Contains(buscar));
        }
        return View(await q.OrderBy(p => p.Apellidos).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var paciente = await _context.Pacientes
            .Include(p => p.Entregas).ThenInclude(e => e.Medicina)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (paciente == null) return NotFound();
        return View(paciente);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create() => View(new Paciente { FechaNacimiento = new DateTime(1990, 1, 1) });

    // Model binding: recibe Paciente del formulario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(Paciente paciente)
    {
        if (_context.Pacientes.Any(p => p.Dni == paciente.Dni))
            ModelState.AddModelError(nameof(paciente.Dni), "Ya existe un paciente con esa cédula.");
        if (!ModelState.IsValid) return View(paciente);
        _context.Add(paciente);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Paciente registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();
        return View(paciente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, Paciente paciente)
    {
        if (id != paciente.Id) return NotFound();
        if (_context.Pacientes.Any(p => p.Dni == paciente.Dni && p.Id != paciente.Id))
            ModelState.AddModelError(nameof(paciente.Dni), "Ya existe otro paciente con esa cédula.");
        if (!ModelState.IsValid) return View(paciente);
        try
        {
            _context.Update(paciente);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Paciente actualizado correctamente.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Pacientes.Any(p => p.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();
        return View(paciente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var paciente = await _context.Pacientes
            .Include(p => p.Entregas)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (paciente == null) return NotFound();
        if (paciente.Entregas.Any())
        {
            TempData["Error"] = "No se puede eliminar: el paciente tiene entregas registradas.";
            return RedirectToAction(nameof(Index));
        }
        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Paciente eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
