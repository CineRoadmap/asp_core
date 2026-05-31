# Base de datos

## Orden de ejecucion

Acceder a la ruta de phpMyAdmin (`http://localhost:8081`), iniciar con el perfil root e ir a la sección de importar. Importar los datos en el orden siguiente:

1. `001_init_database.sql`: crea la base, tablas, relaciones y catalogos iniciales.
2. `002_create_database_user.sql`: crea el usuario tecnico usando variables de sesion.

## Diagrama de base de datos

El siguiente Modelo Entidad-Relación presenta la forma en la que está construida la base de datos a importar
[resources/img/MER_CineRoadmap.jpg](resources/img/MER_CineRoadmap.jpg)