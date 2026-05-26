// Archivo: CRM.Datos\Seed\DatabaseInitializer.cs
// Inicializador que prepara datos base para que la aplicacion pueda arrancar con catalogo de ejemplo.

using Dapper;
using CRM.Datos.Context;
using CRM.Entidad.Enums;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Security;
using MySqlConnector;

namespace CRM.Datos.Seed;

// Representa la responsabilidad de DatabaseInitializer dentro de la aplicacion.
public sealed class DatabaseInitializer : IDatabaseInitializer
{
    // Guarda la dependencia _connectionFactory recibida por inyeccion.
    private readonly IConnectionFactory _connectionFactory;

    // Inicializa DatabaseInitializer con las dependencias necesarias.
    public DatabaseInitializer(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Punto de entrada que deja la base lista cada vez que arranca la aplicacion web.
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var dbConnection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var connection = (MySqlConnection)dbConnection;

        await EnsureSeedCatalogAsync(connection, cancellationToken);
        await EnsureProgressRowsAsync(connection, cancellationToken);
    }

    // Inserta datos de ejemplo cuando MySQL esta vacio para que la home y el catalogo no arranquen sin peliculas.
    private static async Task EnsureSeedCatalogAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        if (await CountAsync(connection, "generos", cancellationToken) == 0)
        {
            const string sql = """
                INSERT INTO generos (id, nombre)
                VALUES (@Id, @Name);
                """;
            await connection.ExecuteAsync(new CommandDefinition(sql, SeedCatalog.Genres.Select(genre => new { genre.Id, genre.Name }), cancellationToken: cancellationToken));
        }

        if (await CountAsync(connection, "directores", cancellationToken) == 0)
        {
            const string sql = """
                INSERT INTO directores (id, nombre)
                VALUES (@Id, @Name);
                """;
            await connection.ExecuteAsync(new CommandDefinition(sql, SeedCatalog.Directors.Select(director => new { director.Id, director.Name }), cancellationToken: cancellationToken));
        }

        if (await CountAsync(connection, "actores", cancellationToken) == 0)
        {
            const string sql = """
                INSERT INTO actores (id, nombre)
                VALUES (@Id, @Name);
                """;
            await connection.ExecuteAsync(new CommandDefinition(sql, SeedCatalog.Actors.Select(actor => new { actor.Id, actor.Name }), cancellationToken: cancellationToken));
        }

