// Archivo: CRM.Control\Services\MovieService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;

namespace CRM.Control.Services;

// Representa la responsabilidad de MovieService dentro de la aplicacion.
public sealed class MovieService : IMovieService
{
    // Guarda la dependencia _movies recibida por inyeccion.
    private readonly IMovieRepository _movies;

    // Guarda la dependencia _achievements recibida por inyeccion.
    private readonly IAchievementRepository _achievements;

    // Guarda la dependencia _challenges recibida por inyeccion.
    private readonly IChallengeRepository _challenges;

    // Inicializa MovieService con las dependencias necesarias.
    public MovieService(
        IMovieRepository movies,
        IAchievementRepository achievements,
        IChallengeRepository challenges)
    {
        _movies = movies;
        _achievements = achievements;
        _challenges = challenges;
    }

    // Obtiene el catalogo paginado aplicando filtros y contexto de usuario si existe sesion.
    public Task<MovieCatalogDto> GetCatalogAsync(MovieFilterRequest request, int? userId, CancellationToken cancellationToken) =>
        _movies.GetCatalogAsync(request, userId, cancellationToken);

    // Devuelve la ficha completa de una pelicula, incluyendo estado personal como lista y valoracion.
    public Task<MovieDetailsDto?> GetDetailsAsync(int movieId, int? userId, CancellationToken cancellationToken) =>
        _movies.GetDetailsAsync(movieId, userId, cancellationToken);

    // Valida la puntuacion, guarda la valoracion y recalcula logros y retos del usuario.
    public async Task RateAsync(int movieId, int userId, int score, CancellationToken cancellationToken)
    {
        if (score is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "La puntuaciÃ³n debe estar entre 1 y 5.");
        }

        var exists = await _movies.MovieExistsAsync(movieId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("La pelÃ­cula no existe.");
        }

        await _movies.RateAsync(movieId, userId, score, cancellationToken);
        await _achievements.RefreshProgressAsync(userId, cancellationToken);
        await _challenges.RefreshProgressAsync(userId, cancellationToken);
    }

    // Alterna la pelicula dentro de la lista de pendientes del usuario autenticado.
    public async Task ToggleWatchlistAsync(int movieId, int userId, CancellationToken cancellationToken)
    {
        var exists = await _movies.MovieExistsAsync(movieId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("La pelÃ­cula no existe.");
        }

        await _movies.ToggleWatchlistAsync(movieId, userId, cancellationToken);
    }
}
