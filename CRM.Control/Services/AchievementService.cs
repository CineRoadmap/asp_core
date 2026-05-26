// Archivo: CRM.Control\Services\AchievementService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;

namespace CRM.Control.Services;

// Representa la responsabilidad de AchievementService dentro de la aplicacion.
public sealed class AchievementService : IAchievementService
{
    // Guarda la dependencia _repository recibida por inyeccion.
    private readonly IAchievementRepository _repository;

    // Inicializa AchievementService con las dependencias necesarias.
    public AchievementService(IAchievementRepository repository)
    {
        _repository = repository;
    }

    // Pide al repositorio los logros y conserva aqui el contrato de negocio de la capa de control.
    public Task<IReadOnlyCollection<AchievementProgressDto>> GetAchievementsAsync(int userId, string? filter, CancellationToken cancellationToken) =>
        _repository.GetAchievementsAsync(userId, filter, cancellationToken);
}
