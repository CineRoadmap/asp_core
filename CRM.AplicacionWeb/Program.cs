// Archivo: CRM.AplicacionWeb\Program.cs
// Punto de entrada de la aplicacion web: configura servicios, autenticacion, rutas y middleware.

// Importa el metodo de extension que registra servicios, repositorios y conexion a base de datos.
using CRM.Dependencia;
// Importa los contratos de la capa de proyecto, incluido el inicializador de base de datos.
using CRM.Proyecto.Contracts;
// Importa la autenticacion por cookies que se usa para mantener la sesion del usuario.
using Microsoft.AspNetCore.Authentication.Cookies;

// Crea el builder principal de ASP.NET Core con configuracion, logging, servicios y argumentos de arranque.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Registra el sistema de autenticacion de la aplicacion.
builder.Services
    // Indica que el esquema por defecto para autenticar usuarios sera una cookie.
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    // Configura las opciones concretas de la cookie de sesion.
    .AddCookie(options =>
    {
        // Ruta a la que se redirige al usuario cuando intenta entrar sin estar autenticado.
        options.LoginPath = "/Account/Login";
        // Ruta a la que se redirige cuando el usuario autenticado no tiene permisos suficientes.
        options.AccessDeniedPath = "/Account/Login";
        // Nombre fisico de la cookie que se guarda en el navegador.
        options.Cookie.Name = "CineRoadMap.Auth";
        // Renueva la duracion de la sesion mientras el usuario sigue usando la aplicacion.
        options.SlidingExpiration = true;
    });

// Activa el sistema de autorizacion para poder usar atributos como [Authorize].
builder.Services.AddAuthorization();
// Registra MVC con controladores y vistas Razor.
builder.Services.AddControllersWithViews();
// Registra las dependencias propias de CineRoadMap usando la configuracion cargada desde appsettings.
builder.Services.AddCineRoadMapCore(builder.Configuration);

// Construye la aplicacion final con todos los servicios registrados.
var app = builder.Build();

// Crea un scope temporal para resolver servicios scoped durante el arranque.
using (var scope = app.Services.CreateScope())
{
    // Obtiene el inicializador de base de datos desde el contenedor de dependencias.
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    // Ejecuta la inicializacion: crea/asegura datos necesarios como catalogo base y progreso de usuarios.
    await initializer.InitializeAsync(CancellationToken.None);
}

// En produccion se configura una pagina de error controlada y HSTS para HTTPS.
if (!app.Environment.IsDevelopment())
{
    // Redirige errores no controlados a la accion Home/Error en vez de mostrar detalles tecnicos.
    app.UseExceptionHandler("/Home/Error");
    // Indica al navegador que debe usar HTTPS para futuras peticiones al dominio.
    app.UseHsts();
}

// Redirige peticiones HTTP a HTTPS cuando hay puerto HTTPS configurado.
app.UseHttpsRedirection();
// Activa el sistema de enrutado antes de autenticacion y autorizacion.
app.UseRouting();
// Lee la cookie de sesion y rellena HttpContext.User si el usuario esta autenticado.
app.UseAuthentication();
// Aplica reglas de permisos y atributos [Authorize] sobre las rutas.
app.UseAuthorization();

// Expone archivos estaticos optimizados por el sistema de static web assets.
app.MapStaticAssets();

// Define la ruta MVC por defecto: controlador, accion e id opcional.
app.MapControllerRoute(
    // Nombre interno de la ruta.
    name: "default",
    // Patron que hace que / vaya a Home/Index y que /Movies/Details/5 resuelva controlador, accion e id.
    pattern: "{controller=Home}/{action=Index}/{id?}")
    // Asocia tambien los assets estaticos generados a esta ruta.
    .WithStaticAssets();

// Arranca el servidor web y mantiene la aplicacion escuchando peticiones.
app.Run();
