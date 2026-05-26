// Archivo: CRM.AplicacionWeb\Models\Account\LoginViewModel.cs
// Modelo de vista con los datos que introduce el usuario para iniciar sesion.

using System.ComponentModel.DataAnnotations;

namespace CRM.AplicacionWeb.Models.Account;


// Representa la responsabilidad de LoginViewModel dentro de la aplicacion.
public sealed class LoginViewModel
{
    // Expone el valor UserName usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce tu usuario.")]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    // Expone el valor Password usado por esta capa de la aplicacion.
    [Required(ErrorMessage = "Introduce tu contraseÃ±a.")]
    [DataType(DataType.Password)]
    [Display(Name = "ContraseÃ±a")]
    public string Password { get; set; } = string.Empty;
}
