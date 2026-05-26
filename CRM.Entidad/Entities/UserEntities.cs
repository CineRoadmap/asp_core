// Archivo: CRM.Entidad\Entities\UserEntities.cs
// Entidades de dominio que representan los datos principales persistidos por la aplicacion.

using CRM.Entidad.Enums;

namespace CRM.Entidad.Entities;

// Representa la responsabilidad de User dentro de la aplicacion
public sealed class User
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor UserName usado por esta capa de la aplicacion.
    public string UserName { get; set; } = string.Empty;

    // Expone el valor NickName usado por esta capa de la aplicacion.
    public string NickName { get; set; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    public string Email { get; set; } = string.Empty;

    // Expone el valor Phone usado por esta capa de la aplicacion.
    public string Phone { get; set; } = string.Empty;

    // Expone el valor PasswordHash usado por esta capa de la aplicacion.
    public string PasswordHash { get; set; } = string.Empty;

    // Expone el valor CreatedAtUtc usado por esta capa de la aplicacion.
    public DateTime CreatedAtUtc { get; set; }
}

// Representa la responsabilidad de Badge dentro de la aplicacion.
public sealed class Badge
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Name usado por esta capa de la aplicacion.
    public string Name { get; set; } = string.Empty;

    // Expone el valor ImagePath usado por esta capa de la aplicacion.
    public string ImagePath { get; set; } = string.Empty;
}

// Representa la responsabilidad de Achievement dentro de la aplicacion.
public sealed class Achievement
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Title usado por esta capa de la aplicacion.
    public string Title { get; set; } = string.Empty;

    // Expone el valor Description usado por esta capa de la aplicacion.

    public string Description { get; set; } = string.Empty;

    // Expone el valor Objective usado por esta capa de la aplicacion.
    public int Objective { get; set; }

    // Expone el valor BadgeId usado por esta capa de la aplicacion.
    public int BadgeId { get; set; }

    // Expone el valor RequirementType usado por esta capa de la aplicacion.
    public RuleType RequirementType { get; set; }

    // Expone el valor RequirementValue usado por esta capa de la aplicacion.
    public string RequirementValue { get; set; } = string.Empty;
}

// Representa la responsabilidad de UserAchievement dentro de la aplicacion.
public sealed class UserAchievement
{
    // Expone el valor UserId usado por esta capa de la aplicacion.
    public int UserId { get; set; }

    // Expone el valor AchievementId usado por esta capa de la aplicacion.
    public int AchievementId { get; set; }

    // Expone el valor Progress usado por esta capa de la aplicacion.
    public int Progress { get; set; }

    // Expone el valor Completed usado por esta capa de la aplicacion.
    public bool Completed { get; set; }
}

// Representa la responsabilidad de Challenge dentro de la aplicacion.
public sealed class Challenge
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Name usado por esta capa de la aplicacion.
    public string Name { get; set; } = string.Empty;

    // Expone el valor Description usado por esta capa de la aplicacion.
    public string Description { get; set; } = string.Empty;

    // Expone el valor Type usado por esta capa de la aplicacion.
    public ChallengeType Type { get; set; }

    // Expone el valor RuleType usado por esta capa de la aplicacion.
    public RuleType RuleType { get; set; }

    // Expone el valor RuleValue usado por esta capa de la aplicacion.
    public string RuleValue { get; set; } = string.Empty;

    // Expone el valor TargetProgress usado por esta capa de la aplicacion.
    public int TargetProgress { get; set; }
}

// Representa la responsabilidad de UserChallenge dentro de la aplicacion.
public sealed class UserChallenge
{
    // Expone el valor UserId usado por esta capa de la aplicacion.
    public int UserId { get; set; }

    // Expone el valor ChallengeId usado por esta capa de la aplicacion.
    public int ChallengeId { get; set; }

    // Expone el valor Status usado por esta capa de la aplicacion.
    public ChallengeStatus Status { get; set; }

    // Expone el valor CurrentProgress usado por esta capa de la aplicacion.
    public int CurrentProgress { get; set; }

    // Expone el valor AssignedAtUtc usado por esta capa de la aplicacion.
    public DateTime AssignedAtUtc { get; set; }

    // Expone el valor DueDateUtc usado por esta capa de la aplicacion.
    public DateTime DueDateUtc { get; set; }
}
