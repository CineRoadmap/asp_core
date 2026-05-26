// Archivo: CRM.AplicacionWeb\Models\Admin\TmdbImportAdminViewModel.cs
// Modelo de vista utilizado por la zona de administracion para lanzar importaciones desde TMDB.

using System.ComponentModel.DataAnnotations;
using CRM.Proyecto.Dtos;

namespace CRM.AplicacionWeb.Models.Admin;

// Transporta los datos de TmdbImportAdminViewModel entre capas.
public sealed record TmdbImportAdminViewModel
{
    // Expone el valor ApiKey usado por esta capa de la aplicacion.
    [Display(Name = "API key de TMDB")]
    public string ApiKey { get; init; } = string.Empty;

    // Expone el valor TotalPages usado por esta capa de la aplicacion.
    [Display(Name = "Paginas a importar")]
    [Range(1, 1000, ErrorMessage = "Debes indicar entre 1 y 1000 paginas.")]
    public int TotalPages { get; init; } = 10;

    // Expone el valor Language usado por esta capa de la aplicacion.
    [Display(Name = "Idioma")]
    [Required(ErrorMessage = "Debes indicar un idioma.")]
    public string Language { get; init; } = "es-ES";

    // Expone el valor DelayMsBetweenPages usado por esta capa de la aplicacion.
    [Display(Name = "Espera entre paginas (ms)")]
    [Range(0, 5000, ErrorMessage = "Debes indicar un valor entre 0 y 5000 ms.")]
    public int DelayMsBetweenPages { get; init; } = 200;

    // Expone el valor SkipWithoutPoster usado por esta capa de la aplicacion.
    [Display(Name = "Omitir peliculas sin poster")]
    public bool SkipWithoutPoster { get; init; } = true;

    // Expone el valor SkipWithoutOverview usado por esta capa de la aplicacion.
    [Display(Name = "Omitir peliculas sin sinopsis")]
    public bool SkipWithoutOverview { get; init; } = true;

    // Expone el valor HasConfiguredApiKey usado por esta capa de la aplicacion.
    public bool HasConfiguredApiKey { get; init; }

    // Ejecuta la operacion Status con los parametros recibidos.
    public AdminImportStatusDto Status { get; init; } = new(
        false,
        "Inactivo",
        "Sin importaciones ejecutadas todavia.",
        string.Empty,
        null,
        null,
        0,
        0,
        0,
        0,
        0,
        0,
        string.Empty);
}

// Transporta los datos de AdminApiProbeViewModel entre capas.
public sealed record AdminApiProbeViewModel
{
    // Expone el valor ApiKey usado por esta capa de la aplicacion.
    [Display(Name = "API key de TMDB")]
    public string ApiKey { get; init; } = string.Empty;

    // Expone el valor MovieId usado por esta capa de la aplicacion.
    [Display(Name = "ID de pelicula en TMDB")]
    [Range(1, int.MaxValue, ErrorMessage = "Debes indicar un ID valido de TMDB.")]
    public int MovieId { get; init; } = 550;

    // Expone el valor Language usado por esta capa de la aplicacion.
    [Display(Name = "Idioma")]
    [Required(ErrorMessage = "Debes indicar un idioma.")]
    public string Language { get; init; } = "es-ES";
}

// Transporta los datos de AdminPanelViewModel entre capas.
public sealed record AdminPanelViewModel
{
    // Ejecuta la operacion Import con los parametros recibidos.
    public TmdbImportAdminViewModel Import { get; init; } = new();

    // Ejecuta la operacion Probe con los parametros recibidos.
    public AdminApiProbeViewModel Probe { get; init; } = new();

    // Ejecuta la operacion Health con los parametros recibidos.
    public AdminCatalogHealthDto Health { get; init; } = new(0, 0, 0, 0, 0, 0, "Sin peliculas");

    // Expone el valor ProbeResult usado por esta capa de la aplicacion.
    public AdminApiProbeResultDto? ProbeResult { get; init; }
}
