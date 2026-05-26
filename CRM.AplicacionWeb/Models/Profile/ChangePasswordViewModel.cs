// Archivo: CRM.AplicacionWeb\Models\Profile\ChangePasswordViewModel.cs
// Modelo de vista para cambiar la contrasena del usuario autenticado.

using System.ComponentModel.DataAnnotations;

namespace CRM.AplicacionWeb.Models.Profile;

// Representa la responsabilidad de ChangePasswordViewModel dentro de la aplicacion.
public sealed class ChangePasswordViewModel
{
    // Expone el valor CurrentPassword usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce tu contrasena actual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contrasena actual")]
    public string CurrentPassword { get; set; } = string.Empty;

    // Expone el valor NewPassword usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce la nueva contrasena.")]
    [MinLength(8, ErrorMessage = "La nueva contrasena debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contrasena")]
    public string NewPassword { get; set; } = string.Empty;

    // Expone el valor ConfirmNewPassword usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Repite la nueva contrasena.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contrasenas no coinciden.")]
    [Display(Name = "Repetir nueva contrasena")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
