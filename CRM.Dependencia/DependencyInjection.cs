// Archivo: CRM.Dependencia\DependencyInjection.cs
// Registro central de inyeccion de dependencias entre contratos, servicios y repositorios.

// Servicios de la capa de control: contienen reglas de negocio y coordinan repositorios.
using CRM.Control.Services;
// Fabrica de conexiones MySQL usada por los repositorios Dapper.
using CRM.Datos.Context;
// Servicios de importacion y administracion del catalogo externo.
using CRM.Datos.Import;
// Repositorios concretos que ejecutan consultas contra MySQL.
using CRM.Datos.Repositories;
// Inicializador que asegura datos base y progreso al arrancar.
using CRM.Datos.Seed;
// Interfaces compartidas entre capas para desacoplar implementaciones.
using CRM.Proyecto.Contracts;
// Acceso a appsettings y cadenas de conexion.
using Microsoft.Extensions.Configuration;
// Tipos del contenedor de inyeccion de dependencias de .NET.
using Microsoft.Extensions.DependencyInjection;

// Agrupa los registros de dependencias de la aplicacion en un unico punto reutilizable.
namespace CRM.Dependencia;

// Clase estatica porque solo expone metodos de extension, no mantiene estado propio.
public static class DependencyInjection
{
    // Registra repositorios, servicios y dependencias compartidas de CineRoadMap en el contenedor DI.
    public static IServiceCollection AddCineRoadMapCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Lee la cadena de conexion "DefaultConnection" desde appsettings.json o variables de entorno.
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        // Si no existe cadena de conexion, se detiene el arranque porque la app depende de MySQL.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No se ha configurado la cadena de conexion 'DefaultConnection'.");
        }

        // Registra una unica fabrica de conexiones para toda la aplicacion; cada uso abre su propia conexion.
        services.AddSingleton<IConnectionFactory>(_ => new MySqlConnectionFactory(connectionString));

        // Repositorio de usuarios: login, registro, perfil y comunidad.
        services.AddScoped<IUserRepository, UserRepository>();
        // Repositorio de peliculas: catalogo, detalle, valoraciones, lista y dashboard.
        services.AddScoped<IMovieRepository, MovieRepository>();
        // Repositorio de logros: progreso e insignias del usuario.
        services.AddScoped<IAchievementRepository, AchievementRepository>();
        // Repositorio de retos: asignacion, progreso y estado de retos.
        services.AddScoped<IChallengeRepository, ChallengeRepository>();
        // Inicializador de base de datos ejecutado al arrancar la web.
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        // Servicio singleton porque mantiene el estado en memoria de la importacion TMDB en curso.
        services.AddSingleton<IAdminImportService, AdminImportService>();

        // Servicio de cuentas: valida login, registro y obtencion de usuario autenticado.
        services.AddScoped<IAccountService, AccountService>();
        // Servicio de inicio: prepara el dashboard principal.
        services.AddScoped<IHomeService, HomeService>();
        // Servicio de peliculas: valida acciones y coordina catalogo, lista y valoraciones.
        services.AddScoped<IMovieService, MovieService>();
        // Servicio de logros: expone el progreso de insignias a los controladores.
        services.AddScoped<IAchievementService, AchievementService>();
        // Servicio de retos: expone los retos del usuario a los controladores.
        services.AddScoped<IChallengeService, ChallengeService>();
        // Servicio de perfil: devuelve resumen y estadisticas del usuario.
        services.AddScoped<IProfileService, ProfileService>();
        // Servicio de comunidad: devuelve miembros y metricas publicas.
        services.AddScoped<ICommunityService, CommunityService>();

        // Devuelve la misma coleccion para permitir encadenar mas registros en Program.cs.
        return services;
    }
}
