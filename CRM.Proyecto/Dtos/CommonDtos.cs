// Archivo: CRM.Proyecto\Dtos\CommonDtos.cs
// DTOs utilizados para mover datos entre capas sin exponer directamente las entidades.
namespace CRM.Proyecto.Dtos;

// Transporta los datos de AuthenticatedUserDto entre capas.
public sealed record AuthenticatedUserDto(int Id, string UserName, string NickName, string Email);

// Transporta los datos de GenreDto entre capas.
public sealed record GenreDto(int Id, string Name);

// Transporta los datos de PagedResult entre capas.
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalItems)
{
    // Expone el valor TotalPages usado por esta capa de la aplicacion.
    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
