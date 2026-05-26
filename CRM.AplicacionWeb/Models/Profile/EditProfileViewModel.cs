// Archivo: CRM.AplicacionWeb\Models\Profile\EditProfileViewModel.cs
// Modelo de vista para editar los datos publicos de la cuenta.

using System.ComponentModel.DataAnnotations;

namespace CRM.AplicacionWeb.Models.Profile;

// Representa la responsabilidad de EditProfileViewModel dentro de la aplicacion.
public sealed class EditProfileViewModel
{
    // Expone el valor NickName usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce un nick.")]
    [Display(Name = "Nick")]
    public string NickName { get; set; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce un email.")]
    [EmailAddress(ErrorMessage = "Introduce un email valido.")]
    public string Email { get; set; } = string.Empty;

    // Expone el valor Phone usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce un telefono.")]
    [Display(Name = "Telefono")]
    public string Phone { get; set; } = string.Empty;
}
