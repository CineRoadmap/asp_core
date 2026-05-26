// Archivo: CRM.AplicacionWeb\Controllers\ChallengesController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using System.Security.Claims;
using CRM.Proyecto.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de ChallengesController dentro de la aplicacion.

[Authorize]
public sealed class ChallengesController : Controller
{
    // Guarda la dependencia _challengeService recibida por inyeccion.
    private readonly IChallengeService _challengeService;

    // Inicializa ChallengesController con las dependencias necesarias.
    public ChallengesController(IChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    // Muestra los retos asignados al usuario autenticado con su progreso actual.
    // Ejecuta la operacion Index con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Retos";
        var items = await _challengeService.GetChallengesAsync(GetCurrentUserId()!.Value, cancellationToken);
        return View(items);
    }

    // Lee el identificador de usuario desde las claims de la sesion actual.
    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
