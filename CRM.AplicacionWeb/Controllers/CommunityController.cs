// Archivo: CRM.AplicacionWeb\Controllers\CommunityController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using CRM.Proyecto.Contracts;
using CRM.AplicacionWeb.Models.Community;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de CommunityController dentro de la aplicacion.
public sealed class CommunityController : Controller
{
    // Guarda la dependencia _communityService recibida por inyeccion.
    private readonly ICommunityService _communityService;

    // Inicializa CommunityController con las dependencias necesarias.
    public CommunityController(ICommunityService communityService)
    {
        _communityService = communityService;
    }

    // Carga el listado de miembros y sus metricas para la pagina de comunidad.
    // Ejecuta la operacion Index con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        ViewData["Title"] = "Comunidad";
        var currentUserId = GetCurrentUserId();
        var ranking = await _communityService.GetMembersAsync(null, cancellationToken);
        var profiles = await _communityService.GetMembersAsync(currentUserId, cancellationToken);

        return View(new CommunityIndexViewModel
        {
            CurrentUserId = currentUserId,
            Ranking = ranking,
            Profiles = profiles
        });
    }

    // Lee el identificador de usuario actual para no mostrarlo dentro de la comunidad.
    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
