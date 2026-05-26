// Archivo: CRM.Proyecto\Dtos\ProfileDtos.cs
// DTOs utilizados para mover datos entre capas sin exponer directamente las entidades.

namespace CRM.Proyecto.Dtos;


// Transporta los datos de AchievementProgressDto entre capas.
public sealed record AchievementProgressDto(
    int Id,
    string Title,
    string Description,
    string BadgeImagePath,
    string RequirementLabel,
    int Objective,
    int Progress,
    bool Completed);


// Transporta los datos de UserChallengeDto entre capas.
public sealed record UserChallengeDto(
    int Id,
    string Name,
    string Description,
    string TypeLabel,
    string RuleLabel,
    int TargetProgress,
    int CurrentProgress,
    int PointsEarned,
    int PointsAvailable,
    string StatusLabel,
    DateTime DueDateUtc);


// Transporta los datos de ActivityPointDto entre capas.
public sealed record ActivityPointDto(
    string Label,
    int Count);


// Transporta los datos de ProfileSummaryDto entre capas.
public sealed record ProfileSummaryDto(
    int Id,
    string UserName,
    string NickName,
    string Email,
    string Phone,
    int TotalMoviesWatched,
    int TotalRatings,
    int TotalMinutes,
    int AverageDuration,
    int UnlockedBadges,
    IReadOnlyCollection<ActivityPointDto> MonthlyActivity,
    IReadOnlyCollection<AchievementProgressDto> UnlockedAchievements);


// Transporta los datos de CommunityMemberDto entre capas.
public sealed record CommunityMemberDto(
    int Id,
    string UserName,
    string NickName,
    int RankingPoints,
    int CompletedChallenges,
    int ChallengePoints,
    int MoviePoints,
    int MoviesWatched,
    int RatingsCount,
    int BadgesUnlocked);


// Transporta los datos de PublicUserProfileDto entre capas.
public sealed record PublicUserProfileDto(
    ProfileSummaryDto Summary,
    IReadOnlyCollection<MovieCardDto> WatchedMovies);
