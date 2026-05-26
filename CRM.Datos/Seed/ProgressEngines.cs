// Archivo: CRM.Datos\Seed\ProgressEngines.cs
// Motor de calculo de progreso, retos e insignias a partir de las peliculas vistas.

using Dapper;
using MySqlConnector;

namespace CRM.Datos.Seed;

// Representa la responsabilidad de AchievementProgressEngine dentro de la aplicacion.

internal static class AchievementProgressEngine
{
    // Recalcula todos los logros de un usuario y actualiza progreso/completado en MySQL.
    internal static async Task RefreshAsync(MySqlConnection connection, int userId, CancellationToken cancellationToken)
    {
        const string ensureSql = """
            INSERT IGNORE INTO logros_usuario (idUsuario, idLogro, progreso, completado)
            SELECT @UserId, l.idLogro, 0, 0
            FROM logros l;
            """;

        await connection.ExecuteAsync(new CommandDefinition(ensureSql, new { UserId = userId }, cancellationToken: cancellationToken));

        const string achievementsSql = """
            SELECT idLogro AS Id,
                nombreReto AS Title,
                COALESCE(descripcion, '') AS Description,
                objetivo AS Objective,
                tipo_requisito AS RequirementType,
                COALESCE(valor_requisito, '') AS RequirementValue
            FROM logros;
            """;

        var achievements = await connection.QueryAsync<AchievementProgressSeedRow>(
            new CommandDefinition(achievementsSql, cancellationToken: cancellationToken));

        foreach (var achievement in achievements)
        {
            var progress = await CountProgressAsync(
                connection,
                userId,
                achievement.Title,
                achievement.Description,
                achievement.RequirementType,
                achievement.RequirementValue,
                null,
                null,
                cancellationToken);

            const string updateSql = """
                UPDATE logros_usuario
                SET progreso = @Progress,
                    completado = @Completed
                WHERE idUsuario = @UserId AND idLogro = @AchievementId;
                """;

            await connection.ExecuteAsync(new CommandDefinition(
                updateSql,
                new
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    Progress = progress,
                    Completed = progress >= achievement.Objective
                },
                cancellationToken: cancellationToken));
        }
    }

    // Cuenta cuantas acciones del usuario cumplen una regla concreta de logro o reto.
    internal static async Task<int> CountProgressAsync(
        MySqlConnection connection,
        int userId,
        string title,
        string description,
        string requirementType,
        string requirementValue,
        DateTime? assignedAtUtc,
        DateTime? dueDateUtc,
        CancellationToken cancellationToken)
    {
        var timeClause = BuildTimeClause(assignedAtUtc, dueDateUtc);

        object parameters = new
        {
            UserId = userId,
            RequirementValue = requirementValue,
            GeneralMetric = ResolveGeneralMetric(title, description, requirementValue),
            AssignedAtUtc = assignedAtUtc,
            DueDateUtc = dueDateUtc
        };

        string sql = requirementType switch
        {
            "general" => BuildGeneralProgressSql(timeClause),
            "genero" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN pelicula_generos pg ON pg.pelicula_id = v.pelicula_id
                INNER JOIN generos g ON g.id = pg.genero_id
                WHERE v.usuario_id = @UserId AND g.nombre = @RequirementValue{timeClause};
                """,
            "director" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN pelicula_directores pd ON pd.pelicula_id = v.pelicula_id
                INNER JOIN directores d ON d.id = pd.director_id
                WHERE v.usuario_id = @UserId AND d.nombre = @RequirementValue{timeClause};
                """,
            "actor" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN pelicula_actores pa ON pa.pelicula_id = v.pelicula_id
                INNER JOIN actores a ON a.id = pa.actor_id
                WHERE v.usuario_id = @UserId AND a.nombre = @RequirementValue{timeClause};
                """,
            "anio" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN peliculas p ON p.id = v.pelicula_id
                WHERE v.usuario_id = @UserId AND p.anio = CAST(@RequirementValue AS SIGNED){timeClause};
                """,
            "genre_documentary" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN pelicula_generos pg ON pg.pelicula_id = v.pelicula_id
                INNER JOIN generos g ON g.id = pg.genero_id
                WHERE v.usuario_id = @UserId AND g.nombre = 'Documental'{timeClause};
                """,
            "year_range_70_80" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN peliculas p ON p.id = v.pelicula_id
                WHERE v.usuario_id = @UserId AND p.anio BETWEEN 1970 AND 1989{timeClause};
                """,
            "same_director" => $"""
                SELECT COALESCE(MAX(DirectorCount), 0)
                FROM (
                    SELECT COUNT(*) AS DirectorCount
                    FROM valoraciones v
                    INNER JOIN pelicula_directores pd ON pd.pelicula_id = v.pelicula_id
                    WHERE v.usuario_id = @UserId{timeClause}
                    GROUP BY pd.director_id
                ) directors;
                """,
            "distinct_genres" => $"""
                SELECT COUNT(DISTINCT pg.genero_id)
                FROM valoraciones v
                INNER JOIN pelicula_generos pg ON pg.pelicula_id = v.pelicula_id
                WHERE v.usuario_id = @UserId{timeClause};
                """,
            "language_non_spanish" => $"""
                SELECT COUNT(*)
                FROM valoraciones v
                INNER JOIN peliculas p ON p.id = v.pelicula_id
                WHERE v.usuario_id = @UserId
                AND COALESCE(p.lenguaje_orig, '') NOT IN ('es', 'esp', 'spa')
                {timeClause};
                """,
            _ => "SELECT 0;"
        };

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    // Construye el SQL para objetivos generales como valoraciones, retos completados o insignias.
    private static string BuildGeneralProgressSql(string timeClause) => $@"
        SELECT CASE @GeneralMetric
            WHEN 'ratings' THEN (
                SELECT COUNT(*)
                FROM valoraciones v
                WHERE v.usuario_id = @UserId{timeClause}
            )
            WHEN 'completed_challenges' THEN (
                SELECT COUNT(*)
                FROM usuario_retos
                WHERE usuario_id = @UserId AND estado = 'COMPLETADO'
            )
            WHEN 'completed_achievements' THEN (
                SELECT COUNT(*)
                FROM logros_usuario
                WHERE idUsuario = @UserId AND completado = 1
            )
            ELSE (
                SELECT COUNT(*)
                FROM valoraciones v
                WHERE v.usuario_id = @UserId{timeClause}
            )
        END;
        ";

    // Genera la condicion temporal cuando una regla solo debe contar dentro de un plazo.
    private static string BuildTimeClause(DateTime? assignedAtUtc, DateTime? dueDateUtc)
    {
        if (assignedAtUtc.HasValue && dueDateUtc.HasValue)
        {
            return " AND v.fecha_registro >= @AssignedAtUtc AND v.fecha_registro < DATE_ADD(@DueDateUtc, INTERVAL 1 DAY)";
        }

        if (dueDateUtc.HasValue)
        {
            return " AND v.fecha_registro < DATE_ADD(@DueDateUtc, INTERVAL 1 DAY)";
        }

        return string.Empty;
    }

    // Decide que metrica general debe contarse leyendo titulo, descripcion y valor de requisito.
    private static string ResolveGeneralMetric(string title, string description, string requirementValue)
    {
        if (!string.IsNullOrWhiteSpace(requirementValue))
        {
            return requirementValue;
        }

        var text = $"{title} {description}".ToLowerInvariant();
        if (text.Contains("valoracion", StringComparison.Ordinal) || text.Contains("critico", StringComparison.Ordinal))
        {
            return "ratings";
        }

        if (text.Contains("insign", StringComparison.Ordinal) || text.Contains("erudito", StringComparison.Ordinal))
        {
            return "completed_achievements";
        }

        if (text.Contains("reto", StringComparison.Ordinal))
        {
            return "completed_challenges";
        }

        return "views";
    }

    private sealed class AchievementProgressSeedRow
    {
        // Expone el valor Id usado por esta capa de la aplicacion.
        public int Id { get; init; }

        // Expone el valor Title usado por esta capa de la aplicacion.
        public string Title { get; init; } = string.Empty;

        // Expone el valor Description usado por esta capa de la aplicacion.
        public string Description { get; init; } = string.Empty;

        // Expone el valor Objective usado por esta capa de la aplicacion.
        public int Objective { get; init; }

        // Expone el valor RequirementType usado por esta capa de la aplicacion.
        public string RequirementType { get; init; } = string.Empty;

        // Expone el valor RequirementValue usado por esta capa de la aplicacion.
        public string RequirementValue { get; init; } = string.Empty;
    }
}

