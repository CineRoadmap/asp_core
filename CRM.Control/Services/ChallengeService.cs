// Archivo: CRM.Control\Services\ChallengeService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;

namespace CRM.Control.Services;

// Representa la responsabilidad de ChallengeService dentro de la aplicacion.
public sealed class ChallengeService : IChallengeService
{
    // Guarda la dependencia _repository recibida por inyeccion.
    private readonly IChallengeRepository _repository;

    // Inicializa ChallengeService con las dependencias necesarias.
    public ChallengeService(IChallengeRepository repository)
    {
        _repository = repository;
    }

    // Pide al repositorio los retos asignados al usuario.
    public Task<IReadOnlyCollection<UserChallengeDto>> GetChallengesAsync(int userId, CancellationToken cancellationToken) =>
        _repository.GetChallengesAsync(userId, cancellationToken);
}
