// Archivo: CRM.AplicacionWeb\Models\Community\CommunityIndexViewModel.cs
// Modelo de vista para separar ranking visible y perfiles visitables de comunidad.

using CRM.Proyecto.Dtos;

namespace CRM.AplicacionWeb.Models.Community;

// Representa la responsabilidad de CommunityIndexViewModel dentro de la aplicacion.
public sealed class CommunityIndexViewModel
{
    // Expone el valor CurrentUserId usado por esta capa de la aplicacion.
    public int? CurrentUserId { get; init; }

    // Ejecuta la operacion Ranking con los parametros recibidos.
    public IReadOnlyCollection<CommunityMemberDto> Ranking { get; init; } = Array.Empty<CommunityMemberDto>();

    // Ejecuta la operacion Profiles con los parametros recibidos.
    public IReadOnlyCollection<CommunityMemberDto> Profiles { get; init; } = Array.Empty<CommunityMemberDto>();
}
