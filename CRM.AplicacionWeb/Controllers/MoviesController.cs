// Archivo: CRM.AplicacionWeb\Controllers\MoviesController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using System.Security.Claims;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de MoviesController dentro de la aplicacion.
public sealed class MoviesController : Controller
{
    // Guarda la dependencia _movieService recibida por inyeccion.
    private readonly IMovieService _movieService;

    // Inicializa MoviesController con las dependencias necesarias.
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    // Renderiza el catalogo y normaliza nombres antiguos/nuevos de filtros para mantener compatibilidad con enlaces existentes.
    // Ejecuta la operacion Index con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery(Name = "busqueda")] string? legacySearch = null,
        string? search = null,
        [FromQuery(Name = "genero")] int? legacyGenreId = null,
        int? genreId = null,
        [FromQuery(Name = "anio")] int? legacyYear = null,
        int? year = null,
        [FromQuery(Name = "ver")] string? legacyViewMode = null,
        string? viewMode = null,
        int? p = null,
        int? page = null,
        CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Peliculas";

        var currentUserId = GetCurrentUserId();
        var resolvedSearch = string.IsNullOrWhiteSpace(legacySearch) ? search ?? string.Empty : legacySearch;
        var resolvedGenreId = NormalizeOptionalPositiveInt(legacyGenreId ?? genreId);
        var resolvedYear = NormalizeOptionalPositiveInt(legacyYear ?? year);
        var resolvedPage = Math.Max(p ?? page ?? 1, 1);
        var resolvedViewMode = ResolveViewMode(legacyViewMode, viewMode, currentUserId.HasValue);

        var catalog = await _movieService.GetCatalogAsync(new MovieFilterRequest
        {
            Search = resolvedSearch,
            GenreId = resolvedGenreId,
            Year = resolvedYear,
            ViewMode = resolvedViewMode,
            Page = resolvedPage,
            PageSize = 40
        }, currentUserId, cancellationToken);

        return View(catalog);
    }

    // Muestra la ficha de una pelicula o devuelve 404 si el id no existe en el catalogo.
    // Ejecuta la operacion Details con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var details = await _movieService.GetDetailsAsync(id, GetCurrentUserId(), cancellationToken);
        if (details is null)
        {
            return NotFound();
        }

        ViewData["Title"] = details.Title;
        return View(details);
    }

    // Guarda la puntuacion del usuario autenticado y vuelve a la ficha actualizada.
    // Ejecuta la operacion Rate con los parametros recibidos.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, int score, CancellationToken cancellationToken)
    {
        await _movieService.RateAsync(id, GetCurrentUserId()!.Value, score, cancellationToken);
        TempData["Flash"] = "Tu valoracion se ha guardado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Anade o elimina la pelicula de "Mi Lista" para el usuario autenticado.
    // Ejecuta la operacion ToggleWatchlist con los parametros recibidos.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleWatchlist(int id, CancellationToken cancellationToken)
    {
        await _movieService.ToggleWatchlistAsync(id, GetCurrentUserId()!.Value, cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    // Lee el identificador de usuario desde las claims creadas en el login.
    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    // Traduce los modos de vista legacy a los valores internos usados por el repositorio.
    private static string ResolveViewMode(string? legacyView, string? currentViewMode, bool isAuthenticated)
    {
        if (!isAuthenticated)
        {
            return "all";
        }

        var value = !string.IsNullOrWhiteSpace(legacyView)
            ? legacyView.Trim().ToLowerInvariant()
            : (currentViewMode ?? string.Empty).Trim().ToLowerInvariant();

        return value switch
        {
            "lista" => "watchlist",
            "valoradas" => "rated",
            "watchlist" => "watchlist",
            "rated" => "rated",
            _ => "all"
        };
    }

    // Convierte filtros opcionales a null cuando llegan vacios, cero o negativos.
    private static int? NormalizeOptionalPositiveInt(int? value) =>
        value.HasValue && value.Value > 0 ? value.Value : null;
}
