// Archivo: CRM.Proyecto\Contracts\ServiceContracts.cs
// Contratos de servicio que exponen los casos de uso consumidos por la capa web.

using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;

namespace CRM.Proyecto.Contracts;

// Define las operaciones disponibles para IAccountService.

public interface IAccountService
{
    // Declara la operacion LoginAsync que deben implementar las clases concretas.
    Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    // Declara la operacion RegisterAsync que deben implementar las clases concretas.
    Task<RegisterResultDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);

    // Declara la operacion ResetPasswordAsync que deben implementar las clases concretas.
    Task<AccountActionResultDto> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);

    // Declara la operacion GetByIdAsync que deben implementar las clases concretas.
    Task<AuthenticatedUserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IHomeService.
public interface IHomeService
{
    // Declara la operacion GetDashboardAsync que deben implementar las clases concretas.
    Task<HomeDashboardDto> GetDashboardAsync(int? userId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IMovieService.
public interface IMovieService
{
    // Declara la operacion GetCatalogAsync que deben implementar las clases concretas.
    Task<MovieCatalogDto> GetCatalogAsync(MovieFilterRequest request, int? userId, CancellationToken cancellationToken);

    // Declara la operacion GetDetailsAsync que deben implementar las clases concretas.
    Task<MovieDetailsDto?> GetDetailsAsync(int movieId, int? userId, CancellationToken cancellationToken);

    // Declara la operacion RateAsync que deben implementar las clases concretas.
    Task RateAsync(int movieId, int userId, int score, CancellationToken cancellationToken);

    // Declara la operacion ToggleWatchlistAsync que deben implementar las clases concretas.
    Task ToggleWatchlistAsync(int movieId, int userId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IAchievementService.
public interface IAchievementService
{
    // Declara la operacion GetAchievementsAsync que deben implementar las clases concretas.
    Task<IReadOnlyCollection<AchievementProgressDto>> GetAchievementsAsync(int userId, string? filter, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IChallengeService.
public interface IChallengeService
{
    // Declara la operacion GetChallengesAsync que deben implementar las clases concretas.
    Task<IReadOnlyCollection<UserChallengeDto>> GetChallengesAsync(int userId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IProfileService.
public interface IProfileService
{
    // Declara la operacion GetProfileAsync que deben implementar las clases concretas.
    Task<ProfileSummaryDto?> GetProfileAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion GetPublicProfileAsync que deben implementar las clases concretas.
    Task<PublicUserProfileDto?> GetPublicProfileAsync(int userId, CancellationToken cancellationToken);

    // Declara la operacion UpdateProfileAsync que deben implementar las clases concretas.
    Task<ProfileActionResultDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken);

    // Declara la operacion ChangePasswordAsync que deben implementar las clases concretas.
    Task<ProfileActionResultDto> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}


// Define las operaciones disponibles para ICommunityService.
public interface ICommunityService
{
    // Devuelve miembros visibles, metricas publicas y puntos de ranking.
    // Declara la operacion GetMembersAsync que deben implementar las clases concretas.
    Task<IReadOnlyCollection<CommunityMemberDto>> GetMembersAsync(int? excludedUserId, CancellationToken cancellationToken);
}

// Define las operaciones disponibles para IAdminImportService.
public interface IAdminImportService
{
    // Declara la operacion GetStatusAsync que deben implementar las clases concretas.
    Task<AdminImportStatusDto> GetStatusAsync(CancellationToken cancellationToken);

    // Declara la operacion GetCatalogHealthAsync que deben implementar las clases concretas.
    Task<AdminCatalogHealthDto> GetCatalogHealthAsync(CancellationToken cancellationToken);

    // Declara la operacion ProbeTmdbMovieAsync que deben implementar las clases concretas.
    Task<AdminApiProbeResultDto> ProbeTmdbMovieAsync(TmdbApiProbeRequest request, CancellationToken cancellationToken);

    // Declara la operacion StartTmdbImportAsync que deben implementar las clases concretas.
    Task<AdminImportLaunchResultDto> StartTmdbImportAsync(TmdbImportRequest request, string startedBy, CancellationToken cancellationToken);
}
