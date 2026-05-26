// Archivo: CRM.Proyecto\Contracts\RepositoryContracts.cs
// Contratos de repositorio que definen las operaciones de persistencia disponibles.

using CRM.Entidad.Entities;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;

namespace CRM.Proyecto.Contracts;

// Define las operaciones disponibles para IUserRepository.
public interface IUserRepository
{
    // Declara la operacion GetByUserNameAsync que deben implementar las clases concretas.
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);

    // Declara la operacion GetByIdAsync que deben implementar las clases concretas.
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion ExistsAsync que deben implementar las clases concretas.
    Task<bool> ExistsAsync(string userName, string email, CancellationToken cancellationToken);

    // Declara la operacion CreateAsync que deben implementar las clases concretas.
    Task<int> CreateAsync(User user, CancellationToken cancellationToken);

    // Declara la operacion GetProfileAsync que deben implementar las clases concretas.
    Task<ProfileSummaryDto?> GetProfileAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion GetPublicProfileAsync que deben implementar las clases concretas.
    Task<PublicUserProfileDto?> GetPublicProfileAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion IsEmailUsedByAnotherUserAsync que deben implementar las clases concretas.
    Task<bool> IsEmailUsedByAnotherUserAsync(int userId, string email, CancellationToken cancellationToken);

    // Declara la operacion UpdateProfileAsync que deben implementar las clases concretas.
    Task UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken);

    // Declara la operacion UpdatePasswordAsync que deben implementar las clases concretas.
    Task UpdatePasswordAsync(int userId, string passwordHash, CancellationToken cancellationToken);

    // Obtiene los miembros que se mostraran en la pantalla de comunidad con su ranking.
    Task<IReadOnlyCollection<CommunityMemberDto>> GetCommunityMembersAsync(int? excludedUserId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IMovieRepository.
public interface IMovieRepository
{
    // Declara la operacion GetDashboardAsync que deben implementar las clases concretas.
    Task<HomeDashboardDto> GetDashboardAsync(int? userId, CancellationToken cancellationToken);

    // Declara la operacion GetCatalogAsync que deben implementar las clases concretas.
    Task<MovieCatalogDto> GetCatalogAsync(MovieFilterRequest request, int? userId, CancellationToken cancellationToken);

    // Declara la operacion GetDetailsAsync que deben implementar las clases concretas.
    Task<MovieDetailsDto?> GetDetailsAsync(int movieId, int? userId, CancellationToken cancellationToken);

    // Declara la operacion RateAsync que deben implementar las clases concretas.
    Task RateAsync(int movieId, int userId, int score, CancellationToken cancellationToken);

    // Declara la operacion ToggleWatchlistAsync que deben implementar las clases concretas.
    Task ToggleWatchlistAsync(int movieId, int userId, CancellationToken cancellationToken);

    // Declara la operacion MovieExistsAsync que deben implementar las clases concretas.
    Task<bool> MovieExistsAsync(int movieId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IAchievementRepository.
public interface IAchievementRepository
{

    // Declara la operacion EnsureUserProgressRowsAsync que deben implementar las clases concretas.
    Task EnsureUserProgressRowsAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion RefreshProgressAsync que deben implementar las clases concretas.
    Task RefreshProgressAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AchievementProgressDto>> GetAchievementsAsync(int userId, string? filter, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IChallengeRepository.
public interface IChallengeRepository
{
    // Declara la operacion AssignInitialChallengesAsync que deben implementar las clases concretas.
    Task AssignInitialChallengesAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion RefreshProgressAsync que deben implementar las clases concretas.
    Task RefreshProgressAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserChallengeDto>> GetChallengesAsync(int userId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IDatabaseInitializer.
public interface IDatabaseInitializer
{
    // Declara la operacion InitializeAsync que deben implementar las clases concretas.
    Task InitializeAsync(CancellationToken cancellationToken);
}
