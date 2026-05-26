// Archivo: CRM.AplicacionWeb\Models\Account\ForgotPasswordViewModel.cs
// Modelo de vista para recuperar una contrasena olvidada desde el login.

using System.ComponentModel.DataAnnotations;

namespace CRM.AplicacionWeb.Models.Account;

// Representa la responsabilidad de ForgotPasswordViewModel dentro de la aplicacion.
public sealed class ForgotPasswordViewModel
{
    // Expone el valor UserName usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce tu usuario.")]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce tu email.")]
    [EmailAddress(ErrorMessage = "Introduce un email valido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

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
