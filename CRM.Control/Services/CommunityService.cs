// Archivo: CRM.Control\Services\CommunityService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;

namespace CRM.Control.Services;

// Representa la responsabilidad de CommunityService dentro de la aplicacion.
public sealed class CommunityService : ICommunityService
{
    // Guarda la dependencia _users recibida por inyeccion.
    private readonly IUserRepository _users;

    // Inicializa CommunityService con las dependencias necesarias.
    public CommunityService(IUserRepository users)
    {
        _users = users;
    }

    // Devuelve la lista de miembros ordenada por ranking de puntos de la comunidad.
    // Solo se devuelven DTOs publicos, no datos sensibles de cuenta ni credenciales.
    public Task<IReadOnlyCollection<CommunityMemberDto>> GetMembersAsync(int? excludedUserId, CancellationToken cancellationToken) =>
        _users.GetCommunityMembersAsync(excludedUserId, cancellationToken);
}
