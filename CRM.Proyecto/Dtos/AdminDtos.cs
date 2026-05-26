// Archivo: CRM.Proyecto\Dtos\AdminDtos.cs
// DTOs utilizados para mover datos entre capas sin exponer directamente las entidades.
namespace CRM.Proyecto.Dtos;


// Transporta los datos de TmdbImportProgressDto entre capas.
public sealed record TmdbImportProgressDto(
    string Stage,
    int TotalPages,
    int CurrentPage,
    int ProcessedMovies,
    int ImportedMovies,
    int SkippedMovies,
    int FailedMovies,
    string Message);


// Transporta los datos de AdminImportStatusDto entre capas.
public sealed record AdminImportStatusDto(
    bool IsRunning,
    string Stage,
    string Message,
    string StartedBy,
    DateTime? StartedAtUtc,
    DateTime? FinishedAtUtc,
    int TotalPages,
    int CurrentPage,
    int ProcessedMovies,
    int ImportedMovies,
    int SkippedMovies,
    int FailedMovies,
    string LastError);


// Transporta los datos de AdminImportLaunchResultDto entre capas.
public sealed record AdminImportLaunchResultDto(
    bool Started,
    string Message);


// Transporta los datos de AdminCatalogHealthDto entre capas.
public sealed record AdminCatalogHealthDto(
    int MovieCount,
    int GenreCount,
    int DirectorCount,
    int ActorCount,
    int RatingCount,
    int WatchlistCount,
    string LastMovieTitle);


// Transporta los datos de AdminApiProbeResultDto entre capas.
public sealed record AdminApiProbeResultDto(
    bool Succeeded,
    string Provider,
    string Message,
    int TmdbId,
    string Title,
    string OriginalTitle,
    string ReleaseDate,
    int RuntimeMinutes,
    string OriginalLanguage,
    string PosterPath,
    string ImdbId,
    string ImdbUrl,
    IReadOnlyCollection<string> Genres,
    IReadOnlyCollection<string> Directors,
    IReadOnlyCollection<string> Actors);
