// Archivo: CRM.Proyecto\Requests\Requests.cs
// Objetos de peticion que agrupan entradas de usuario o filtros enviados a los servicios.
namespace CRM.Proyecto.Requests;


// Representa la responsabilidad de LoginRequest dentro de la aplicacion.
public sealed class LoginRequest
{
    // Expone el valor UserName usado por esta capa de la aplicacion.
    public string UserName { get; init; } = string.Empty;

    // Expone el valor Password usado por esta capa de la aplicacion.
    public string Password { get; init; } = string.Empty;
}


// Representa la responsabilidad de RegisterUserRequest dentro de la aplicacion.
public sealed class RegisterUserRequest
{

    // Expone el valor UserName usado por esta capa de la aplicacion.
    public string UserName { get; init; } = string.Empty;

    // Expone el valor NickName usado por esta capa de la aplicacion.
    public string NickName { get; init; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    public string Email { get; init; } = string.Empty;

    // Expone el valor Phone usado por esta capa de la aplicacion.
    public string Phone { get; init; } = string.Empty;

    // Expone el valor Password usado por esta capa de la aplicacion.
    public string Password { get; init; } = string.Empty;
}


// Representa la responsabilidad de ResetPasswordRequest dentro de la aplicacion.
public sealed class ResetPasswordRequest
{

    // Expone el valor UserName usado por esta capa de la aplicacion.
    public string UserName { get; init; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    public string Email { get; init; } = string.Empty;

    // Expone el valor NewPassword usado por esta capa de la aplicacion.
    public string NewPassword { get; init; } = string.Empty;
}


// Representa la responsabilidad de UpdateProfileRequest dentro de la aplicacion.
public sealed class UpdateProfileRequest
{
    // Expone el valor UserId usado por esta capa de la aplicacion.
    public int UserId { get; init; }

    // Expone el valor NickName usado por esta capa de la aplicacion.
    public string NickName { get; init; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    public string Email { get; init; } = string.Empty;

    // Expone el valor Phone usado por esta capa de la aplicacion.
    public string Phone { get; init; } = string.Empty;
}


// Representa la responsabilidad de ChangePasswordRequest dentro de la aplicacion.
public sealed class ChangePasswordRequest
{
    // Expone el valor UserId usado por esta capa de la aplicacion.
    public int UserId { get; init; }

    // Expone el valor CurrentPassword usado por esta capa de la aplicacion.
    public string CurrentPassword { get; init; } = string.Empty;

    // Expone el valor NewPassword usado por esta capa de la aplicacion.
    public string NewPassword { get; init; } = string.Empty;
}


// Transporta los datos de ProfileActionResultDto entre capas.
public sealed record ProfileActionResultDto(bool Succeeded, string Message)
{

    // Ejecuta la operacion Success con los parametros recibidos.
    public static ProfileActionResultDto Success(string message) => new(true, message);

    // Ejecuta la operacion Failure con los parametros recibidos.
    public static ProfileActionResultDto Failure(string message) => new(false, message);
}


// Representa la responsabilidad de MovieFilterRequest dentro de la aplicacion.
public sealed class MovieFilterRequest
{
    // Expone el valor Search usado por esta capa de la aplicacion.
    public string Search { get; init; } = string.Empty;

    // Expone el valor GenreId usado por esta capa de la aplicacion.
    public int? GenreId { get; init; }

    // Expone el valor Year usado por esta capa de la aplicacion.
    public int? Year { get; init; }

    // Expone el valor ViewMode usado por esta capa de la aplicacion.
    public string ViewMode { get; init; } = "all";

    // Expone el valor Page usado por esta capa de la aplicacion.
    public int Page { get; init; } = 1;

    // Expone el valor PageSize usado por esta capa de la aplicacion.
    public int PageSize { get; init; } = 40;
}
