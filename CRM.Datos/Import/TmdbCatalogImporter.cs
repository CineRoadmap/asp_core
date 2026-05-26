// Archivo: CRM.Datos\Import\TmdbCatalogImporter.cs
// Importador que consulta TMDB en paralelo y transforma sus datos al modelo del catalogo local.

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Dapper;
using CRM.Datos.Context;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;
using MySqlConnector;

namespace CRM.Datos.Import;

public sealed class TmdbCatalogImporter
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<int, int> _genreIdsByTmdbId = new();
    private readonly Dictionary<string, int> _directorIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _actorIds = new(StringComparer.OrdinalIgnoreCase);
    
    private readonly object _cacheLock = new();

    public TmdbCatalogImporter(IConnectionFactory connectionFactory, HttpClient httpClient)
    {
        _connectionFactory = connectionFactory;
        _httpClient = httpClient;
    }

    public async Task ImportPopularCatalogAsync(
        TmdbImportRequest request,
        IProgress<TmdbImportProgressDto>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new InvalidOperationException("Debes indicar una API key de TMDB.");
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var processedMovies = 0;
        var importedMovies = 0;
        var skippedMovies = 0;
        var failedMovies = 0;

        progress?.Report(new TmdbImportProgressDto(
            "Iniciando",
            request.TotalPages,
            0,
            processedMovies,
            importedMovies,
            skippedMovies,
            failedMovies,
            "Preparando importacion de TMDB."));

        await ImportGenresAsync(connection, request, cancellationToken);
        progress?.Report(new TmdbImportProgressDto(
            "Generos",
            request.TotalPages,
            0,
            processedMovies,
            importedMovies,
            skippedMovies,
            failedMovies,
            "Generos importados."));

        for (var page = 1; page <= Math.Max(1, request.TotalPages); page++)
        {
            progress?.Report(new TmdbImportProgressDto(
                "Pagina",
                request.TotalPages,
                page,
                processedMovies,
                importedMovies,
                skippedMovies,
                failedMovies,
                $"Consultando pagina {page} de TMDB."));

            var popular = await GetFromTmdbAsync<TmdbPopularMoviesResponse>(
                $"movie/popular?api_key={Uri.EscapeDataString(request.ApiKey)}&language={Uri.EscapeDataString(request.Language)}&page={page}",
                cancellationToken);

            if (popular?.Results is null || popular.Results.Count == 0)
            {
                continue;
            }

            // 🔥 OPTIMIZACIÓN CLAVE: Creamos un lote de tareas para procesar la página EN PARALELO
            var tareasPeliculas = popular.Results.Select(async movie =>
            {
                // Incremento seguro entre hilos concurrentes
                Interlocked.Increment(ref processedMovies);

                if (request.SkipWithoutPoster && string.IsNullOrWhiteSpace(movie.PosterPath))
                {
                    Interlocked.Increment(ref skippedMovies);
                    return;
                }

                if (request.SkipWithoutOverview && string.IsNullOrWhiteSpace(movie.Overview))
                {
                    Interlocked.Increment(ref skippedMovies);
                    return;
                }

                try
                {
                    // Cada hilo/película en paralelo necesita abrir su propia conexión para no mezclar transacciones
                    await using var localConnection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
                    await ImportMovieAsync(localConnection, request, movie, cancellationToken);
                    
                    Interlocked.Increment(ref importedMovies);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failedMovies);
                    Console.WriteLine($"[WARN] Pelicula {movie.Id} - {movie.Title}: {ex.Message}");
                }
            });

            await Task.WhenAll(tareasPeliculas);

            progress?.Report(new TmdbImportProgressDto(
                "Pagina",
                request.TotalPages,
                page,
                processedMovies,
                importedMovies,
                skippedMovies,
                failedMovies,
                $"Pagina {page} procesada en paralelo."));

            if (page % 5 == 0 || page == request.TotalPages)
            {
                Console.WriteLine($"Pagina {page} procesada.");
            }

            if (request.DelayMsBetweenPages > 0)
            {
                await Task.Delay(request.DelayMsBetweenPages, cancellationToken);
            }
        }

        progress?.Report(new TmdbImportProgressDto(
            "Completado",
            request.TotalPages,
            request.TotalPages,
            processedMovies,
            importedMovies,
            skippedMovies,
            failedMovies,
            "Importacion completada."));
    }

    private async Task ImportGenresAsync(MySqlConnection connection, TmdbImportRequest request, CancellationToken cancellationToken)
    {
        var genreResponse = await GetFromTmdbAsync<TmdbGenreListResponse>(
            $"genre/movie/list?api_key={Uri.EscapeDataString(request.ApiKey)}&language={Uri.EscapeDataString(request.Language)}",
            cancellationToken);

        if (genreResponse?.Genres is null)
        {
            return;
        }

        const string sql = """
            INSERT INTO generos (id, nombre)
            VALUES (@Id, @Name)
            ON DUPLICATE KEY UPDATE
                nombre = VALUES(nombre);
            SELECT id FROM generos WHERE nombre = @Name LIMIT 1;
            """;

        foreach (var genre in genreResponse.Genres)
        {
            var localGenreId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                sql,
                new { genre.Id, genre.Name },
                cancellationToken: cancellationToken));

            _genreIdsByTmdbId[genre.Id] = localGenreId;
        }
    }

    private async Task ImportMovieAsync(
        MySqlConnection connection,
        TmdbImportRequest request,
        TmdbMovieSummary movie,
        CancellationToken cancellationToken)
    {
        var details = await GetFromTmdbAsync<TmdbMovieDetailsResponse>(
            $"movie/{movie.Id}?api_key={Uri.EscapeDataString(request.ApiKey)}&language={Uri.EscapeDataString(request.Language)}&append_to_response=credits",
            cancellationToken);

        if (details is null)
        {
            return;
        }

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string movieSql = """
            INSERT INTO peliculas (id, nombre, anio, duracion, lenguaje_orig, sinapsis, srcImagen)
            VALUES (@Id, @Title, @Year, @DurationMinutes, @OriginalLanguage, @Synopsis, @PosterPath)
            ON DUPLICATE KEY UPDATE
                nombre = VALUES(nombre),
                anio = VALUES(anio),
                duracion = VALUES(duracion),
                lenguaje_orig = VALUES(lenguaje_orig),
                sinapsis = VALUES(sinapsis),
                srcImagen = VALUES(srcImagen);
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            movieSql,
            new
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = ParseYear(movie.ReleaseDate),
                DurationMinutes = details.Runtime ?? 0,
                OriginalLanguage = movie.OriginalLanguage ?? string.Empty,
                Synopsis = movie.Overview ?? string.Empty,
                PosterPath = movie.PosterPath ?? string.Empty
            },
            transaction: transaction,
            cancellationToken: cancellationToken));

        if (movie.GenreIds.Count > 0)
        {
            const string genreRelationSql = """
                INSERT IGNORE INTO pelicula_generos (pelicula_id, genero_id)
                VALUES (@MovieId, @GenreId);
                """;

            var genreRelations = movie.GenreIds
                .Select(tmdbGenreId => _genreIdsByTmdbId.TryGetValue(tmdbGenreId, out var localGenreId)
                    ? new { MovieId = movie.Id, GenreId = localGenreId }
                    : new { MovieId = movie.Id, GenreId = tmdbGenreId });

            await connection.ExecuteAsync(new CommandDefinition(
                genreRelationSql,
                genreRelations,
                transaction: transaction,
                cancellationToken: cancellationToken));
        }

        var directors = details.Credits?.Crew?
            .Where(crewMember => string.Equals(crewMember.Job, "Director", StringComparison.OrdinalIgnoreCase))
            .Select(crewMember => crewMember.Name!)
            .Where(personName => !string.IsNullOrWhiteSpace(personName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        foreach (var director in directors)
        {
            var directorId = await EnsurePersonAsync(
                connection,
                transaction,
                "directores",
                director,
                _directorIds,
                cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT IGNORE INTO pelicula_directores (pelicula_id, director_id)
                VALUES (@MovieId, @DirectorId);
                """,
                new { MovieId = movie.Id, DirectorId = directorId },
                transaction: transaction,
                cancellationToken: cancellationToken));
        }

        var cast = details.Credits?.Cast?
            .Where(castMember => !string.IsNullOrWhiteSpace(castMember.Name))
            .Take(5)
            .Select(castMember => castMember.Name!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        foreach (var actor in cast)
        {
            var actorId = await EnsurePersonAsync(
                connection,
                transaction,
                "actores",
                actor,
                _actorIds,
                cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT IGNORE INTO pelicula_actores (pelicula_id, actor_id)
                VALUES (@MovieId, @ActorId);
                """,
                new { MovieId = movie.Id, ActorId = actorId },
                transaction: transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<int> EnsurePersonAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string tableName,
        string name,
        Dictionary<string, int> cache,
        CancellationToken cancellationToken)
    {
        // 1. Intento de lectura segura y rápida en caché sin bloquear
        lock (_cacheLock)
        {
            if (cache.TryGetValue(name, out var existingId))
            {
                return existingId;
            }
        }

        var insertSql = $"""
            INSERT INTO {tableName} (nombre)
            VALUES (@Name)
            ON DUPLICATE KEY UPDATE id = LAST_INSERT_ID(id);
            SELECT LAST_INSERT_ID();
            """;

        var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            insertSql,
            new { Name = name },
            transaction: transaction,
            cancellationToken: cancellationToken));

        // 2. Escritura controlada en la caché para que otros hilos la vean inmediatamente
        lock (_cacheLock)
        {
            cache[name] = id;
        }

        return id;
    }

    private async Task<T?> GetFromTmdbAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private static int? ParseYear(string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate) || releaseDate.Length < 4)
        {
            return null;
        }

        return int.TryParse(releaseDate[..4], out var year) ? year : null;
    }

    // --- Subclases de mapeo JSON internas ---
    private sealed class TmdbGenreListResponse
    {
        [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; init; } = [];
    }

    private sealed class TmdbGenre
    {
        [JsonPropertyName("id")] public int Id { get; init; }
        [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    }

    private sealed class TmdbPopularMoviesResponse
    {
        [JsonPropertyName("results")] public List<TmdbMovieSummary> Results { get; init; } = [];
    }

    private sealed class TmdbMovieSummary
    {
        [JsonPropertyName("id")] public int Id { get; init; }
        [JsonPropertyName("title")] public string Title { get; init; } = string.Empty;
        [JsonPropertyName("overview")] public string Overview { get; init; } = string.Empty;
        [JsonPropertyName("poster_path")] public string PosterPath { get; init; } = string.Empty;
        [JsonPropertyName("original_language")] public string OriginalLanguage { get; init; } = string.Empty;
        [JsonPropertyName("release_date")] public string ReleaseDate { get; init; } = string.Empty;
        [JsonPropertyName("genre_ids")] public List<int> GenreIds { get; init; } = [];
    }

    private sealed class TmdbMovieDetailsResponse
    {
        [JsonPropertyName("runtime")] public int? Runtime { get; init; }
        [JsonPropertyName("credits")] public TmdbCreditsResponse? Credits { get; init; }
    }

    private sealed class TmdbCreditsResponse
    {
        [JsonPropertyName("cast")] public List<TmdbCastMember> Cast { get; init; } = [];
        [JsonPropertyName("crew")] public List<TmdbCrewMember> Crew { get; init; } = [];
    }

    private sealed class TmdbCastMember
    {
        [JsonPropertyName("name")] public string? Name { get; init; }
    }

    private sealed class TmdbCrewMember
    {
        [JsonPropertyName("name")] public string? Name { get; init; }
        [JsonPropertyName("job")] public string? Job { get; init; }
    }
}