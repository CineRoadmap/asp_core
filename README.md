# CineRoadMap

CineRoadMap es una aplicación web ASP.NET Core MVC para registrar películas vistas, valoraciones, retos, logros y recomendaciones de cine. El proyecto está organizado por capas para separar interfaz, reglas de negocio, acceso a datos, contratos y entidades.

## Estructura

- `CRM.AplicacionWeb`: controladores, vistas Razor, estilos y configuración de la web.
- `CRM.Control`: servicios de negocio y validaciones de uso.
- `CRM.Datos`: repositorios Dapper, inicialización de datos e importación TMDB.
- `CRM.Entidad`: entidades y enumeraciones del dominio.
- `CRM.Proyecto`: contratos, DTOs, requests y utilidades compartidas.
- `CRM.Dependencia`: registro centralizado de inyección de dependencias.
- `CRM.ImportadorTmdb`: herramienta de consola para importar catálogo desde TMDB.
- `Scripts_BD`: scripts SQL de base de datos, usuario técnico y mantenimiento.

## Requisitos

- .NET SDK 10.0.100 o superior dentro de la familia .NET 10 para ejecutar el proyecto sin Docker. Los proyectos usan `TargetFramework` `net10.0`.
- Docker Desktop para levantar MySQL, phpMyAdmin y la aplicación con Compose.
- Una API key de TMDB solo para importar catálogo real.

## Configuración local

El repositorio incluye `.env.example` como plantilla. Cópialo a `.env` para trabajar en local y rellena ahí las credenciales de desarrollo o la API key real de TMDB si vas a importar catálogo externo. El archivo `.env` queda ignorado por Git para evitar subir secretos.

Para Docker, revisa `.env` y ejecuta `docker compose up --build`.

## Manual de instalación y uso

### Instalación

1. Instalar Docker Desktop y comprobar que está en ejecución.
2. Clonar o descargar el proyecto en el equipo.
3. Copiar `.env.example` como `.env`.
4. Revisar las variables de `.env`, especialmente las contraseñas de MySQL y `TMDB_API_KEY` si se quiere importar catálogo real desde TMDB.
5. Abrir una terminal en la carpeta del proyecto.

### Ejecución

En una terminal de Docker ejecutar el comando:

```powershell
docker compose up --build
```

1. Web: `http://localhost:8080` (Importante: Antes de acceder a la web, debes importar la base de datos en la base de datos, tal como se especifica en [Scripts_BD/README.md](Scripts_BD/README.md))
2. phpMyAdmin: `http://localhost:8081` (solo accesible desde la máquina local)

### Uso de la aplicación

1. Entrar en `http://localhost:8080`.
2. Iniciar sesión con un usuario existente o crear una cuenta desde registro.
3. Consultar el catálogo de películas desde la sección de películas.
4. Abrir el detalle de una película para ver información, géneros, reparto y valoraciones.
5. Valorar películas vistas con una puntuación entre 1 y 5.
6. Añadir o quitar películas de la lista pendiente.
7. Revisar retos para ver progreso, estado y puntos.
8. Consultar logros para comprobar insignias desbloqueadas y progreso.
9. Abrir el perfil para ver estadísticas personales y editar datos.
10. Entrar en comunidad para consultar ranking y perfiles públicos.
11. Usar el panel de administración solo con usuario autorizado para revisar el catálogo o importar desde TMDB.

### Apagado

Para detener la aplicación:

```powershell
docker compose down
```

## Pruebas

La estrategia de pruebas está descrita en [docs/PLAN_PRUEBAS.md](docs/PLAN_PRUEBAS.md).

## Estilo de código

Las reglas de comentarios, nombres de variables y convenciones entre capas están en [docs/ESTILOS_CODIGO.md](docs/ESTILO_CODIGO.md).

## Respaldo y restauración

El plan de respaldo de MySQL está documentado en [docs/PLAN_RECUPERACION.md](docs/PLAN_RECUPERACION.md). Incluye comandos `mysqldump`, restauración y una checklist de verificación para la entrega.
