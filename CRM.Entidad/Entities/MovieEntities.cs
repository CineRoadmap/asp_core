// Archivo: CRM.Entidad\Entities\MovieEntities.cs
// Entidades de dominio que representan los datos principales persistidos por la aplicacion.

namespace CRM.Entidad.Entities;

// Representa la responsabilidad de Movie dentro de la aplicacion.
public sealed class Movie
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Title usado por esta capa de la aplicacion.
    public string Title { get; set; } = string.Empty;

    // Expone el valor Year usado por esta capa de la aplicacion.
    public int Year { get; set; }

    // Expone el valor DurationMinutes usado por esta capa de la aplicacion.
    public int DurationMinutes { get; set; }

    // Expone el valor OriginalLanguage usado por esta capa de la aplicacion.
    public string OriginalLanguage { get; set; } = string.Empty;

    // Expone el valor Synopsis usado por esta capa de la aplicacion.
    public string Synopsis { get; set; } = string.Empty;

    // Expone el valor PosterPath usado por esta capa de la aplicacion.
    public string PosterPath { get; set; } = string.Empty;
}

// Representa la responsabilidad de Genre dentro de la aplicacion.
public sealed class Genre
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Name usado por esta capa de la aplicacion.
    public string Name { get; set; } = string.Empty;
}

// Representa la responsabilidad de Director dentro de la aplicacion.
public sealed class Director
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Name usado por esta capa de la aplicacion.
    public string Name { get; set; } = string.Empty;
}

// Representa la responsabilidad de Actor dentro de la aplicacion.
public sealed class Actor
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor Name usado por esta capa de la aplicacion.
    public string Name { get; set; } = string.Empty;
}

// Representa la responsabilidad de Rating dentro de la aplicacion.
public sealed class Rating
{
    // Expone el valor Id usado por esta capa de la aplicacion.
    public int Id { get; set; }

    // Expone el valor MovieId usado por esta capa de la aplicacion.
    public int MovieId { get; set; }

    // Expone el valor UserId usado por esta capa de la aplicacion.
    public int UserId { get; set; }

    // Expone el valor Score usado por esta capa de la aplicacion.
    public int Score { get; set; }
    
    // Expone el valor CreatedAtUtc usado por esta capa de la aplicacion.
    public DateTime CreatedAtUtc { get; set; }
}

// Representa la responsabilidad de WatchlistEntry dentro de la aplicacion.
public sealed class WatchlistEntry
{
    // Expone el valor UserId usado por esta capa de la aplicacion.
    public int UserId { get; set; }

    // Expone el valor MovieId usado por esta capa de la aplicacion.
    public int MovieId { get; set; }

    // Expone el valor CreatedAtUtc usado por esta capa de la aplicacion.
    public DateTime CreatedAtUtc { get; set; }
}
