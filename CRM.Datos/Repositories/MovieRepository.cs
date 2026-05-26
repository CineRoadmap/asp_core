// Archivo: CRM.Datos\Repositories\MovieRepository.cs
// Repositorio Dapper encargado del acceso a datos y consultas SQL de esta entidad.

using System.Text;
using Dapper;
using CRM.Datos.Context;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;

namespace CRM.Datos.Repositories;


// Representa la responsabilidad de MovieRepository dentro de la aplicacion.

public sealed class MovieRepository : DapperRepositoryBase, IMovieRepository
{
   
    // Inicializa MovieRepository con las dependencias necesarias.

    public MovieRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    // Construye los datos de la pagina de inicio: destacada, recomendacion diaria, recomendaciones semanales y top.
    public async Task<HomeDashboardDto> GetDashboardAsync(int? userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var featured = await connection.QuerySingleOrDefaultAsync<MovieCardRow>(
            new CommandDefinition(BuildTopMoviesSql(1), cancellationToken: cancellationToken));

        var seed = DateTime.UtcNow.ToString("yyyyMMdd");
        const string dailySql = """
            SELECT p.id AS Id,
                   p.nombre AS Title,
                   p.anio AS Year,
                   p.srcImagen AS PosterPath,
                   COALESCE(GROUP_CONCAT(DISTINCT g.nombre ORDER BY g.nombre SEPARATOR ', '), '') AS GenreNames,
                   COALESCE(AVG(v.puntuacion), 0) AS AverageScore,
                   COUNT(DISTINCT v.id) AS RatingCount
            FROM peliculas p
            LEFT JOIN pelicula_generos pg ON pg.pelicula_id = p.id
            LEFT JOIN generos g ON g.id = pg.genero_id
            LEFT JOIN valoraciones v ON v.pelicula_id = p.id
            GROUP BY p.id, p.nombre, p.anio, p.srcImagen
            ORDER BY ABS(CRC32(CONCAT(CAST(p.id AS CHAR), @Seed)))
            LIMIT 1;
            """;

        var daily = await connection.QuerySingleOrDefaultAsync<MovieCardRow>(
            new CommandDefinition(dailySql, new { Seed = seed }, cancellationToken: cancellationToken));

        var topMovies = (await connection.QueryAsync<MovieCardRow>(
            new CommandDefinition(BuildTopMoviesSql(6), cancellationToken: cancellationToken)))
            .Select(MapMovieCard)
            .ToArray();

        var weeklyRows = userId.HasValue
            ? (await connection.QueryAsync<MovieCardRow>(
                new CommandDefinition(BuildWeeklyRecommendationsSql(), new { UserId = userId.Value }, cancellationToken: cancellationToken))).ToArray()
            : Array.Empty<MovieCardRow>();

        if (weeklyRows.Length == 0)
        {
            weeklyRows = (await connection.QueryAsync<MovieCardRow>(
                new CommandDefinition(BuildTopMoviesSql(3), cancellationToken: cancellationToken))).ToArray();
        }

        var displayName = "Invitado";
        if (userId.HasValue)
        {
            const string userSql = "SELECT nick FROM usuarios WHERE idUsuario = @UserId;";
            displayName = await connection.ExecuteScalarAsync<string>(
                new CommandDefinition(userSql, new { UserId = userId.Value }, cancellationToken: cancellationToken)) ?? "Cinefilo";
        }

        return new HomeDashboardDto(
            displayName,
            userId.HasValue,
            featured is null ? null : MapMovieCard(featured),
            daily is null ? null : MapMovieCard(daily),
            weeklyRows.Select(MapMovieCard).ToArray(),
            topMovies);
    }

    // Devuelve el catalogo paginado aplicando busqueda, genero, anio y vistas especiales del usuario.
    public async Task<MovieCatalogDto> GetCatalogAsync(MovieFilterRequest request, int? userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("Search", $"%{request.Search.Trim()}%");
        parameters.Add("GenreId", request.GenreId);
        parameters.Add("Year", request.Year);
        parameters.Add("UserId", userId);
        parameters.Add("Offset", (Math.Max(request.Page, 1) - 1) * request.PageSize);
        parameters.Add("PageSize", request.PageSize);

        var joins = new StringBuilder();
        joins.AppendLine("FROM peliculas p");
        joins.AppendLine("LEFT JOIN pelicula_generos pg_all ON pg_all.pelicula_id = p.id");
        joins.AppendLine("LEFT JOIN generos g_all ON g_all.id = pg_all.genero_id");
        joins.AppendLine("LEFT JOIN valoraciones v_all ON v_all.pelicula_id = p.id");

        if (request.GenreId.HasValue)
        {
            joins.AppendLine("INNER JOIN pelicula_generos pg_filter ON pg_filter.pelicula_id = p.id AND pg_filter.genero_id = @GenreId");
        }

        if (request.ViewMode == "watchlist" && userId.HasValue)
        {
            joins.AppendLine("INNER JOIN lista_pendientes lp ON lp.pelicula_id = p.id AND lp.usuario_id = @UserId");
        }

        if (request.ViewMode == "rated" && userId.HasValue)
        {
            joins.AppendLine("INNER JOIN valoraciones v_user ON v_user.pelicula_id = p.id AND v_user.usuario_id = @UserId");
        }

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            where.Add("p.nombre LIKE @Search");
        }

