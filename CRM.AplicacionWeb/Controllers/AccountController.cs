// Archivo: CRM.AplicacionWeb\Controllers\AccountController.cs
// Controlador MVC que recibe peticiones web y coordina la respuesta de la vista usando los servicios de negocio.

using System.Security.Claims;
using CRM.AplicacionWeb.Models.Account;
using CRM.Proyecto.Contracts;
using CRM.Proyecto.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AplicacionWeb.Controllers;


// Representa la responsabilidad de AccountController dentro de la aplicacion.
public sealed class AccountController : Controller
{
    // Guarda la dependencia _accountService recibida por inyeccion.
    private readonly IAccountService _accountService;
    
    // Inicializa AccountController con las dependencias necesarias.
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // Muestra el formulario de login o redirige al inicio si el usuario ya tiene sesion activa.
    // Ejecuta la operacion Login con los parametros recibidos.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Acceso";
        return View(new LoginViewModel());
    }

    // Valida credenciales, crea las claims de autenticacion y abre la sesion con cookie.
    // Ejecuta la operacion Login con los parametros recibidos.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Acceso";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.LoginAsync(new LoginRequest
        {
            UserName = model.UserName,
            Password = model.Password
        }, cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
            new(ClaimTypes.Name, result.User.UserName),
            new("nick", result.User.NickName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        TempData["Flash"] = $"Bienvenido de vuelta, {result.User.NickName}.";
        return RedirectToAction("Index", "Home");
    }

    // Muestra el formulario de registro o evita registrar otra cuenta si ya hay sesion iniciada.
    // Ejecuta la operacion Register con los parametros recibidos.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Registro";
        return View(new RegisterViewModel());
    }

    // Crea un usuario nuevo y prepara sus logros y retos iniciales.
    // Ejecuta la operacion Register con los parametros recibidos.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Registro";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.RegisterAsync(new RegisterUserRequest
        {
            UserName = model.UserName,
            NickName = model.NickName,
            Email = model.Email,
            Phone = model.Phone,
            Password = model.Password
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        TempData["Flash"] = "Cuenta creada. Ya puedes iniciar sesiÃ³n con tus nuevas credenciales.";
        return RedirectToAction(nameof(Login));
    }

    // Muestra el formulario para cambiar una contrasena olvidada.
    // Ejecuta la operacion ForgotPassword con los parametros recibidos.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Recuperar contrasena";
        return View(new ForgotPasswordViewModel());
    }

    // Verifica usuario y email, y guarda la nueva contrasena.
    // Ejecuta la operacion ForgotPassword con los parametros recibidos.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Recuperar contrasena";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserName = model.UserName,
            Email = model.Email,
            NewPassword = model.NewPassword
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Flash"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    // Cierra la cookie de autenticacion y devuelve al usuario al inicio.
    // Ejecuta la operacion Logout con los parametros recibidos.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Flash"] = "Has cerrado sesiÃ³n.";
        return RedirectToAction("Index", "Home");
    }
}
