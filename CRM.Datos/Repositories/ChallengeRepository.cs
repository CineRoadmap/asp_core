// Archivo: CRM.Datos\Repositories\ChallengeRepository.cs
// Repositorio Dapper encargado del acceso a datos y consultas SQL de esta entidad.

using Dapper;
using CRM.Datos.Context;
using CRM.Datos.Seed;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;

namespace CRM.Datos.Repositories;


// Representa la responsabilidad de ChallengeRepository dentro de la aplicacion.

public sealed class ChallengeRepository : DapperRepositoryBase, IChallengeRepository
{
   
    // Inicializa ChallengeRepository con las dependencias necesarias.

    public ChallengeRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    // Asigna retos iniciales al usuario si todavia no tiene ninguno.
    public async Task AssignInitialChallengesAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        const string existingSql = "SELECT COUNT(*) FROM usuario_retos WHERE usuario_id = @UserId;";
        var existing = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(existingSql, new { UserId = userId }, cancellationToken: cancellationToken));
        if (existing > 0)
        {
            return;
        }

        const string sql = """
            SELECT id AS Id, tipo AS Type
            FROM catalogo_retos
            ORDER BY RAND()
            LIMIT 3;
            """;

        var selected = (await connection.QueryAsync<ChallengeSelectionRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken))).ToArray();

        foreach (var challenge in selected)
        {
            var assignedAt = DateTime.UtcNow;
            var dueDate = assignedAt.AddDays(challenge.Type switch
            {
                "DIARIO" => 1,
                "SEMANAL" => 7,
                _ => 30
            });

            const string insertSql = """
                INSERT IGNORE INTO usuario_retos (usuario_id, reto_id, estado, progreso_actual, fecha_fin)
                VALUES (@UserId, @ChallengeId, 'ACEPTADO', 0, @DueDateUtc);
                """;

            await connection.ExecuteAsync(new CommandDefinition(
                insertSql,
                new { UserId = userId, ChallengeId = challenge.Id, DueDateUtc = dueDate.Date },
                cancellationToken: cancellationToken));
        }

        await ChallengeProgressEngine.RefreshAsync(connection, userId, cancellationToken);
    }

    // Recalcula el progreso de los retos del usuario segun sus valoraciones actuales.
    public async Task RefreshProgressAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await ChallengeProgressEngine.RefreshAsync(connection, userId, cancellationToken);
    }

    // Devuelve los retos asignados al usuario con estado, vencimiento y progreso.
    public async Task<IReadOnlyCollection<UserChallengeDto>> GetChallengesAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await ChallengeProgressEngine.RefreshAsync(connection, userId, cancellationToken);

        const string sql = """
            SELECT c.id AS Id,
                   c.nombre AS Name,
                   c.descripcion AS Description,
                   c.tipo AS Type,
                   c.progreso_objetivo AS TargetProgress,
                   ur.progreso_actual AS CurrentProgress,
                   ur.estado AS Status,
                   COALESCE(ur.fecha_fin, CURRENT_DATE()) AS DueDateUtc
            FROM usuario_retos ur
            INNER JOIN catalogo_retos c ON c.id = ur.reto_id
            WHERE ur.usuario_id = @UserId
            ORDER BY ur.fecha_fin ASC;
            """;

        var challengeRows = await connection.QueryAsync<ChallengeRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return challengeRows.Select(challenge =>
        {
            const int pointsAvailable = 100;
            var boundedProgress = Math.Clamp(challenge.CurrentProgress, 0, Math.Max(challenge.TargetProgress, 1));
            var pointsEarned = challenge.Status == "COMPLETADO"
                ? pointsAvailable
                : boundedProgress * pointsAvailable / Math.Max(challenge.TargetProgress, 1);

            return new UserChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Description,
                FormatTypeLabel(challenge.Type),
                BuildRuleLabel(challenge.Name, challenge.Description),
                challenge.TargetProgress,
                challenge.CurrentProgress,
                pointsEarned,
                pointsAvailable,
                FormatStatusLabel(challenge.Status),
                challenge.DueDateUtc);
        }).ToArray();
    }

    // Traduce el tipo tecnico del reto a una etiqueta visible.
    private static string FormatTypeLabel(string value) => value switch
    {
        "DIARIO" => "Diario",
        "SEMANAL" => "Semanal",
        _ => "Tematico"
    };

    // Traduce el estado tecnico del reto a una etiqueta visible.
    private static string FormatStatusLabel(string value) => value switch
    {
        "COMPLETADO" => "Completado",
        "EXPIRADO" => "Expirado",
        _ => "Activo"
    };

    // Deduce una descripcion de regla a partir del texto del reto heredado.
    private static string BuildRuleLabel(string name, string description)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        if (text.Contains("comedia", StringComparison.Ordinal))
        {
            return "Genero: Comedia";
        }

        if (text.Contains("terror", StringComparison.Ordinal))
        {
            return "Genero: Terror";
        }

        if (text.Contains("accion", StringComparison.Ordinal) || text.Contains("acciÃ³n", StringComparison.Ordinal))
        {
            return "Genero: Accion";
        }

        if (text.Contains("director", StringComparison.Ordinal))
        {
            return "Objetivo: mismo director";
        }

        if (text.Contains("generos diferentes", StringComparison.Ordinal) || text.Contains("gÃ©neros diferentes", StringComparison.Ordinal))
        {
            return "Objetivo: generos distintos";
        }

        if (text.Contains("no habladas en espaÃ±ol", StringComparison.Ordinal) || text.Contains("no habladas en espanol", StringComparison.Ordinal))
        {
            return "Idioma: distinto de espanol";
        }

        if (text.Contains("70", StringComparison.Ordinal) || text.Contains("80", StringComparison.Ordinal))
        {
            return "Periodo: anos 70 u 80";
        }

        if (text.Contains("documental", StringComparison.Ordinal))
        {
            return "Genero: Documental";
        }

        return "Progreso general";
    }

    private sealed class ChallengeSelectionRow
    {
       
        // Expone el valor Id usado por esta capa de la aplicacion.
    
        public int Id { get; init; }
       
        // Expone el valor Type usado por esta capa de la aplicacion.
    
        public string Type { get; init; } = string.Empty;
    }

    private sealed class ChallengeRow
    {
       
        // Expone el valor Id usado por esta capa de la aplicacion.
    
        public int Id { get; init; }
       
        // Expone el valor Name usado por esta capa de la aplicacion.
    
        public string Name { get; init; } = string.Empty;
       
        // Expone el valor Description usado por esta capa de la aplicacion.
    
        public string Description { get; init; } = string.Empty;
       
        // Expone el valor Type usado por esta capa de la aplicacion.
    
        public string Type { get; init; } = string.Empty;
       
        // Expone el valor TargetProgress usado por esta capa de la aplicacion.
    
        public int TargetProgress { get; init; }
       
        // Expone el valor CurrentProgress usado por esta capa de la aplicacion.
    
        public int CurrentProgress { get; init; }
       
        // Expone el valor Status usado por esta capa de la aplicacion.
    
        public string Status { get; init; } = string.Empty;
       
        // Expone el valor DueDateUtc usado por esta capa de la aplicacion.
    
        public DateTime DueDateUtc { get; init; }
    }
}
