using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaMedicamentos.Models;
using SistemaMedicamentos.Services;

namespace SistemaMedicamentos.Controllers;

public class HomeController : Controller
{
    private readonly AppConfigService _config;

    // Singleton inyectado por DI.
    public HomeController(AppConfigService config)
    {
        _config = config;
    }

    public IActionResult Index()
    {
        ViewBag.AppDescripcion = _config.Descripcion;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
