// Archivo: CRM.Datos\Import\AdminImportService.cs
// Servicio de importacion usado por administracion para cargar catalogo externo en la base de datos.

using CRM.Datos.Context;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;
using Dapper;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CRM.Datos.Import;

// Representa la responsabilidad de AdminImportService dentro de la aplicacion.
public sealed class AdminImportService : IAdminImportService
{
    // Guarda la dependencia _connectionFactory recibida por inyeccion.
    private readonly IConnectionFactory _connectionFactory;

    // Guarda la dependencia _sync recibida por inyeccion.
    private readonly object _sync = new();

    // Guarda el estado actual del proceso de importacion TMDB.
    private ImportState _state = ImportState.Idle();

    // Inicializa AdminImportService con las dependencias necesarias.
    public AdminImportService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Devuelve el ultimo estado conocido de la importacion sin bloquear la pantalla de administracion.
    public Task<AdminImportStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            return Task.FromResult(_state.ToDto());
        }
    }

    // Resume el estado real del catalogo guardado en MySQL para detectar si la aplicacion esta sin peliculas.
    public async Task<AdminCatalogHealthDto> GetCatalogHealthAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(*) FROM peliculas;
            SELECT COUNT(*) FROM generos;
            SELECT COUNT(*) FROM directores;
            SELECT COUNT(*) FROM actores;
            SELECT COUNT(*) FROM valoraciones;
            SELECT COUNT(*) FROM lista_pendientes;
            SELECT COALESCE(nombre, 'Sin peliculas')
            FROM peliculas
            ORDER BY id DESC
            LIMIT 1;
            """;

        using var multi = await connection.QueryMultipleAsync(new Dapper.CommandDefinition(sql, cancellationToken: cancellationToken));

        return new AdminCatalogHealthDto(
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleAsync<int>(),
            await multi.ReadSingleOrDefaultAsync<string>() ?? "Sin peliculas");
    }

    // Consulta una pelicula concreta en TMDB y expone el identificador de IMDb que TMDB devuelve para comprobar el enlace entre APIs.
    public async Task<AdminApiProbeResultDto> ProbeTmdbMovieAsync(TmdbApiProbeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return EmptyProbe(false, request.MovieId, "Debes indicar una API key de TMDB para probar la conexion.");
        }

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.themoviedb.org/3/")
            };

            var url = $"movie/{request.MovieId}?api_key={Uri.EscapeDataString(request.ApiKey.Trim())}&language={Uri.EscapeDataString(request.Language.Trim())}&append_to_response=credits";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return EmptyProbe(false, request.MovieId, $"TMDB ha respondido {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            var details = await response.Content.ReadFromJsonAsync<TmdbProbeMovieResponse>(cancellationToken: cancellationToken);
            if (details is null)
            {
                return EmptyProbe(false, request.MovieId, "TMDB no ha devuelto datos legibles para esa pelicula.");
            }

            var imdbUrl = string.IsNullOrWhiteSpace(details.ImdbId)
                ? string.Empty
                : $"https://www.imdb.com/title/{details.ImdbId}/";

            return new AdminApiProbeResultDto(
                true,
                "TMDB + IMDb",
                string.IsNullOrWhiteSpace(details.ImdbId)
                    ? "TMDB responde correctamente, pero esta ficha no trae imdb_id."
                    : "TMDB responde correctamente y trae enlace de IMDb.",
                details.Id,
                details.Title,
                details.OriginalTitle,
                details.ReleaseDate,
                details.Runtime ?? 0,
                details.OriginalLanguage,
                ResolvePosterPath(details.PosterPath),
                details.ImdbId ?? string.Empty,
                imdbUrl,
                details.Genres.Select(genre => genre.Name).Where(genreName => !string.IsNullOrWhiteSpace(genreName)).ToArray(),
                details.Credits?.Crew
                    .Where(crewMember => string.Equals(crewMember.Job, "Director", StringComparison.OrdinalIgnoreCase))
                    .Select(crewMember => crewMember.Name ?? string.Empty)
                    .Where(personName => !string.IsNullOrWhiteSpace(personName))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray() ?? Array.Empty<string>(),
                details.Credits?.Cast
                    .Select(castMember => castMember.Name ?? string.Empty)
                    .Where(personName => !string.IsNullOrWhiteSpace(personName))
                    .Take(5)
                    .ToArray() ?? Array.Empty<string>());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            return EmptyProbe(false, request.MovieId, $"No se ha podido consultar TMDB: {ex.Message}");
        }
    }

    // Arranca la importacion popular de TMDB en segundo plano para no dejar esperando al navegador.
    public Task<AdminImportLaunchResultDto> StartTmdbImportAsync(TmdbImportRequest request, string startedBy, CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_state.IsRunning)
            {
                return Task.FromResult(new AdminImportLaunchResultDto(false, "Ya hay una importacion en ejecucion."));
            }

            _state = ImportState.Started(startedBy, request.TotalPages);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                // Mantenemos el HttpClient aquí con un timeout prudencial para peticiones paralelas masivas
                using var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.themoviedb.org/3/"),
                    Timeout = TimeSpan.FromSeconds(30) 
                };

                var importer = new TmdbCatalogImporter(_connectionFactory, httpClient);
                var progress = new Progress<TmdbImportProgressDto>(UpdateProgress);
                
                await importer.ImportPopularCatalogAsync(request, progress, CancellationToken.None);
                MarkCompleted();
            }
            catch (Exception ex)
            {
                MarkFailed(ex);
            }
        }, CancellationToken.None);

        return Task.FromResult(new AdminImportLaunchResultDto(true, "La importacion de TMDB se ha iniciado en segundo plano."));
    }

    // Actualiza contadores y mensaje visible en el panel mientras el importador avanza.
    private void UpdateProgress(TmdbImportProgressDto progress)
    {
        lock (_sync)
        {
            _state = _state with
            {
                IsRunning = progress.Stage != "Completado",
                Stage = progress.Stage,
                Message = progress.Message,
                TotalPages = progress.TotalPages,
                CurrentPage = progress.CurrentPage,
                ProcessedMovies = progress.ProcessedMovies,
                ImportedMovies = progress.ImportedMovies,
                SkippedMovies = progress.SkippedMovies,
                FailedMovies = progress.FailedMovies
            };
        }
    }

    // Marca la tarea como terminada cuando el importador completa todas las paginas solicitadas.
    private void MarkCompleted()
    {
        lock (_sync)
        {
            _state = _state with
            {
                IsRunning = false,
                Stage = "Completado",
                Message = "Importacion completada.",
                FinishedAtUtc = DateTime.UtcNow
            };
        }
    }

    // Guarda el error final para que el panel pueda mostrar que fallo y por que.
    private void MarkFailed(Exception exception)
    {
        lock (_sync)
        {
            _state = _state with
            {
                IsRunning = false,
                Stage = "Error",
                Message = "La importacion ha terminado con error.",
                LastError = exception.Message,
                FinishedAtUtc = DateTime.UtcNow
            };
        }
    }

    // Crea una respuesta uniforme cuando la prueba no llega a obtener una ficha valida de TMDB.
    private static AdminApiProbeResultDto EmptyProbe(bool succeeded, int tmdbId, string message) => new(
        succeeded,
        "TMDB + IMDb",
        message,
        tmdbId,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>());

    // Convierte el poster relativo de TMDB en una URL completa para poder previsualizarlo en el panel.
    private static string ResolvePosterPath(string? posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath))
        {
            return string.Empty;
        }

        return posterPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? posterPath
            : $"https://image.tmdb.org/t/p/w300{posterPath}";
    }


    // Transporta los datos de ImportState entre capas.
    private sealed record ImportState(
        bool IsRunning,
        string Stage,
        string Message,
        string StartedBy,
        DateTime? StartedAtUtc,
        DateTime? FinishedAtUtc,
        int TotalPages,
        int CurrentPage,
        int ProcessedMovies,
        int ImportedMovies,
        int SkippedMovies,
        int FailedMovies,
        string LastError)
    {
        // Ejecuta la operacion Idle con los parametros recibidos.    
        public static ImportState Idle() => new(
            false,
            "Inactivo",
            "Sin importaciones ejecutadas todavia.",
            string.Empty,
            null,
            null,
            0,
            0,
            0,
            0,
            0,
            0,
            string.Empty);

        // Ejecuta la operacion Started con los parametros recibidos.    
        public static ImportState Started(string startedBy, int totalPages) => new(
            true,
            "En cola",
            "La importacion se esta preparando.",
            startedBy,
            DateTime.UtcNow,
            null,
            totalPages,
            0,
            0,
            0,
            0,
            0,
            string.Empty);

        // Transforma el estado interno mutable en un DTO inmutable para la capa web.
        public AdminImportStatusDto ToDto() => new(
            IsRunning,
            Stage,
            Message,
            StartedBy,
            StartedAtUtc,
            FinishedAtUtc,
            TotalPages,
            CurrentPage,
            ProcessedMovies,
            ImportedMovies,
            SkippedMovies,
            FailedMovies,
            LastError);
    }

    private sealed class TmdbProbeMovieResponse
    {
        // Expone el valor Id usado por esta capa de la aplicacion.
        [JsonPropertyName("id")]
        public int Id { get; init; }

        // Expone el valor Title usado por esta capa de la aplicacion.    
        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        // Expone el valor OriginalTitle usado por esta capa de la aplicacion.    
        [JsonPropertyName("original_title")]
        public string OriginalTitle { get; init; } = string.Empty;

        // Expone el valor ReleaseDate usado por esta capa de la aplicacion.    
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; init; } = string.Empty;

        // Expone el valor Runtime usado por esta capa de la aplicacion.    
        [JsonPropertyName("runtime")]
        public int? Runtime { get; init; }

        // Expone el valor OriginalLanguage usado por esta capa de la aplicacion.    
        [JsonPropertyName("original_language")]
        public string OriginalLanguage { get; init; } = string.Empty;

        // Expone el valor PosterPath usado por esta capa de la aplicacion.    
        [JsonPropertyName("poster_path")]
        public string PosterPath { get; init; } = string.Empty;

        // Expone el valor ImdbId usado por esta capa de la aplicacion.    
        [JsonPropertyName("imdb_id")]
        public string? ImdbId { get; init; }

        // Expone el valor Genres usado por esta capa de la aplicacion.    
        [JsonPropertyName("genres")]
        public List<TmdbProbeGenre> Genres { get; init; } = [];

        // Expone el valor Credits usado por esta capa de la aplicacion.    
        [JsonPropertyName("credits")]
        public TmdbProbeCredits? Credits { get; init; }
    }

    private sealed class TmdbProbeGenre
    {
        // Expone el valor Name usado por esta capa de la aplicacion.
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
    }

    private sealed class TmdbProbeCredits
    {
        // Expone el valor Cast usado por esta capa de la aplicacion.
        [JsonPropertyName("cast")]
        public List<TmdbProbeCastMember> Cast { get; init; } = [];

        // Expone el valor Crew usado por esta capa de la aplicacion.    
        [JsonPropertyName("crew")]
        public List<TmdbProbeCrewMember> Crew { get; init; } = [];
    }

    private sealed class TmdbProbeCastMember
    {
        // Expone el valor Name usado por esta capa de la aplicacion.
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed class TmdbProbeCrewMember
    {
        // Expone el valor Name usado por esta capa de la aplicacion.
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        // Expone el valor Job usado por esta capa de la aplicacion.    
        [JsonPropertyName("job")]
        public string? Job { get; init; }
    }
}
