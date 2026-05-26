// Archivo: CRM.AplicacionWeb\Controllers\ProfileController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.
using System.Security.Claims;
using CRM.AplicacionWeb.Models.Profile;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CRM.AplicacionWeb.Controllers;

// Representa la responsabilidad de ProfileController dentro de la aplicacion.
[Authorize]
public sealed class ProfileController : Controller
{
    // Guarda la dependencia _profileService recibida por inyeccion.
    private readonly IProfileService _profileService;

    // Inicializa ProfileController con las dependencias necesarias.
    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    // Muestra el perfil del usuario autenticado con estadisticas y logros completados.
    // Ejecuta la operacion Index con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetProfileAsync(GetCurrentUserId()!.Value, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Mi perfil";
        return View(profile);
    }

    // Muestra una pantalla separada para editar solo los datos publicos del perfil.
    // Ejecuta la operacion Edit con los parametros recibidos.
    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetProfileAsync(GetCurrentUserId()!.Value, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Editar perfil";
        return View(new EditProfileViewModel
        {
            NickName = profile.NickName,
            Email = profile.Email,
            Phone = profile.Phone
        });
    }

    // Guarda los datos editables del perfil.
    // Ejecuta la operacion Edit con los parametros recibidos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Editar perfil";
            return View(model);
        }

        var userId = GetCurrentUserId()!.Value;
        var result = await _profileService.UpdateProfileAsync(new UpdateProfileRequest
        {
            UserId = userId,
            NickName = model.NickName,
            Email = model.Email,
            Phone = model.Phone
        }, cancellationToken);

        TempData["Flash"] = result.Message;
        if (result.Succeeded)
        {
            await RefreshAuthenticationClaimsAsync(userId, cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    // Cambia la contrasena del usuario autenticado.
    // Ejecuta la operacion ChangePassword con los parametros recibidos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Flash"] = "Revisa los datos de la contrasena.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _profileService.ChangePasswordAsync(new ChangePasswordRequest
        {
            UserId = GetCurrentUserId()!.Value,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword
        }, cancellationToken);

        TempData["Flash"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // Muestra el perfil publico de otro miembro de la comunidad.
    // Ejecuta la operacion UserProfile con los parametros recibidos.
    [HttpGet("Profile/User/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> UserProfile(int id, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetPublicProfileAsync(id, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Perfil de {profile.Summary.NickName}";
        return View("Public", profile);
    }

    // Lee el identificador de usuario desde las claims de la sesion actual.
    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    // Ejecuta la operacion RefreshAuthenticationClaimsAsync con los parametros recibidos.
    private async Task RefreshAuthenticationClaimsAsync(int userId, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetProfileAsync(userId, cancellationToken);
        if (profile is null)
        {
            return;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new(ClaimTypes.Name, profile.UserName),
            new("nick", profile.NickName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
