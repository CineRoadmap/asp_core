// Archivo: CRM.AplicacionWeb\Models\Account\RegisterViewModel.cs
// Modelo de vista con los datos necesarios para registrar una nueva cuenta.

using System.ComponentModel.DataAnnotations;

namespace CRM.AplicacionWeb.Models.Account;

// Representa la responsabilidad de RegisterViewModel dentro de la aplicacion.
public sealed class RegisterViewModel
{
    // Expone el valor UserName usado por esta capa de la aplicacion.
    [Required]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    // Expone el valor NickName usado por esta capa de la aplicacion.
    [Required]
    [Display(Name = "Nick")]
    public string NickName { get; set; } = string.Empty;

    // Expone el valor Email usado por esta capa de la aplicacion.
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Expone el valor Phone usado por esta capa de la aplicacion.
    [Required]
    [Display(Name = "TelÃ©fono")]
    public string Phone { get; set; } = string.Empty;

    // Expone el valor Password usado por esta capa de la aplicacion.
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    [Display(Name = "ContraseÃ±a")]
    public string Password { get; set; } = string.Empty;

    // Expone el valor ConfirmPassword usado por esta capa de la aplicacion.
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseÃ±as no coinciden.")]
    [Display(Name = "Repetir contraseÃ±a")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