        if (await CountAsync(connection, "peliculas", cancellationToken) == 0)
        {
            await InsertMoviesAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "insignias", cancellationToken) == 0)
        {
            await InsertBadgesAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "logros", cancellationToken) == 0)
        {
            await InsertAchievementsAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "catalogo_retos", cancellationToken) == 0)
        {
            await InsertChallengesAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "usuarios", cancellationToken) == 0)
        {
            await InsertDemoUsersAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "valoraciones", cancellationToken) == 0)
        {
            await InsertRatingsAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "lista_pendientes", cancellationToken) == 0)
        {
            await InsertWatchlistAsync(connection, cancellationToken);
        }

        if (await CountAsync(connection, "usuario_retos", cancellationToken) == 0)
        {
            await InsertUserChallengesAsync(connection, cancellationToken);
        }
    }

    // Carga las peliculas base y sus relaciones para que las tarjetas tengan generos, reparto y directores.
    private static async Task InsertMoviesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string movieSql = """
            INSERT INTO peliculas (id, nombre, anio, duracion, lenguaje_orig, sinapsis, srcImagen)
            VALUES (@Id, @Title, @Year, @DurationMinutes, @OriginalLanguage, @Synopsis, @PosterPath);
            """;
        await connection.ExecuteAsync(new CommandDefinition(movieSql, SeedCatalog.Movies, cancellationToken: cancellationToken));

        const string genreSql = """
            INSERT INTO pelicula_generos (pelicula_id, genero_id)
            VALUES (@MovieId, @GenreId);
            """;
        await connection.ExecuteAsync(new CommandDefinition(genreSql, SeedCatalog.MovieGenres.Select(movieGenre => new { movieGenre.MovieId, movieGenre.GenreId }), cancellationToken: cancellationToken));

        const string directorSql = """
            INSERT INTO pelicula_directores (pelicula_id, director_id)
            VALUES (@MovieId, @DirectorId);
            """;
        await connection.ExecuteAsync(new CommandDefinition(directorSql, SeedCatalog.MovieDirectors.Select(movieDirector => new { movieDirector.MovieId, movieDirector.DirectorId }), cancellationToken: cancellationToken));

        const string actorSql = """
            INSERT INTO pelicula_actores (pelicula_id, actor_id)
            VALUES (@MovieId, @ActorId);
            """;
        await connection.ExecuteAsync(new CommandDefinition(actorSql, SeedCatalog.MovieActors.Select(movieActor => new { movieActor.MovieId, movieActor.ActorId }), cancellationToken: cancellationToken));
    }

    // Crea insignias de ejemplo solo cuando no existen todavia en la base.
    private static async Task InsertBadgesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO insignias (idInsignia, nombre, srcImagen)
            VALUES (@Id, @Name, @ImagePath);
            """;
        await connection.ExecuteAsync(new CommandDefinition(sql, SeedCatalog.Badges, cancellationToken: cancellationToken));
    }

    // Crea logros vinculados a las insignias para que el progreso del usuario pueda calcularse.
    private static async Task InsertAchievementsAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO logros (idLogro, nombreReto, descripcion, objetivo, idInsignia, tipo_requisito, valor_requisito)
            VALUES (@Id, @Title, @Description, @Objective, @BadgeId, @RequirementType, @RequirementValue);
            """;

        var achievementRows = SeedCatalog.Achievements.Select(achievement => new
        {
            achievement.Id,
            achievement.Title,
            achievement.Description,
            achievement.Objective,
            achievement.BadgeId,
            RequirementType = ToRuleValue(achievement.RequirementType),
            achievement.RequirementValue
        });

        await connection.ExecuteAsync(new CommandDefinition(sql, achievementRows, cancellationToken: cancellationToken));
    }

    // Crea retos de ejemplo para que el registro y el panel de retos tengan contenido asignable.
    private static async Task InsertChallengesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO catalogo_retos (id, nombre, descripcion, tipo, progreso_objetivo)
            VALUES (@Id, @Name, @Description, @Type, @TargetProgress);
            """;

        var challengeRows = SeedCatalog.Challenges.Select(challenge => new
        {
            challenge.Id,
            challenge.Name,
            challenge.Description,
            Type = ToChallengeValue(challenge.Type),
            challenge.TargetProgress
        });

        await connection.ExecuteAsync(new CommandDefinition(sql, challengeRows, cancellationToken: cancellationToken));
    }

    // Inserta usuarios demo con una contrasena conocida para poder entrar y probar la aplicacion.
    private static async Task InsertDemoUsersAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        var passwordColumn = await GetPasswordColumnNameAsync(connection, cancellationToken);
        var sql = $"""
            INSERT INTO usuarios (idUsuario, usuario, nick, {passwordColumn}, email, telefono)
            VALUES (@Id, @UserName, @NickName, @PasswordHash, @Email, @Phone);
            """;

        var userRows = SeedCatalog.Users.Select(user => new
        {
            user.Id,
            user.UserName,
            user.NickName,
            user.Email,
            user.Phone,
            PasswordHash = PasswordCodec.Hash("demo1234")
        });

        await connection.ExecuteAsync(new CommandDefinition(sql, userRows, cancellationToken: cancellationToken));
    }

    // Inserta valoraciones historicas para que rankings, recomendaciones y perfil muestren datos desde el primer login.
    private static async Task InsertRatingsAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO valoraciones (usuario_id, pelicula_id, puntuacion, fecha_registro)
            SELECT @UserId, @MovieId, @Score, DATE_SUB(UTC_TIMESTAMP(), INTERVAL @DaysAgo DAY)
            WHERE EXISTS (SELECT 1 FROM usuarios WHERE idUsuario = @UserId)
            AND EXISTS (SELECT 1 FROM peliculas WHERE id = @MovieId);
            """;
        await connection.ExecuteAsync(new CommandDefinition(sql, SeedCatalog.Ratings, cancellationToken: cancellationToken));
    }

    // Inserta peliculas pendientes de ejemplo para que el filtro "Mi Lista" tenga contenido en usuarios demo.
    private static async Task InsertWatchlistAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO lista_pendientes (usuario_id, pelicula_id, creado_en)
            SELECT @UserId, @MovieId, UTC_TIMESTAMP()
            WHERE EXISTS (SELECT 1 FROM usuarios WHERE idUsuario = @UserId)
            AND EXISTS (SELECT 1 FROM peliculas WHERE id = @MovieId);
            """;
        await connection.ExecuteAsync(new CommandDefinition(sql, SeedCatalog.WatchlistEntries, cancellationToken: cancellationToken));
    }

    // Asigna retos iniciales a usuarios demo para que el modulo de retos no se vea vacio.
    private static async Task InsertUserChallengesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO usuario_retos (usuario_id, reto_id, estado, progreso_actual, fecha_fin)
            SELECT @UserId, @ChallengeId, 'ACEPTADO', 0, DATE_ADD(CURRENT_DATE(), INTERVAL @DaysLeft DAY)
            WHERE EXISTS (SELECT 1 FROM usuarios WHERE idUsuario = @UserId)
            AND EXISTS (SELECT 1 FROM catalogo_retos WHERE id = @ChallengeId);
            """;

        var userChallengeRows = SeedCatalog.UserChallenges.Select(userChallenge => new
        {
            userChallenge.UserId,
            ChallengeId = userChallenge.ChallengeId,
            DaysLeft = Math.Max(1, 30 - userChallenge.DaysAgoAssigned)
        });

        await connection.ExecuteAsync(new CommandDefinition(sql, userChallengeRows, cancellationToken: cancellationToken));
    }

    // Cuenta filas de una tabla controlada por el codigo para decidir si hay que sembrar datos.
    private static Task<int> CountAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        var sql = $"SELECT COUNT(*) FROM `{tableName}`;";
        return connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    // Localiza la columna de contrasena aunque la base venga de scripts con nombres heredados.
    private static async Task<string> GetPasswordColumnNameAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
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

        return EscapeIdentifier(resolved ?? "contrasena");
    }

    // Escapa nombres de columnas dinamicos para evitar romper SQL con caracteres especiales.
    private static string EscapeIdentifier(string identifier) =>
        $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";

    // Traduce el enum de dominio al valor guardado por el esquema MySQL.
    private static string ToRuleValue(RuleType value) => value switch
    {
        RuleType.Genre => "genero",
        RuleType.Director => "director",
        RuleType.Actor => "actor",
        RuleType.Year => "anio",
        _ => "general"
    };

    // Traduce el enum de reto al literal que espera la columna ENUM de catalogo_retos.
    private static string ToChallengeValue(ChallengeType value) => value switch
    {
        ChallengeType.Daily => "DIARIO",
        ChallengeType.Weekly => "SEMANAL",
        _ => "TEMATICO"
    };

    // Garantiza que cada usuario tenga una fila de progreso para cada logro disponible.
    private static async Task EnsureProgressRowsAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        const string achievementsSql = """
            INSERT IGNORE INTO logros_usuario (idUsuario, idLogro, progreso, completado)
            SELECT u.idUsuario, l.idLogro, 0, 0
            FROM usuarios u
            CROSS JOIN logros l;
            """;

        await connection.ExecuteAsync(new CommandDefinition(achievementsSql, cancellationToken: cancellationToken));
    }
}
