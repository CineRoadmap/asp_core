// Archivo: CRM.ImportadorTmdb\Program.cs
// Punto de entrada de la herramienta de consola que importa catalogo desde TMDB.

using System.Text.Json;
using CRM.Datos.Context;
using CRM.Datos.Import;
using CRM.Proyecto.Requests;

var arguments = ParseArguments(args);

if (arguments.ContainsKey("help") || arguments.ContainsKey("?"))
{
    PrintHelp();
    return;
}

var repoRoot = AppContext.BaseDirectory;
var connectionString = ResolveConnectionString(arguments, repoRoot);
var apiKey = ResolveApiKey(arguments, repoRoot);

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se ha encontrado la cadena de conexion. Usa --connection o configura DefaultConnection en CRM.AplicacionWeb/appsettings.json.");
}

if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException(
        "No se ha encontrado la API key de TMDB. Usa --api-key o la variable de entorno TMDB_API_KEY.");
}

var request = new TmdbImportRequest
{
    ApiKey = apiKey,
    Language = GetValue(arguments, "language") ?? "es-ES",
    TotalPages = ParseInt(GetValue(arguments, "pages"), 500),
    DelayMsBetweenPages = ParseInt(GetValue(arguments, "delay-ms"), 200),
    SkipWithoutPoster = ParseBool(GetValue(arguments, "skip-without-poster"), true),
    SkipWithoutOverview = ParseBool(GetValue(arguments, "skip-without-overview"), true)
};

Console.WriteLine("Iniciando importacion de TMDB...");
Console.WriteLine($"Paginas: {request.TotalPages}");
Console.WriteLine($"Idioma: {request.Language}");

var connectionFactory = new MySqlConnectionFactory(connectionString);
using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.themoviedb.org/3/")
};

var importer = new TmdbCatalogImporter(connectionFactory, httpClient);
await importer.ImportPopularCatalogAsync(request);

Console.WriteLine("Importacion completada.");

static Dictionary<string, string?> ParseArguments(string[] args)
{
    var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < args.Length; i++)
    {
        var current = args[i];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = current[2..];
        string? value = null;

        if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            value = args[++i];
        }
        else
        {
            value = "true";
        }

        values[key] = value;
    }

    return values;
}

static string? ResolveConnectionString(IReadOnlyDictionary<string, string?> arguments, string baseDirectory)
{
    var direct = GetValue(arguments, "connection");
    if (!string.IsNullOrWhiteSpace(direct))
    {
        return direct;
    }

    var env = Environment.GetEnvironmentVariable("CRM_CONNECTION_STRING");
    if (!string.IsNullOrWhiteSpace(env))
    {
        return env;
    }

    var settingsPath = FindExistingFileUpwards(
        baseDirectory,
        Path.Combine("CRM.AplicacionWeb", "appsettings.Local.json"),
        Path.Combine("CRM.AplicacionWeb", "appsettings.json"));
    if (settingsPath is null || !File.Exists(settingsPath))
    {
        return null;
    }

    using var stream = File.OpenRead(settingsPath);
    using var document = JsonDocument.Parse(stream);
    if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
        connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
    {
        return defaultConnection.GetString();
    }

    return null;
}

static string? ResolveApiKey(IReadOnlyDictionary<string, string?> arguments, string baseDirectory)
{
    var direct = GetValue(arguments, "api-key");
    if (!string.IsNullOrWhiteSpace(direct))
    {
        return direct;
    }

    var env = Environment.GetEnvironmentVariable("TMDB_API_KEY");
    if (!string.IsNullOrWhiteSpace(env))
    {
        return env;
    }

    var importerSettingsPath = FindExistingFileUpwards(
        baseDirectory,
        Path.Combine("CRM.ImportadorTmdb", "appsettings.Local.json"),
        Path.Combine("CRM.ImportadorTmdb", "appsettings.json"));
    if (importerSettingsPath is null || !File.Exists(importerSettingsPath))
    {
        return null;
    }

    using var stream = File.OpenRead(importerSettingsPath);
    using var document = JsonDocument.Parse(stream);
    if (document.RootElement.TryGetProperty("Tmdb", out var tmdb) &&
        tmdb.TryGetProperty("ApiKey", out var apiKey))
    {
        return apiKey.GetString();
    }

    return null;
}

static string? FindFileUpwards(string startDirectory, string relativePath)
{
    var current = new DirectoryInfo(startDirectory);
    while (current is not null)
    {
        var candidate = Path.Combine(current.FullName, relativePath);
        if (File.Exists(candidate))
        {
            return candidate;
        }

        current = current.Parent;
    }

    return null;
}

static string? FindExistingFileUpwards(string startDirectory, params string[] relativePaths)
{
    foreach (var relativePath in relativePaths)
    {
        var path = FindFileUpwards(startDirectory, relativePath);
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            return path;
        }
    }

    return null;
}

static string? GetValue(IReadOnlyDictionary<string, string?> arguments, string key) =>
    arguments.TryGetValue(key, out var value) ? value : null;

static int ParseInt(string? value, int defaultValue) =>
    int.TryParse(value, out var parsed) ? parsed : defaultValue;

static bool ParseBool(string? value, bool defaultValue) =>
    bool.TryParse(value, out var parsed) ? parsed : defaultValue;

static void PrintHelp()
{
    Console.WriteLine("""
Uso:
  dotnet run --project CRM.ImportadorTmdb -- --api-key TU_API_KEY

Opciones:
  --api-key TU_API_KEY
  --connection "Server=...;Database=...;"
  --pages 500
  --language es-ES
  --delay-ms 200
  --skip-without-poster true|false
  --skip-without-overview true|false

Variables de entorno soportadas:
  TMDB_API_KEY
  CRM_CONNECTION_STRING
""");
}
