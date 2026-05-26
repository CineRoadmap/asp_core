// Archivo: CRM.AplicacionWeb\Controllers\AdminController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using CRM.AplicacionWeb.Models.Admin;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de AdminController dentro de la aplicacion.
[Authorize]
public sealed class AdminController : Controller
{
    // Guarda la dependencia _adminImportService recibida por inyeccion.
    private readonly IAdminImportService _adminImportService;

    // Guarda la dependencia _configuration recibida por inyeccion.
    private readonly IConfiguration _configuration;

    // Inicializa AdminController con las dependencias necesarias.
    public AdminController(IAdminImportService adminImportService, IConfiguration configuration)
    {
        _adminImportService = adminImportService;
        _configuration = configuration;
    }

    // Muestra el panel completo: importador, salud del catalogo y herramienta de prueba TMDB/IMDb.
    // Ejecuta la operacion Index con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!CurrentUserCanAccessAdmin())
        {
            return Forbid();
        }

        ViewData["Title"] = "Administracion";
        ViewData["ShellMode"] = "section";
        ViewData["Slogan"] = "Controla la importacion del catalogo desde TMDB y revisa su estado.";

        var status = await _adminImportService.GetStatusAsync(cancellationToken);
        return View(await BuildViewModelAsync(status, cancellationToken));
    }

    // Procesa el formulario que lanza la importacion del catalogo popular de TMDB.
    // Ejecuta la operacion ImportTmdb con los parametros recibidos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportTmdb(TmdbImportAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!CurrentUserCanAccessAdmin())
        {
            return Forbid();
        }

        var status = await _adminImportService.GetStatusAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Administracion";
            ViewData["ShellMode"] = "section";
            ViewData["Slogan"] = "Controla la importacion del catalogo desde TMDB y revisa su estado.";
            return View("Index", await BuildViewModelAsync(status, cancellationToken, model));
        }

        var apiKey = string.IsNullOrWhiteSpace(model.ApiKey)
            ? (_configuration["AdminPanel:TmdbApiKey"] ?? string.Empty).Trim()
            : model.ApiKey.Trim();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            TempData["Flash"] = "Debes indicar una API key de TMDB en el formulario o en la configuracion.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _adminImportService.StartTmdbImportAsync(
            new TmdbImportRequest
            {
                ApiKey = apiKey,
                TotalPages = model.TotalPages,
                Language = model.Language.Trim(),
                DelayMsBetweenPages = model.DelayMsBetweenPages,
                SkipWithoutPoster = model.SkipWithoutPoster,
                SkipWithoutOverview = model.SkipWithoutOverview
            },
            User.Identity?.Name ?? "desconocido",
            cancellationToken);

        TempData["Flash"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // Ejecuta una consulta puntual a TMDB y enseÃ±a si la ficha trae identificador/enlace de IMDb.
    // Ejecuta la operacion ProbeTmdb con los parametros recibidos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProbeTmdb(AdminApiProbeViewModel model, CancellationToken cancellationToken)
    {
        if (!CurrentUserCanAccessAdmin())
        {
            return Forbid();
        }

        ViewData["Title"] = "Administracion";
        ViewData["ShellMode"] = "section";
        ViewData["Slogan"] = "Controla la importacion del catalogo desde TMDB y revisa su estado.";

        var status = await _adminImportService.GetStatusAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(status, cancellationToken, probeInput: model));
        }

        var apiKey = string.IsNullOrWhiteSpace(model.ApiKey)
            ? (_configuration["AdminPanel:TmdbApiKey"] ?? string.Empty).Trim()
            : model.ApiKey.Trim();

        var result = await _adminImportService.ProbeTmdbMovieAsync(
            new TmdbApiProbeRequest
            {
                ApiKey = apiKey,
                MovieId = model.MovieId,
                Language = model.Language.Trim()
            },
            cancellationToken);

        return View("Index", await BuildViewModelAsync(status, cancellationToken, probeInput: model, probeResult: result));
    }

    // Compone todos los datos que necesita la vista del panel de administracion.
    private async Task<AdminPanelViewModel> BuildViewModelAsync(
        AdminImportStatusDto status,
        CancellationToken cancellationToken,
        TmdbImportAdminViewModel? importInput = null,
        AdminApiProbeViewModel? probeInput = null,
        AdminApiProbeResultDto? probeResult = null)
    {
        var health = await _adminImportService.GetCatalogHealthAsync(cancellationToken);

        return new AdminPanelViewModel
        {
            Import = BuildImportViewModel(status, importInput),
            Probe = BuildProbeViewModel(probeInput),
            Health = health,
            ProbeResult = probeResult
        };
    }

    // Rellena el formulario de importacion con valores introducidos o con la configuracion por defecto.
    private TmdbImportAdminViewModel BuildImportViewModel(AdminImportStatusDto status, TmdbImportAdminViewModel? input = null)
    {
        return new TmdbImportAdminViewModel
        {
            ApiKey = input?.ApiKey ?? string.Empty,
            TotalPages = input?.TotalPages ?? GetConfigInt("AdminPanel:DefaultTmdbPages", 10),
            Language = input?.Language ?? (_configuration["AdminPanel:DefaultTmdbLanguage"] ?? "es-ES"),
            DelayMsBetweenPages = input?.DelayMsBetweenPages ?? GetConfigInt("AdminPanel:DefaultTmdbDelayMs", 200),
            SkipWithoutPoster = input?.SkipWithoutPoster ?? GetConfigBool("AdminPanel:SkipWithoutPoster", true),
            SkipWithoutOverview = input?.SkipWithoutOverview ?? GetConfigBool("AdminPanel:SkipWithoutOverview", true),
            HasConfiguredApiKey = !string.IsNullOrWhiteSpace(_configuration["AdminPanel:TmdbApiKey"]),
            Status = status
        };
    }

    // Rellena el formulario de prueba de API con la ultima entrada del usuario o valores utiles de ejemplo.
    private AdminApiProbeViewModel BuildProbeViewModel(AdminApiProbeViewModel? input = null)
    {
        return new AdminApiProbeViewModel
        {
            ApiKey = input?.ApiKey ?? string.Empty,
            MovieId = input?.MovieId ?? GetConfigInt("AdminPanel:DefaultProbeTmdbMovieId", 550),
            Language = input?.Language ?? (_configuration["AdminPanel:DefaultTmdbLanguage"] ?? "es-ES")
        };
    }

    // Decide si el usuario actual puede usar el panel segun la lista configurada o el modo local abierto.
    private bool CurrentUserCanAccessAdmin()
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(currentUser))
        {
            return false;
        }

        if (GetConfigBool("AdminPanel:AllowAllAuthenticated", false))
        {
            return true;
        }

        var allowedUsers = _configuration.GetSection("AdminPanel:AllowedUsers").Get<string[]>() ?? [];
        return allowedUsers.Any(user => string.Equals(user, currentUser, StringComparison.OrdinalIgnoreCase));
    }

    // Lee enteros de configuracion y aplica un valor seguro si faltan o vienen mal.
    private int GetConfigInt(string key, int fallback)
    {
        return int.TryParse(_configuration[key], out var value) ? value : fallback;
    }

    // Lee booleanos de configuracion y aplica un valor seguro si faltan o vienen mal.
    private bool GetConfigBool(string key, bool fallback)
    {
        return bool.TryParse(_configuration[key], out var value) ? value : fallback;
    }
}
