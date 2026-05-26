// Archivo: CRM.Control\Services\HomeService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;

namespace CRM.Control.Services;

// Representa la responsabilidad de HomeService dentro de la aplicacion.
public sealed class HomeService : IHomeService
{
    // Guarda la dependencia _movies recibida por inyeccion.
    private readonly IMovieRepository _movies;

    // Inicializa HomeService con las dependencias necesarias.
    public HomeService(IMovieRepository movies)
    {
        _movies = movies;
    }

    // Recupera el contenido del dashboard de inicio, personalizado si hay usuario autenticado.
    public Task<HomeDashboardDto> GetDashboardAsync(int? userId, CancellationToken cancellationToken) =>
        _movies.GetDashboardAsync(userId, cancellationToken);
}
