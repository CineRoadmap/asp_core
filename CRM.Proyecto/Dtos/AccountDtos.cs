// Archivo: CRM.Proyecto\Dtos\AccountDtos.cs
// DTOs utilizados para mover datos entre capas sin exponer directamente las entidades.

namespace CRM.Proyecto.Dtos;


// Transporta los datos de LoginResultDto entre capas.

public sealed record LoginResultDto(bool Succeeded, string ErrorMessage, AuthenticatedUserDto? User)
{
    // Ejecuta la operacion Success con los parametros recibidos.
    public static LoginResultDto Success(AuthenticatedUserDto user) => new(true, string.Empty, user);

    // Ejecuta la operacion Failure con los parametros recibidos.
    public static LoginResultDto Failure(string errorMessage) => new(false, errorMessage, null);
}


// Transporta los datos de RegisterResultDto entre capas.
public sealed record RegisterResultDto(bool Succeeded, string ErrorMessage)
{
    // Ejecuta la operacion Success con los parametros recibidos.
    public static RegisterResultDto Success() => new(true, string.Empty);

    // Ejecuta la operacion Failure con los parametros recibidos.
    public static RegisterResultDto Failure(string errorMessage) => new(false, errorMessage);
}


// Transporta los datos de AccountActionResultDto entre capas.

public sealed record AccountActionResultDto(bool Succeeded, string Message)
{
    // Ejecuta la operacion Success con los parametros recibidos.
    public static AccountActionResultDto Success(string message) => new(true, message);

    // Ejecuta la operacion Failure con los parametros recibidos.
    public static AccountActionResultDto Failure(string message) => new(false, message);
}
