// Archivo: CRM.AplicacionWeb\Controllers\AchievementsController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using System.Security.Claims;
using CRM.Proyecto.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de AchievementsController dentro de la aplicacion.
[Authorize]
public sealed class AchievementsController : Controller
{
    // Guarda la dependencia _achievementService recibida por inyeccion.
    private readonly IAchievementService _achievementService;

    // Inicializa AchievementsController con las dependencias necesarias.
    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    // Muestra los logros del usuario autenticado y aplica filtro opcional por tipo.
    // Ejecuta la operacion Index con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Index(string? filter, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Logros";
        ViewBag.ActiveFilter = filter?.Trim().ToLowerInvariant() ?? string.Empty;
        var items = await _achievementService.GetAchievementsAsync(GetCurrentUserId()!.Value, filter, cancellationToken);
        return View(items);
    }

    // Lee el identificador de usuario desde las claims de la sesion actual.
    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
