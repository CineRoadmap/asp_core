// Archivo: CRM.Proyecto\Requests\TmdbImportRequest.cs
// Peticion con parametros para controlar una importacion desde TMDB.
namespace CRM.Proyecto.Requests;


// Representa la responsabilidad de TmdbImportRequest dentro de la aplicacion.
public sealed class TmdbImportRequest
{
    // Expone el valor ApiKey usado por esta capa de la aplicacion.
    public string ApiKey { get; init; } = string.Empty;

    // Expone el valor Language usado por esta capa de la aplicacion.
    public string Language { get; init; } = "es-ES";

    // Expone el valor TotalPages usado por esta capa de la aplicacion.
    public int TotalPages { get; init; } = 500;

    // Expone el valor DelayMsBetweenPages usado por esta capa de la aplicacion.
    public int DelayMsBetweenPages { get; init; } = 200;

    // Expone el valor SkipWithoutPoster usado por esta capa de la aplicacion.
    public bool SkipWithoutPoster { get; init; } = true;

    // Expone el valor SkipWithoutOverview usado por esta capa de la aplicacion.
    public bool SkipWithoutOverview { get; init; } = true;
}


// Representa la responsabilidad de TmdbApiProbeRequest dentro de la aplicacion.
public sealed class TmdbApiProbeRequest
{
    // Expone el valor ApiKey usado por esta capa de la aplicacion.
    public string ApiKey { get; init; } = string.Empty;

    // Expone el valor MovieId usado por esta capa de la aplicacion.
    public int MovieId { get; init; } = 550;

    // Expone el valor Language usado por esta capa de la aplicacion.
    public string Language { get; init; } = "es-ES";
}