        if (request.Year.HasValue)
        {
            where.Add("p.anio = @Year");
        }

        var whereClause = where.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", where)}";

        var countSql = $"""
            SELECT COUNT(DISTINCT p.id)
            {joins}
            {whereClause};
            """;

        var totalItems = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var dataSql = $"""
            SELECT p.id AS Id,
                   p.nombre AS Title,
                   p.anio AS Year,
                   p.srcImagen AS PosterPath,
                   COALESCE(GROUP_CONCAT(DISTINCT g_all.nombre ORDER BY g_all.nombre SEPARATOR ', '), '') AS GenreNames,
                   COALESCE(AVG(v_all.puntuacion), 0) AS AverageScore,
                   COUNT(DISTINCT v_all.id) AS RatingCount
            {joins}
            {whereClause}
            GROUP BY p.id, p.nombre, p.anio, p.srcImagen
            ORDER BY p.anio DESC, p.nombre ASC
            LIMIT @Offset, @PageSize;
            """;

        var movies = (await connection.QueryAsync<MovieCardRow>(
            new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken)))
            .Select(MapMovieCard)
            .ToArray();

        const string genresSql = "SELECT id AS Id, nombre AS Name FROM generos ORDER BY nombre;";
        var genres = (await connection.QueryAsync<GenreDto>(
            new CommandDefinition(genresSql, cancellationToken: cancellationToken))).ToArray();

        return new MovieCatalogDto(
            new PagedResult<MovieCardDto>(movies, Math.Max(request.Page, 1), request.PageSize, totalItems),
            genres,
            request.Search,
            request.GenreId,
            request.Year,
            request.ViewMode);
    }

    // Recupera la ficha completa de una pelicula, incluyendo relaciones y datos personales del usuario.
    public async Task<MovieDetailsDto?> GetDetailsAsync(int movieId, int? userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT p.id AS Id,
                   p.nombre AS Title,
                   p.anio AS Year,
                   p.duracion AS DurationMinutes,
                   p.lenguaje_orig AS OriginalLanguage,
                   p.sinapsis AS Synopsis,
                   p.srcImagen AS PosterPath,
                   COALESCE(AVG(v_all.puntuacion), 0) AS AverageScore,
                   COALESCE(GROUP_CONCAT(DISTINCT g.nombre ORDER BY g.nombre SEPARATOR '||'), '') AS Genres,
                   COALESCE(GROUP_CONCAT(DISTINCT d.nombre ORDER BY d.nombre SEPARATOR '||'), '') AS Directors,
                   COALESCE(GROUP_CONCAT(DISTINCT a.nombre ORDER BY a.nombre SEPARATOR '||'), '') AS Actors,
                   CASE WHEN @UserId IS NULL THEN 0 ELSE EXISTS (
                       SELECT 1 FROM lista_pendientes lp WHERE lp.usuario_id = @UserId AND lp.pelicula_id = p.id
                   ) END AS IsInWatchlist,
                   (
                       SELECT puntuacion
                       FROM valoraciones vu
                       WHERE vu.usuario_id = @UserId AND vu.pelicula_id = p.id
                       LIMIT 1
                   ) AS UserScore
            FROM peliculas p
            LEFT JOIN valoraciones v_all ON v_all.pelicula_id = p.id
            LEFT JOIN pelicula_generos pg ON pg.pelicula_id = p.id
            LEFT JOIN generos g ON g.id = pg.genero_id
            LEFT JOIN pelicula_directores pd ON pd.pelicula_id = p.id
            LEFT JOIN directores d ON d.id = pd.director_id
            LEFT JOIN pelicula_actores pa ON pa.pelicula_id = p.id
            LEFT JOIN actores a ON a.id = pa.actor_id
            WHERE p.id = @MovieId
            GROUP BY p.id, p.nombre, p.anio, p.duracion, p.lenguaje_orig, p.sinapsis, p.srcImagen;
            """;

        var row = await connection.QuerySingleOrDefaultAsync<MovieDetailsRow>(
            new CommandDefinition(sql, new { MovieId = movieId, UserId = userId }, cancellationToken: cancellationToken));
        if (row is null)
        {
            return null;
        }

        return new MovieDetailsDto(
            row.Id,
            row.Title,
            row.Year,
            row.DurationMinutes,
            row.OriginalLanguage,
            row.Synopsis,
            ResolvePosterPath(row.PosterPath),
            row.AverageScore,
            SplitList(row.Genres),
            SplitList(row.Directors),
            SplitList(row.Actors),
            row.IsInWatchlist == 1,
            row.UserScore);
    }

    // Inserta o actualiza la valoracion del usuario y retira la pelicula de pendientes si estaba en su lista.
    public async Task RateAsync(int movieId, int userId, int score, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        const string sql = """
            INSERT INTO valoraciones (pelicula_id, usuario_id, puntuacion, fecha_registro)
            VALUES (@MovieId, @UserId, @Score, UTC_TIMESTAMP())
            ON DUPLICATE KEY UPDATE
                puntuacion = VALUES(puntuacion),
                fecha_registro = VALUES(fecha_registro);

            DELETE FROM lista_pendientes
            WHERE pelicula_id = @MovieId AND usuario_id = @UserId;
            """;

        await connection.ExecuteAsync(new CommandDefinition(sql, new { MovieId = movieId, UserId = userId, Score = score }, cancellationToken: cancellationToken));
    }

    // Alterna la pelicula en la lista de pendientes: si existe la elimina; si no existe la inserta.
    public async Task ToggleWatchlistAsync(int movieId, int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string existsSql = """
            SELECT EXISTS(
                SELECT 1 FROM lista_pendientes WHERE pelicula_id = @MovieId AND usuario_id = @UserId
            );
            """;

        var exists = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(existsSql, new { MovieId = movieId, UserId = userId }, cancellationToken: cancellationToken));

        if (exists == 1)
        {
            const string deleteSql = "DELETE FROM lista_pendientes WHERE pelicula_id = @MovieId AND usuario_id = @UserId;";
            await connection.ExecuteAsync(new CommandDefinition(deleteSql, new { MovieId = movieId, UserId = userId }, cancellationToken: cancellationToken));
            return;
        }

        const string insertSql = """
            INSERT IGNORE INTO lista_pendientes (usuario_id, pelicula_id, creado_en)
            VALUES (@UserId, @MovieId, UTC_TIMESTAMP());
            """;
        await connection.ExecuteAsync(new CommandDefinition(insertSql, new { MovieId = movieId, UserId = userId }, cancellationToken: cancellationToken));
    }

    // Comprueba que la pelicula existe antes de permitir acciones de usuario sobre ella.
    public async Task<bool> MovieExistsAsync(int movieId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = "SELECT COUNT(*) FROM peliculas WHERE id = @MovieId;";
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { MovieId = movieId }, cancellationToken: cancellationToken));
        return count > 0;
    }

    // Genera la consulta base para obtener peliculas ordenadas por nota media y fecha.
    private static string BuildTopMoviesSql(int limit) => $"""
        SELECT p.id AS Id,
               p.nombre AS Title,
               p.anio AS Year,
               p.srcImagen AS PosterPath,
               COALESCE(GROUP_CONCAT(DISTINCT g.nombre ORDER BY g.nombre SEPARATOR ', '), '') AS GenreNames,
               COALESCE(AVG(v.puntuacion), 0) AS AverageScore,
               COUNT(DISTINCT v.id) AS RatingCount
        FROM peliculas p
        LEFT JOIN pelicula_generos pg ON pg.pelicula_id = p.id
        LEFT JOIN generos g ON g.id = pg.genero_id
        LEFT JOIN valoraciones v ON v.pelicula_id = p.id
        GROUP BY p.id, p.nombre, p.anio, p.srcImagen
        ORDER BY RatingCount DESC, AverageScore DESC, p.anio DESC, p.nombre ASC
        LIMIT {limit};
        """;

    // Genera recomendaciones para usuarios con historial buscando coincidencias de genero, director o actor.
    private static string BuildWeeklyRecommendationsSql() => """
        SELECT p.id AS Id,
               p.nombre AS Title,
               p.anio AS Year,
               p.srcImagen AS PosterPath,
               COALESCE(GROUP_CONCAT(DISTINCT g_all.nombre ORDER BY g_all.nombre SEPARATOR ', '), '') AS GenreNames,
               COALESCE(AVG(v_all.puntuacion), 0) AS AverageScore,
               COUNT(DISTINCT v_all.id) AS RatingCount
        FROM peliculas p
        LEFT JOIN pelicula_generos pg_all ON pg_all.pelicula_id = p.id
        LEFT JOIN generos g_all ON g_all.id = pg_all.genero_id
        LEFT JOIN valoraciones v_all ON v_all.pelicula_id = p.id
        WHERE p.id NOT IN (
            SELECT pelicula_id FROM valoraciones WHERE usuario_id = @UserId
        )
        AND (
            EXISTS (
                SELECT 1
                FROM pelicula_generos pg
                WHERE pg.pelicula_id = p.id
                  AND pg.genero_id IN (
                    SELECT DISTINCT pg2.genero_id
                    FROM valoraciones v2
                    INNER JOIN pelicula_generos pg2 ON pg2.pelicula_id = v2.pelicula_id
                    WHERE v2.usuario_id = @UserId AND v2.puntuacion >= 4
                  )
            )
            OR EXISTS (
                SELECT 1
                FROM pelicula_directores pd
                WHERE pd.pelicula_id = p.id
                  AND pd.director_id IN (
                    SELECT DISTINCT pd2.director_id
                    FROM valoraciones v3
                    INNER JOIN pelicula_directores pd2 ON pd2.pelicula_id = v3.pelicula_id
                    WHERE v3.usuario_id = @UserId AND v3.puntuacion >= 4
                  )
            )
            OR EXISTS (
                SELECT 1
                FROM pelicula_actores pa
                WHERE pa.pelicula_id = p.id
                  AND pa.actor_id IN (
                    SELECT DISTINCT pa2.actor_id
                    FROM valoraciones v4
                    INNER JOIN pelicula_actores pa2 ON pa2.pelicula_id = v4.pelicula_id
                    WHERE v4.usuario_id = @UserId AND v4.puntuacion >= 4
                  )
            )
        )
        GROUP BY p.id, p.nombre, p.anio, p.srcImagen
        ORDER BY RatingCount DESC, AverageScore DESC, p.anio DESC, p.nombre ASC
        LIMIT 3;
        """;

    // Convierte una fila SQL simple en el DTO que consumen las tarjetas de pelicula.
    private static MovieCardDto MapMovieCard(MovieCardRow row) => new(
        row.Id,
        row.Title,
        row.Year,
        ResolvePosterPath(row.PosterPath),
        row.GenreNames,
        Math.Round(row.AverageScore, 1),
        row.RatingCount);

    // Separa listas agregadas por SQL en colecciones limpias para la vista de detalle.
    private static IReadOnlyCollection<string> SplitList(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split("||", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    // Normaliza rutas de poster locales, rutas relativas de TMDB y URLs externas.
    private static string ResolvePosterPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "/img/fondos/fondocine.png";
        }

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (value.StartsWith("/img/", StringComparison.OrdinalIgnoreCase) ||
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

    private sealed class MovieCardRow
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

    private sealed class MovieDetailsRow
    {
       
        // Expone el valor Id usado por esta capa de la aplicacion.
    
        public int Id { get; init; }
       
        // Expone el valor Title usado por esta capa de la aplicacion.
    
        public string Title { get; init; } = string.Empty;
       
        // Expone el valor Year usado por esta capa de la aplicacion.
    
        public int Year { get; init; }
       
        // Expone el valor DurationMinutes usado por esta capa de la aplicacion.
    
        public int DurationMinutes { get; init; }
       
        // Expone el valor OriginalLanguage usado por esta capa de la aplicacion.
    
        public string OriginalLanguage { get; init; } = string.Empty;
       
        // Expone el valor Synopsis usado por esta capa de la aplicacion.
    
        public string Synopsis { get; init; } = string.Empty;
       
        // Expone el valor PosterPath usado por esta capa de la aplicacion.
    
        public string PosterPath { get; init; } = string.Empty;
       
        // Expone el valor AverageScore usado por esta capa de la aplicacion.
    
        public double AverageScore { get; init; }
       
        // Expone el valor Genres usado por esta capa de la aplicacion.
    
        public string Genres { get; init; } = string.Empty;
       
        // Expone el valor Directors usado por esta capa de la aplicacion.
    
        public string Directors { get; init; } = string.Empty;
       
        // Expone el valor Actors usado por esta capa de la aplicacion.
    
        public string Actors { get; init; } = string.Empty;
       
        // Expone el valor IsInWatchlist usado por esta capa de la aplicacion.
    
        public int IsInWatchlist { get; init; }
       
        // Expone el valor UserScore usado por esta capa de la aplicacion.
    
        public int? UserScore { get; init; }
    }
}
