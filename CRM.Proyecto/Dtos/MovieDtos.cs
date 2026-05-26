// Archivo: CRM.Proyecto\Dtos\MovieDtos.cs
// DTOs utilizados para mover datos entre capas sin exponer directamente las entidades.
namespace CRM.Proyecto.Dtos;


// Transporta los datos de MovieCardDto entre capas.
public sealed record MovieCardDto(
    int Id,
    string Title,
    int Year,
    string PosterPath,
    string GenreNames,
    double AverageScore,
    int RatingCount);


// Transporta los datos de MovieDetailsDto entre capas.
public sealed record MovieDetailsDto(
    int Id,
    string Title,
    int Year,
    int DurationMinutes,
    string OriginalLanguage,
    string Synopsis,
    string PosterPath,
    double AverageScore,
    IReadOnlyCollection<string> Genres,
    IReadOnlyCollection<string> Directors,
    IReadOnlyCollection<string> Actors,
    bool IsInWatchlist,
    int? UserScore);


// Transporta los datos de MovieCatalogDto entre capas.
public sealed record MovieCatalogDto(
    PagedResult<MovieCardDto> Movies,
    IReadOnlyCollection<GenreDto> Genres,
    string Search,
    int? GenreId,
    int? Year,
    string ViewMode);


// Transporta los datos de HomeDashboardDto entre capas.
public sealed record HomeDashboardDto(
    string DisplayName,
    bool IsAuthenticated,
    MovieCardDto? FeaturedMovie,
    MovieCardDto? DailyPick,
    IReadOnlyCollection<MovieCardDto> WeeklyRecommendations,
    IReadOnlyCollection<MovieCardDto> TopMovies);
