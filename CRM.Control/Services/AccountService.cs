// Archivo: CRM.Control\Services\AccountService.cs
// Servicio de negocio que aplica reglas de la aplicacion y coordina repositorios para esta funcionalidad.

using CRM.Entidad.Entities;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Dtos;
using CRM.Proyecto.Requests;
using CRM.Proyecto.Security;

namespace CRM.Control.Services;


// Representa la responsabilidad de AccountService dentro de la aplicacion.

public sealed class AccountService : IAccountService
{
    // Guarda la dependencia _users recibida por inyeccion.
    private readonly IUserRepository _users;

    // Guarda la dependencia _achievements recibida por inyeccion.
    private readonly IAchievementRepository _achievements;

    // Guarda la dependencia _challenges recibida por inyeccion.
    private readonly IChallengeRepository _challenges;

    // Inicializa AccountService con las dependencias necesarias.
    public AccountService(
        IUserRepository users,
        IAchievementRepository achievements,
        IChallengeRepository challenges)
    {
        _users = users;
        _achievements = achievements;
        _challenges = challenges;
    }

    // Comprueba que las credenciales son validas y devuelve los datos minimos para crear la sesion.
    public async Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginResultDto.Failure("Introduce usuario y contraseÃ±a.");
        }

        var user = await _users.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);
        if (user is null || !PasswordCodec.Verify(request.Password, user.PasswordHash))
        {
            return LoginResultDto.Failure("Las credenciales no son vÃ¡lidas.");
        }

        return LoginResultDto.Success(new AuthenticatedUserDto(user.Id, user.UserName, user.NickName, user.Email));
    }

    // Valida los datos de alta, crea el usuario y deja preparados sus progresos iniciales.
    public async Task<RegisterResultDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.NickName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return RegisterResultDto.Failure("Todos los campos son obligatorios.");
        }

        if (request.Password.Length < 8)
        {
            return RegisterResultDto.Failure("La contraseÃ±a debe tener al menos 8 caracteres.");
        }

        var exists = await _users.ExistsAsync(request.UserName.Trim(), request.Email.Trim(), cancellationToken);
        if (exists)
        {
            return RegisterResultDto.Failure("Ya existe un usuario o email con esos datos.");
        }

        var userId = await _users.CreateAsync(new User
        {
            UserName = request.UserName.Trim(),
            NickName = request.NickName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            PasswordHash = PasswordCodec.Hash(request.Password)
        }, cancellationToken);

        await _achievements.EnsureUserProgressRowsAsync(userId, cancellationToken);
        await _challenges.AssignInitialChallengesAsync(userId, cancellationToken);

        return RegisterResultDto.Success();
    }

    // Permite cambiar una contrasena olvidada verificando usuario y email.
    public async Task<AccountActionResultDto> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return AccountActionResultDto.Failure("Introduce usuario, email y nueva contrasena.");
        }

        if (request.NewPassword.Length < 8)
        {
            return AccountActionResultDto.Failure("La nueva contrasena debe tener al menos 8 caracteres.");
        }

        var user = await _users.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);
        if (user is null || !string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return AccountActionResultDto.Failure("No se ha encontrado una cuenta con ese usuario y email.");
        }

        await _users.UpdatePasswordAsync(user.Id, PasswordCodec.Hash(request.NewPassword), cancellationToken);
        return AccountActionResultDto.Success("Contrasena actualizada. Ya puedes iniciar sesion con la nueva contrasena.");
    }

    // Busca un usuario por identificador para reconstruir datos basicos cuando haga falta.
    public async Task<AuthenticatedUserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? null
            : new AuthenticatedUserDto(user.Id, user.UserName, user.NickName, user.Email);
    }
}