// Representa la responsabilidad de ChallengeProgressEngine dentro de la aplicacion.
internal static class ChallengeProgressEngine
{
    // Recalcula los retos asignados al usuario y marca completados o expirados segun corresponda.
    internal static async Task RefreshAsync(MySqlConnection connection, int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT ur.usuario_id AS UserId,
                ur.reto_id AS ChallengeId,
                c.nombre AS Name,
                COALESCE(c.descripcion, '') AS Description,
                c.tipo AS Type,
                COALESCE(ur.fecha_fin, CURRENT_DATE()) AS DueDateUtc,
                c.progreso_objetivo AS TargetProgress
            FROM usuario_retos ur
            INNER JOIN catalogo_retos c ON c.id = ur.reto_id
            WHERE ur.usuario_id = @UserId;
            """;

        var assignedChallenges = await connection.QueryAsync<ChallengeSeedRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        foreach (var challenge in assignedChallenges)
        {
            var progress = await AchievementProgressEngine.CountProgressAsync(
                connection,
                userId,
                challenge.Name,
                challenge.Description,
                ResolveRuleType(challenge.Name, challenge.Description),
                ResolveRuleValue(challenge.Name, challenge.Description),
                null,
                challenge.DueDateUtc,
                cancellationToken);

            var status = progress >= challenge.TargetProgress
                ? "COMPLETADO"
                : challenge.DueDateUtc.Date < DateTime.UtcNow.Date
                    ? "EXPIRADO"
                    : "ACEPTADO";

            const string updateSql = """
                UPDATE usuario_retos
                SET progreso_actual = @Progress,
                    estado = @Status
                WHERE usuario_id = @UserId AND reto_id = @ChallengeId;
                """;

            await connection.ExecuteAsync(new CommandDefinition(
                updateSql,
                new
                {
                    UserId = userId,
                    ChallengeId = challenge.ChallengeId,
                    Progress = progress,
                    Status = status
                },
                cancellationToken: cancellationToken));
        }
    }

    // Deduce el tipo de regla de un reto a partir de su nombre y descripcion heredados.
    private static string ResolveRuleType(string name, string description)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        if (text.Contains("comedia", StringComparison.Ordinal))
        {
            return "genero";
        }

        if (text.Contains("terror", StringComparison.Ordinal))
        {
            return "genero";
        }

        if (text.Contains("accion", StringComparison.Ordinal) || text.Contains("acciÃ³n", StringComparison.Ordinal))
        {
            return "genero";
        }

        if (text.Contains("documental", StringComparison.Ordinal))
        {
            return "genre_documentary";
        }

        if (text.Contains("director", StringComparison.Ordinal))
        {
            return "same_director";
        }

        if (text.Contains("generos diferentes", StringComparison.Ordinal) || text.Contains("gÃ©neros diferentes", StringComparison.Ordinal))
        {
            return "distinct_genres";
        }

        if (text.Contains("70", StringComparison.Ordinal) || text.Contains("80", StringComparison.Ordinal))
        {
            return "year_range_70_80";
        }

        if (text.Contains("no habladas en espaÃ±ol", StringComparison.Ordinal) || text.Contains("no habladas en espanol", StringComparison.Ordinal))
        {
            return "language_non_spanish";
        }

        return "general";
    }

    // Deduce el valor concreto de la regla de un reto a partir de su texto.
    private static string ResolveRuleValue(string name, string description)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        if (text.Contains("comedia", StringComparison.Ordinal))
        {
            return "Comedia";
        }

        if (text.Contains("terror", StringComparison.Ordinal))
        {
            return "Terror";
        }

        if (text.Contains("accion", StringComparison.Ordinal) || text.Contains("acciÃ³n", StringComparison.Ordinal))
        {
            return "Accion";
        }

        return "views";
    }

    private sealed class ChallengeSeedRow
    {
        // Expone el valor UserId usado por esta capa de la aplicacion.
        public int UserId { get; init; }

        // Expone el valor ChallengeId usado por esta capa de la aplicacion.
        public int ChallengeId { get; init; }

        // Expone el valor Name usado por esta capa de la aplicacion.
        public string Name { get; init; } = string.Empty;

        // Expone el valor Description usado por esta capa de la aplicacion.
        public string Description { get; init; } = string.Empty;

        // Expone el valor Type usado por esta capa de la aplicacion.
        public string Type { get; init; } = string.Empty;

        // Expone el valor DueDateUtc usado por esta capa de la aplicacion.
        public DateTime DueDateUtc { get; init; }

        // Expone el valor TargetProgress usado por esta capa de la aplicacion.    
        public int TargetProgress { get; init; }
    }
}
