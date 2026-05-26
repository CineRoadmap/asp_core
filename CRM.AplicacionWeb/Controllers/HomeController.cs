// Archivo: CRM.AplicacionWeb\Controllers\HomeController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using System.Diagnostics;
using System.Security.Claims;
using CRM.AplicacionWeb.Models;
using CRM.Proyecto.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de HomeController dentro de la aplicacion.
public sealed class HomeController : Controller
{
    // Guarda la dependencia _homeService recibida por inyeccion.
    private readonly IHomeService _homeService;

    // Inicializa HomeController con las dependencias necesarias.
    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    // Carga el dashboard inicial con contenido general o personalizado segun haya usuario autenticado.
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Inicio";
        var dashboard = await _homeService.GetDashboardAsync(GetCurrentUserId(), cancellationToken);
        return View(dashboard);
    }

    // Muestra la pagina de error con el identificador de traza de la peticion actual.
    // Ejecuta la operacion Error con los parametros recibidos.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Extrae el id de usuario guardado en las claims de la cookie de autenticacion.
    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
