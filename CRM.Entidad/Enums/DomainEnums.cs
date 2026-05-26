// Archivo: CRM.Entidad\Enums\DomainEnums.cs
// Enumeraciones de dominio usadas para clasificar estados, niveles y tipos dentro de la aplicacion.

namespace CRM.Entidad.Enums;

// Agrupa los valores permitidos para RuleType.
public enum RuleType
{
    General = 0,
    Genre = 1,
    Director = 2,
    Actor = 3,
    Year = 4,
    Language = 5
}

// Agrupa los valores permitidos para ChallengeType.
public enum ChallengeType
{
    Daily = 0,
    Weekly = 1,
    Thematic = 2
}

// Agrupa los valores permitidos para ChallengeStatus.
public enum ChallengeStatus
{
    Accepted = 0,
    Completed = 1,
    Expired = 2
}
