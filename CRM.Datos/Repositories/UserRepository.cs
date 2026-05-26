// Archivo: CRM.Datos\Repositories\UserRepository.cs
// Repositorio Dapper encargado del acceso a datos y consultas SQL de esta entidad.

using Dapper;
using CRM.Datos.Context;
using CRM.Datos.Seed;
using CRM.Entidad.Entities;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;
using MySqlConnector;

namespace CRM.Datos.Repositories;

// Representa la responsabilidad de UserRepository dentro de la aplicacion.

public sealed class UserRepository : DapperRepositoryBase, IUserRepository
{
    // Expone el valor _passwordColumnName usado por esta capa de la aplicacion.
    private string? _passwordColumnName;

    // Inicializa UserRepository con las dependencias necesarias.
    public UserRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    // Busca un usuario por nombre para validar el login.
    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var passwordColumn = await GetPasswordColumnNameAsync(connection, cancellationToken);
        var sql = $"""
            SELECT idUsuario AS Id,
                   usuario AS UserName,
                   nick AS NickName,
                   email AS Email,
                   telefono AS Phone,
                   {passwordColumn} AS PasswordHash,
                   UTC_TIMESTAMP() AS CreatedAtUtc
            FROM usuarios
            WHERE usuario = @UserName;
            """;

        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { UserName = userName }, cancellationToken: cancellationToken));
    }

    // Recupera los datos completos de un usuario a partir de su identificador interno.
    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var passwordColumn = await GetPasswordColumnNameAsync(connection, cancellationToken);
        var sql = $"""
            SELECT idUsuario AS Id,
                   usuario AS UserName,
                   nick AS NickName,
                   email AS Email,
                   telefono AS Phone,
                   {passwordColumn} AS PasswordHash,
                   UTC_TIMESTAMP() AS CreatedAtUtc
            FROM usuarios
            WHERE idUsuario = @UserId;
            """;

        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    // Comprueba si ya existe un usuario o email para evitar duplicados en el registro.
    public async Task<bool> ExistsAsync(string userName, string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM usuarios
            WHERE usuario = @UserName OR email = @Email;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UserName = userName, Email = email }, cancellationToken: cancellationToken));
        return count > 0;
    }

    // Inserta un usuario nuevo y devuelve el id generado por MySQL.
    public async Task<int> CreateAsync(User user, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var passwordColumn = await GetPasswordColumnNameAsync(connection, cancellationToken);
        var sql = $"""
            INSERT INTO usuarios (usuario, nick, {passwordColumn}, email, telefono)
            VALUES (@UserName, @NickName, @PasswordHash, @Email, @Phone);
            SELECT LAST_INSERT_ID();
            """;

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    // Compone el resumen del perfil con estadisticas, actividad mensual y logros completados.
    public async Task<ProfileSummaryDto?> GetProfileAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await AchievementProgressEngine.RefreshAsync(connection, userId, cancellationToken);
        await ChallengeProgressEngine.RefreshAsync(connection, userId, cancellationToken);

        const string sql = """
            SELECT idUsuario AS Id,
            usuario AS UserName,
            nick AS NickName,
            email AS Email,
            telefono AS Phone
            FROM usuarios
            WHERE idUsuario = @UserId;

            SELECT COUNT(v.id) AS TotalMoviesWatched,
            COUNT(v.id) AS TotalRatings,
            COALESCE(SUM(p.duracion), 0) AS TotalMinutes
            FROM valoraciones v
            INNER JOIN peliculas p ON p.id = v.pelicula_id
            WHERE v.usuario_id = @UserId;

            SELECT DATE_FORMAT(fecha_registro, '%b %Y') AS Label,
            COUNT(*) AS Count
            FROM valoraciones
            WHERE usuario_id = @UserId
            GROUP BY YEAR(fecha_registro), MONTH(fecha_registro), DATE_FORMAT(fecha_registro, '%b %Y')
            ORDER BY YEAR(fecha_registro), MONTH(fecha_registro);

            SELECT l.idLogro AS Id,
            l.nombreReto AS Title,
            l.descripcion AS Description,
            i.srcImagen AS BadgeImagePath,
            l.tipo_requisito AS RequirementType,
            COALESCE(l.valor_requisito, '') AS RequirementValue,
            l.objetivo AS Objective,
            lu.progreso AS Progress,
            CAST(lu.completado AS UNSIGNED) AS CompletedFlag
            FROM logros_usuario lu
            INNER JOIN logros l ON l.idLogro = lu.idLogro
            INNER JOIN insignias i ON i.idInsignia = l.idInsignia
            WHERE lu.idUsuario = @UserId AND lu.completado = 1
            ORDER BY l.idLogro;
            """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        var user = await multi.ReadSingleOrDefaultAsync<ProfileUserRow>();
        if (user is null)
        {
            return null;
        }

        var stats = await multi.ReadSingleAsync<ProfileStatsRow>();
        var monthlyActivity = (await multi.ReadAsync<ActivityRow>())
            .Select(activityPoint => new ActivityPointDto(activityPoint.Label, Convert.ToInt32(activityPoint.Count)))
            .ToArray();
        var unlockedAchievements = (await multi.ReadAsync<AchievementRow>())
            .Select(achievement => new AchievementProgressDto(
                achievement.Id,
                achievement.Title,
                achievement.Description,
                achievement.BadgeImagePath,
                FormatRequirementLabel(achievement.RequirementType, achievement.RequirementValue),
                achievement.Objective,
                achievement.Progress,
                achievement.CompletedFlag == 1))
            .ToArray();

        var averageDuration = stats.TotalMoviesWatched == 0 ? 0 : (int)Math.Round(stats.TotalMinutes / (double)stats.TotalMoviesWatched);

        return new ProfileSummaryDto(
            user.Id,
            user.UserName,
            user.NickName,
            user.Email,
            user.Phone,
            stats.TotalMoviesWatched,
            stats.TotalRatings,
            stats.TotalMinutes,
            averageDuration,
            unlockedAchievements.Length,
            monthlyActivity,
            unlockedAchievements);
    }

    // Devuelve un perfil publico con metricas, logros completados y peliculas vistas por el usuario.
    public async Task<PublicUserProfileDto?> GetPublicProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var summary = await GetProfileAsync(userId, cancellationToken);
        if (summary is null)
        {
            return null;
        }

        const string sql = """
            SELECT p.id AS Id,
            p.nombre AS Title,
            p.anio AS Year,
            p.srcImagen AS PosterPath,
            COALESCE(GROUP_CONCAT(DISTINCT g.nombre ORDER BY g.nombre SEPARATOR ', '), '') AS GenreNames,
            COALESCE(AVG(v_all.puntuacion), 0) AS AverageScore,
            COUNT(DISTINCT v_all.id) AS RatingCount
            FROM valoraciones vu
            INNER JOIN peliculas p ON p.id = vu.pelicula_id
            LEFT JOIN pelicula_generos pg ON pg.pelicula_id = p.id
            LEFT JOIN generos g ON g.id = pg.genero_id
            LEFT JOIN valoraciones v_all ON v_all.pelicula_id = p.id
            WHERE vu.usuario_id = @UserId
            GROUP BY p.id, p.nombre, p.anio, p.srcImagen, vu.fecha_registro
            ORDER BY vu.fecha_registro DESC, p.nombre ASC;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        var watchedMovies = (await connection.QueryAsync<PublicMovieRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken)))
            .Select(movie => new MovieCardDto(
                movie.Id,
                movie.Title,
                movie.Year,
                ResolvePosterPath(movie.PosterPath),
                movie.GenreNames,
                Math.Round(movie.AverageScore, 1),
                movie.RatingCount))
            .ToArray();

        return new PublicUserProfileDto(summary, watchedMovies);
    }

    // Comprueba duplicados de email excluyendo el usuario que se esta editando.
    public async Task<bool> IsEmailUsedByAnotherUserAsync(int userId, string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM usuarios
            WHERE email = @Email AND idUsuario <> @UserId;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UserId = userId, Email = email }, cancellationToken: cancellationToken));
        return count > 0;
    }

    // Actualiza los datos editables del perfil.
    public async Task UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE usuarios
            SET nick = @NickName,
                email = @Email,
                telefono = @Phone
            WHERE idUsuario = @UserId;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    // Sustituye el hash de contrasena del usuario.
    public async Task UpdatePasswordAsync(int userId, string passwordHash, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var passwordColumn = await GetPasswordColumnNameAsync(connection, cancellationToken);
        var sql = $"""
            UPDATE usuarios
            SET {passwordColumn} = @PasswordHash
            WHERE idUsuario = @UserId;
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { UserId = userId, PasswordHash = passwordHash },
            cancellationToken: cancellationToken));
    }

    // Obtiene el ranking de comunidad con puntos por retos completados y peliculas valoradas.
    public async Task<IReadOnlyCollection<CommunityMemberDto>> GetCommunityMembersAsync(int? excludedUserId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var userIds = (await connection.QueryAsync<int>(
            new CommandDefinition("SELECT idUsuario FROM usuarios;", cancellationToken: cancellationToken))).ToArray();

        foreach (var userId in userIds)
        {
            await AchievementProgressEngine.RefreshAsync(connection, userId, cancellationToken);
        }

        const string sql = """
            WITH user_stats AS (
                SELECT u.idUsuario AS Id,
                u.usuario AS UserName,
                u.nick AS NickName,
                COUNT(DISTINCT v.id) AS MoviesWatched,
                COUNT(DISTINCT v.id) AS RatingsCount,
                COUNT(DISTINCT CASE WHEN lu.completado = 1 THEN lu.idLogro END) AS BadgesUnlocked,
                COUNT(DISTINCT CASE WHEN ur.estado = 'COMPLETADO' THEN ur.reto_id END) AS CompletedChallenges
                FROM usuarios u
                LEFT JOIN valoraciones v ON v.usuario_id = u.idUsuario
                LEFT JOIN logros_usuario lu ON lu.idUsuario = u.idUsuario
                LEFT JOIN usuario_retos ur ON ur.usuario_id = u.idUsuario
                WHERE (@ExcludedUserId IS NULL OR u.idUsuario <> @ExcludedUserId)
                GROUP BY u.idUsuario, u.usuario, u.nick
            )
            SELECT u.Id,
                   u.UserName,
                   u.NickName,
                   u.MoviesWatched,
                   u.RatingsCount,
                   u.BadgesUnlocked,
                   u.CompletedChallenges,
                   (u.CompletedChallenges * 100) AS ChallengePoints,
                   (u.RatingsCount * 10) AS MoviePoints,
                   ((u.CompletedChallenges * 100) + (u.RatingsCount * 10)) AS RankingPoints
            FROM user_stats u
            ORDER BY RankingPoints DESC, CompletedChallenges DESC, RatingsCount DESC, UserName ASC
            LIMIT 10;
            """;

        var communityRows = await connection.QueryAsync<CommunityMemberRow>(
            new CommandDefinition(sql, new { ExcludedUserId = excludedUserId }, cancellationToken: cancellationToken));
        return communityRows.Select(member => new CommunityMemberDto(
            member.Id,
            member.UserName,
            member.NickName,
            Convert.ToInt32(member.RankingPoints),
            Convert.ToInt32(member.CompletedChallenges),
            Convert.ToInt32(member.ChallengePoints),
            Convert.ToInt32(member.MoviePoints),
            Convert.ToInt32(member.MoviesWatched),
            Convert.ToInt32(member.RatingsCount),
            Convert.ToInt32(member.BadgesUnlocked)))
            .ToArray();
    }

    // Convierte el tipo de requisito guardado en base de datos en texto legible para la interfaz.
    private static string FormatRequirementLabel(string type, string value) => type switch
    {
        "general" => "Objetivo general",
        "genero" => $"Genero: {value}",
        "director" => $"Director: {value}",
        "actor" => $"Actor: {value}",
        "anio" => $"Anio: {value}",
        _ => value
    };

    // Normaliza rutas de poster para perfiles publicos.
    private static string ResolvePosterPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "/img/fondos/fondocine.png";
        }

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("/img/", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (value.StartsWith("/", StringComparison.Ordinal))
        {
            return $"https://image.tmdb.org/t/p/w500{value}";
        }

        return $"/img/peliculas/{Uri.EscapeDataString(value)}";
    }

    // Detecta el nombre real de la columna de contrasena para soportar esquemas heredados.
    private async Task<string> GetPasswordColumnNameAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_passwordColumnName))
        {
            return _passwordColumnName;
        }

        const string sql = """
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'usuarios';
            """;

        var columns = (await connection.QueryAsync<string>(
            new CommandDefinition(sql, cancellationToken: cancellationToken))).ToArray();

        var resolved = columns.FirstOrDefault(static column =>
            column.Contains("contrase", StringComparison.OrdinalIgnoreCase) ||
            column.Contains("pass", StringComparison.OrdinalIgnoreCase));

        _passwordColumnName = EscapeIdentifier(resolved ?? "contraseÃ±a");
        return _passwordColumnName;
    }

    // Escapa nombres de columnas dinamicos antes de interpolarlos en SQL.
    private static string EscapeIdentifier(string identifier) =>
        $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";

    private sealed class ProfileUserRow
    {
        // Expone el valor Id usado por esta capa de la aplicacion.
        public int Id { get; init; }

        // Expone el valor UserName usado por esta capa de la aplicacion.    
        public string UserName { get; init; } = string.Empty;

        // Expone el valor NickName usado por esta capa de la aplicacion.
        public string NickName { get; init; } = string.Empty;

        // Expone el valor Email usado por esta capa de la aplicacion.    
        public string Email { get; init; } = string.Empty;

        // Expone el valor Phone usado por esta capa de la aplicacion.    
        public string Phone { get; init; } = string.Empty;
    }

    private sealed class ProfileStatsRow
    {
        // Expone el valor TotalMoviesWatched usado por esta capa de la aplicacion.
        public int TotalMoviesWatched { get; init; }

        // Expone el valor TotalRatings usado por esta capa de la aplicacion.    
        public int TotalRatings { get; init; }

        // Expone el valor TotalMinutes usado por esta capa de la aplicacion.    
        public int TotalMinutes { get; init; }
    }

    private sealed class ActivityRow
    {
        // Expone el valor Label usado por esta capa de la aplicacion.
        public string Label { get; init; } = string.Empty;

        // Expone el valor Count usado por esta capa de la aplicacion.    
        public long Count { get; init; }
    }

    private sealed class CommunityMemberRow
    {
        // Expone el valor Id usado por esta capa de la aplicacion.
        public int Id { get; init; }

        // Expone el valor UserName usado por esta capa de la aplicacion.
        public string UserName { get; init; } = string.Empty;

        // Expone el valor NickName usado por esta capa de la aplicacion.    
        public string NickName { get; init; } = string.Empty;

        // Expone el valor RankingPoints usado por esta capa de la aplicacion.    
        public long RankingPoints { get; init; }

        // Expone el valor CompletedChallenges usado por esta capa de la aplicacion.    
        public long CompletedChallenges { get; init; }

        // Expone el valor ChallengePoints usado por esta capa de la aplicacion.    
        public long ChallengePoints { get; init; }

        // Expone el valor MoviePoints usado por esta capa de la aplicacion.    
        public long MoviePoints { get; init; }

        // Expone el valor MoviesWatched usado por esta capa de la aplicacion.
        public long MoviesWatched { get; init; }

        // Expone el valor RatingsCount usado por esta capa de la aplicacion.    
        public long RatingsCount { get; init; }

        // Expone el valor BadgesUnlocked usado por esta capa de la aplicacion.
        public long BadgesUnlocked { get; init; }
    }

    private sealed class PublicMovieRow
    {
        // Expone el valor Id usado por esta capa de la aplicacion.
        public int Id { get; init; }

        // Expone el valor Title usado por esta capa de la aplicacion.
        public string Title { get; init; } = string.Empty;

        // Expone el valor Year usado por esta capa de la aplicacion.    
        public int Year { get; init; }

        // Expone el valor PosterPath usado por esta capa de la aplicacion.
        public string PosterPath { get; init; } = string.Empty;

        // Expone el valor GenreNames usado por esta capa de la aplicacion.    
        public string GenreNames { get; init; } = string.Empty;

        // Expone el valor AverageScore usado por esta capa de la aplicacion.    
        public double AverageScore { get; init; }

        // Expone el valor RatingCount usado por esta capa de la aplicacion.    
        public int RatingCount { get; init; }
    }

    private sealed class AchievementRow
    {
        // Expone el valor Id usado por esta capa de la aplicacion.
        public int Id { get; init; }

        // Expone el valor Title usado por esta capa de la aplicacion.
        public string Title { get; init; } = string.Empty;

        // Expone el valor Description usado por esta capa de la aplicacion.    
        public string Description { get; init; } = string.Empty;

        // Expone el valor BadgeImagePath usado por esta capa de la aplicacion.    
        public string BadgeImagePath { get; init; } = string.Empty;

        // Expone el valor RequirementType usado por esta capa de la aplicacion.
        public string RequirementType { get; init; } = string.Empty;

        // Expone el valor RequirementValue usado por esta capa de la aplicacion.    
        public string RequirementValue { get; init; } = string.Empty;

        // Expone el valor Objective usado por esta capa de la aplicacion.    
        public int Objective { get; init; }

        // Expone el valor Progress usado por esta capa de la aplicacion.    
        public int Progress { get; init; }

        // Expone el valor CompletedFlag usado por esta capa de la aplicacion.
        public int CompletedFlag { get; init; }
    }
}
