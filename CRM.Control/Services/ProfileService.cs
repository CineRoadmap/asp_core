// Archivo: CRM.Control\Services\ProfileService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;
using CRM.Proyecto.Security;

namespace CRM.Control.Services;

// Representa la responsabilidad de ProfileService dentro de la aplicacion.
public sealed class ProfileService : IProfileService
{
    // Guarda la dependencia _users recibida por inyeccion.
    private readonly IUserRepository _users;

    // Inicializa ProfileService con las dependencias necesarias.
    public ProfileService(IUserRepository users)
    {
        _users = users;
    }

    // Recupera el resumen completo del perfil de un usuario.
    public Task<ProfileSummaryDto?> GetProfileAsync(int userId, CancellationToken cancellationToken) =>
        _users.GetProfileAsync(userId, cancellationToken);

    // Recupera el perfil publico de un usuario de comunidad.
    public Task<PublicUserProfileDto?> GetPublicProfileAsync(int userId, CancellationToken cancellationToken) =>
        _users.GetPublicProfileAsync(userId, cancellationToken);

    // Actualiza los datos editables del perfil privado.
    public async Task<ProfileActionResultDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NickName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Phone))
        {
            return ProfileActionResultDto.Failure("Nick, email y telefono son obligatorios.");
        }

        if (await _users.IsEmailUsedByAnotherUserAsync(request.UserId, request.Email.Trim(), cancellationToken))
        {
            return ProfileActionResultDto.Failure("Ese email ya esta en uso por otro usuario.");
        }

        await _users.UpdateProfileAsync(new UpdateProfileRequest
        {
            UserId = request.UserId,
            NickName = request.NickName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim()
        }, cancellationToken);

        return ProfileActionResultDto.Success("Perfil actualizado correctamente.");
    }

    // Cambia la contrasena tras comprobar la actual.
    public async Task<ProfileActionResultDto> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ProfileActionResultDto.Failure("Introduce la contrasena actual y la nueva.");
        }

        if (request.NewPassword.Length < 8)
        {
            return ProfileActionResultDto.Failure("La nueva contrasena debe tener al menos 8 caracteres.");
        }

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !PasswordCodec.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return ProfileActionResultDto.Failure("La contrasena actual no es correcta.");
        }

        await _users.UpdatePasswordAsync(request.UserId, PasswordCodec.Hash(request.NewPassword), cancellationToken);
        return ProfileActionResultDto.Success("Contrasena cambiada correctamente.");
    }
}
