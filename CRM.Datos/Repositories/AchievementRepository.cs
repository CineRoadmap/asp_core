// Archivo: CRM.Datos\Repositories\AchievementRepository.cs
// Repositorio Dapper encargado del acceso a datos y consultas SQL de esta entidad.

using Dapper;
using CRM.Datos.Context;
using CRM.Datos.Seed;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;

namespace CRM.Datos.Repositories;


// Representa la responsabilidad de AchievementRepository dentro de la aplicacion.

public sealed class AchievementRepository : DapperRepositoryBase, IAchievementRepository
{
   
    // Inicializa AchievementRepository con las dependencias necesarias.

    public AchievementRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    // Crea o actualiza las filas de progreso de logros para un usuario concreto.
    public async Task EnsureUserProgressRowsAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await AchievementProgressEngine.RefreshAsync(connection, userId, cancellationToken);
    }

    // Recalcula el progreso de todos los logros del usuario despues de cambios en valoraciones o retos.
    public async Task RefreshProgressAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await AchievementProgressEngine.RefreshAsync(connection, userId, cancellationToken);
    }

    // Devuelve los logros con su progreso actual, filtrando por tipo si la vista lo solicita.
    public async Task<IReadOnlyCollection<AchievementProgressDto>> GetAchievementsAsync(int userId, string? filter, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await AchievementProgressEngine.RefreshAsync(connection, userId, cancellationToken);

        var normalizedFilter = NormalizeFilter(filter);
        var where = string.IsNullOrWhiteSpace(normalizedFilter)
            ? string.Empty
            : "WHERE l.tipo_requisito = @Filter";

        var sql = $"""
            SELECT l.idLogro AS Id,
                   l.nombreReto AS Title,
                   l.descripcion AS Description,
                   i.srcImagen AS BadgeImagePath,
                   l.tipo_requisito AS RequirementType,
                   COALESCE(l.valor_requisito, '') AS RequirementValue,
                   l.objetivo AS Objective,
                   lu.progreso AS Progress,
                   CAST(lu.completado AS UNSIGNED) AS CompletedFlag
            FROM logros l
            INNER JOIN insignias i ON i.idInsignia = l.idInsignia
            LEFT JOIN logros_usuario lu ON lu.idLogro = l.idLogro AND lu.idUsuario = @UserId
            {where}
            ORDER BY l.idLogro;
            """;

        var achievementRows = await connection.QueryAsync<AchievementRow>(
            new CommandDefinition(sql, new { UserId = userId, Filter = normalizedFilter }, cancellationToken: cancellationToken));

        return achievementRows.Select(achievement => new AchievementProgressDto(
            achievement.Id,
            achievement.Title,
            achievement.Description,
            achievement.BadgeImagePath,
            FormatRequirementLabel(achievement.RequirementType, achievement.RequirementValue),
            achievement.Objective,
            achievement.Progress,
            achievement.CompletedFlag == 1)).ToArray();
    }

    // Convierte requisitos tecnicos de base de datos en textos comprensibles para la interfaz.
    private static string FormatRequirementLabel(string type, string value) => type switch
    {
        "general" => "Objetivo general",
        "genero" => $"Genero: {value}",
        "director" => $"Director: {value}",
        "actor" => $"Actor: {value}",
        "anio" => $"Anio: {value}",
        _ => value
    };

    // Acepta nombres de filtro legacy o actuales y los traduce al valor real de MySQL.
    private static string NormalizeFilter(string? filter) => filter?.Trim().ToLowerInvariant() switch
    {
        "genre" => "genero",
        "year" => "anio",
        "general" => "general",
        "genero" => "genero",
        "director" => "director",
        "actor" => "actor",
        "anio" => "anio",
        _ => string.Empty
    };

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
