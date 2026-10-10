using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMedicamentos.Data;
using SistemaMedicamentos.Models;
using SistemaMedicamentos.Services;

namespace SistemaMedicamentos.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly AppConfigService _config;

    public DashboardController(AppDbContext context, AppConfigService config)
    {
        _context = context;
        _config = config;
    }

    // Panel con conteos (LINQ).
    public async Task<IActionResult> Index()
    {
        ViewBag.AppDescripcion = _config.Descripcion;
        ViewBag.TotalPacientes = await _context.Pacientes.CountAsync();
        ViewBag.TotalMedicinas = await _context.Medicinas.CountAsync();
        ViewBag.TotalSedes = await _context.Sedes.CountAsync();
        ViewBag.TotalEntregas = await _context.Entregas.CountAsync();
        ViewBag.Pendientes = await _context.Entregas.CountAsync(e => e.Estado == EstadoEntrega.Pendiente);
        ViewBag.StockBajo = await _context.Medicinas.CountAsync(m => m.Stock < 50);
        ViewBag.EntregasPorSede = await _context.Sedes
            .Select(s => new { Sede = s.Nombre, Ciudad = s.Ciudad, Total = s.Entregas.Count })
            .OrderByDescending(x => x.Total)
            .ToListAsync();
        var proximas = await _context.Entregas
            .Include(e => e.Paciente).Include(e => e.Medicina).Include(e => e.Sede)
            .OrderBy(e => e.FechaEntrega)
            .Take(5).ToListAsync();
        return View(proximas);
    }

    public IActionResult Calendar() => View();

    // Endpoint JSON que alimenta Full Calendar vía AJAX.
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
}
