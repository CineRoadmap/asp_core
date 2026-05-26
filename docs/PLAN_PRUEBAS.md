Aquí tienes el **Plan de pruebas de CineRoadMap** corregido minuciosamente, añadiendo todas las **tildes (acentos)**, la **letra ñ** y los caracteres correspondientes (*contraseña, autenticación, inicialización, administración, año, película*, etc.) para que mantenga una ortografía impecable en tu memoria de proyecto.

---

# Plan de pruebas de CineRoadMap

## Objetivo

Validar que CineRoadMap funciona correctamente en Docker, que los flujos principales guardan datos en MySQL y que las pantallas de autenticación, perfil, comunidad, catálogo, retos, logros y administración mantienen el comportamiento esperado.

## Alcance

Este plan cubre:

* Arranque de infraestructura con Docker Compose.
* Inicialización de base de datos y datos semilla.
* Login, registro, cierre de sesión y recuperación de contraseña.
* Perfil propio, edición de datos y perfiles públicos.
* Ranking y perfiles de comunidad.
* Catálogo, detalle, valoraciones y lista pendiente.
* Retos, logros y progreso.
* Panel de administración e importación TMDB.
* Pruebas de errores, seguridad básica y regresión.

Quedan fuera de las pruebas obligatorias las integraciones externas reales con TMDB cuando no exista `TMDB_API_KEY`.

## Entorno de pruebas

| Elemento | Valor esperado |
| --- | --- |
| Sistema | Docker Desktop activo |
| Base de datos | MySQL 8.4 en contenedor `cineroadmap-mysql` |
| Aplicación web | Contenedor `cineroadmap-web` |
| phpMyAdmin | Contenedor `cineroadmap-phpmyadmin` |
| URL web | `http://localhost:8080` |
| URL phpMyAdmin | `http://localhost:8081` |
| Puerto MySQL host | `3307` |
| Base de datos | `cineroadmap` |

## Preparación

1. Crear `.env` desde `.env.example` si no existe.
2. Comprobar que `.env` contiene `MYSQL_PASSWORD` y `MYSQL_ROOT_PASSWORD`.
3. Levantar el entorno completo:

```powershell
docker compose up -d --build

```

4. Comprobar servicios:

```powershell
docker compose ps

```

5. Acceder a phpMyAdmin para comprobar que se ha creado correctamente la base de datos e importar los datos y usuarios de los archivos indicados en [Scripts_DB](https://www.google.com/search?q=Scripts/README.md).
6. Abrir `http://localhost:8080` y confirmar que carga la página inicial.

## Datos mínimos esperados

| Tabla | Mínimo esperado |
| --- | --- |
| `usuarios` | Al menos 1 usuario demo |
| `peliculas` | Películas semilla cargadas |
| `generos` | Géneros asociados |
| `logros` | Catálogo de logros cargado |
| `catalogo_retos` | Retos base cargados |
| `usuario_retos` | Retos asignados a usuarios |

Consulta rápida:

```sql
SELECT 'usuarios' tabla, COUNT(*) total FROM usuarios
UNION ALL SELECT 'peliculas', COUNT(*) FROM peliculas
UNION ALL SELECT 'logros', COUNT(*) FROM logros
UNION ALL SELECT 'catalogo_retos', COUNT(*) FROM catalogo_retos;

```

## Pruebas de autenticación

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Login válido | Ir a `/Account/Login`, introducir usuario y contraseña válidos | Redirige a inicio y aparece sesión iniciada. |
| Login inválido | Introducir usuario correcto y contraseña incorrecta | Muestra error y no crea sesión. |
| Campos vacíos en login | Enviar formulario vacío | Muestra validaciones de usuario y contraseña. |
| Cierre de sesión | Iniciar sesión y pulsar logout | Cierra cookie y vuelve a inicio. |
| Usuario autenticado abre login | Ir a `/Account/Login` con sesión activa | Redirige a `Home/Index`. |

## Recuperación de contraseña

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Enlace visible | Abrir login | Aparece `Olvidé mi contraseña`. |
| Abrir recuperación | Pulsar el enlace | Carga `/Account/ForgotPassword`. |
| Recuperación correcta | Introducir usuario, email correcto, nueva contraseña y confirmación | Actualiza la contraseña y redirige al login con mensaje de éxito. |
| Login con contraseña nueva | Iniciar sesión con la nueva contraseña | Login correcto. |
| Login con contraseña antigua | Intentar iniciar sesión con la contraseña anterior | Login rechazado. |
| Email incorrecto | Introducir usuario válido y email que no corresponde | Muestra error y no cambia la contraseña. |
| Usuario inexistente | Introducir usuario no registrado | Muestra error y no cambia datos. |
| Contraseña corta | Usar menos de 8 caracteres | Muestra validación. |
| Confirmación distinta | Nueva contraseña y repetición no coinciden | Muestra validación. |
| Sesión activa | Abrir recuperación con sesión iniciada | Redirige a inicio. |

Comprobación opcional en BD: el campo de contraseña del usuario debe cambiar y debe seguir guardado como hash, no en texto plano.

## Registro

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Registro válido | Crear cuenta con usuario, nick, email, teléfono y contraseña válida | Crea cuenta y redirige al login. |
| Usuario duplicado | Registrar un usuario ya existente | Muestra error. |
| Email duplicado | Registrar email ya existente | Muestra error. |
| Contraseña corta | Usar menos de 8 caracteres | Muestra validación. |
| Confirmación distinta | Repetir contraseña diferente | Muestra validación. |
| Datos iniciales | Revisar BD tras registro | Existen filas iniciales en `logros_usuario` y `usuario_retos`. |

## Perfil propio

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Ver perfil | Iniciar sesión y abrir `/Profile` | Muestra usuario, nick, email, teléfono, estadísticas e insignias. |
| Botón editar | En perfil, pulsar `Editar datos del perfil` | Abre pantalla separada de edición. |
| Editar datos válido | Cambiar nick, email y teléfono | Guarda, vuelve al perfil y muestra datos actualizados. |
| Email ya usado | Editar email usando el de otro usuario | Muestra error o no permite guardar. |
| Validaciones | Enviar campos obligatorios vacíos | Muestra validaciones. |
| Sin cambio de contraseña en perfil | Revisar perfil y pantalla de edición | No aparece formulario de cambio de contraseña en el perfil. |

## Comunidad y ranking

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Ranking anónimo | Abrir `/Community` sin sesión | Muestra top 10 con enlaces a perfiles públicos. |
| Ranking con sesión | Iniciar sesión y abrir `/Community` | El usuario actual aparece en el ranking si está dentro del top 10. |
| Usuario actual no clicable | En ranking, localizar el usuario actual | Su nombre aparece como texto, no como enlace. |
| Otros usuarios clicables | En ranking, pulsar otro usuario | Abre su perfil público. |
| Perfiles de comunidad | Revisar sección `Perfiles de la comunidad` con sesión iniciada | No aparece el usuario actual como perfil visitable. |
| Orden ranking | Comparar puntos visibles | Ordena por puntos, retos completados, valoraciones y usuario. |
| Puntos ranking | Revisar fórmula | `RankingPoints = retos completados * 100 + valoraciones * 10`. |
| Caché | Cambiar de usuario y volver a comunidad | No se mantiene el usuario anterior por caché. |

Consulta de apoyo:

```sql
SELECT u.usuario,
       COUNT(DISTINCT CASE WHEN ur.estado = 'COMPLETADO' THEN ur.reto_id END) * 100
       + COUNT(DISTINCT v.id) * 10 AS puntos
FROM usuarios u
LEFT JOIN usuario_retos ur ON ur.usuario_id = u.idUsuario
LEFT JOIN valoraciones v ON v.usuario_id = u.idUsuario
GROUP BY u.idUsuario, u.usuario
ORDER BY puntos DESC;

```

## Perfiles públicos

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Abrir perfil público | Desde comunidad, pulsar otro usuario | Muestra resumen público y películas vistas. |
| Usuario inexistente | Abrir `/Profile/User/999999` | Devuelve 404. |
| Datos sensibles | Revisar perfil público | No muestra contraseña ni datos privados no previstos. |

## Catálogo y películas

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Catálogo inicial | Abrir `/Movies` | Muestra películas con póster, título, año y datos principales. |
| Búsqueda texto | Buscar una película por nombre | Filtra resultados. |
| Filtro género | Seleccionar género | Muestra películas del género. |
| Filtro año | Filtrar por año | Muestra películas del año. |
| Paginación | Avanzar y retroceder página | Mantiene filtros y no duplica resultados. |
| Detalle | Abrir película | Muestra ficha, géneros, reparto, dirección y valoración media. |
| Película inexistente | Abrir ID inexistente | Devuelve 404. |

## Valoraciones y lista pendiente

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Puntuar película | Iniciar sesión y valorar de 1 a 5 | Guarda valoración y actualiza media. |
| Cambiar valoración | Valorar de nuevo la misma película | Actualiza la fila existente, no duplica. |
| Valoración inválida | Forzar puntuación fuera de 1 a 5 | Se rechaza por validación o constraint. |
| Anónimo valora | Intentar valorar sin sesión | Redirige a login o rechaza acción. |
| Agregar a lista | Pulsar agregar a pendientes | Inserta en `lista_pendientes`. |
| Quitar de lista | Pulsar de nuevo | Elimina o desactiva sin duplicados. |

Consultas de apoyo:

```sql
SELECT * FROM valoraciones WHERE usuario_id = <id_usuario>;
SELECT * FROM lista_pendientes WHERE usuario_id = <id_usuario>;

```

## Retos

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Ver retos | Abrir `/Challenges` con sesión | Muestra retos asignados, progreso, puntos y vencimiento. |
| Progreso tras valorar | Valorar película que cuenta para un reto | El progreso del reto se encuentra actualizado. |
| Reto completado | Cumplir objetivo | Cambia estado a completado y asigna puntos. |
| Sin sesión | Abrir retos sin login | Redirige a login. |

## Logros

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Ver logros | Abrir `/Achievements` con sesión | Muestra logros, progreso y completados. |
| Filtro logros | Buscar por texto | Filtra por nombre o descripción. |
| Progreso tras actividad | Valorar o completar acciones relacionadas | Actualiza `logros_usuario`. |
| Perfil muestra insignias | Completar logro y abrir perfil | La insignia aparece en `MIS INSIGNIAS`. |

## Administración TMDB

| Caso | Pasos | Resultado esperado |
| --- | --- | --- |
| Acceso admin | Abrir panel admin con usuario autorizado | Muestra estado de catálogo e importación. |
| API key vacía | Probar importación sin `TMDB_API_KEY` | Muestra error claro y no inicia importación. |
| Probe válido | Con key válida, probar película concreta | Devuelve datos controlados de TMDB. |
| Importación | Lanzar importación pequeña | Inserta o actualiza películas sin duplicados. |
| Error externo | Simular fallo de TMDB | Muestra error sin romper la app. |

## Seguridad básica

| Caso | Resultado esperado |
| --- | --- |
| Contraseñas | Nunca se muestran en vistas ni consultas visibles. |
| Hash de contraseña | BD guarda hash, no texto plano. |
| Perfil propio en ranking | Se ve en ranking, pero no tiene enlace clicable hacia perfil público. |
| Perfil propio en comunidad | No aparece como tarjeta visitable en perfiles de comunidad. |
| Caché comunidad | No conserva datos de otro usuario tras logout/login. |

## Pruebas de base de datos

Ejecutar después de flujos principales:

```sql
SELECT COUNT(*) FROM usuarios;
SELECT COUNT(*) FROM valoraciones;
SELECT COUNT(*) FROM lista_pendientes;
SELECT COUNT(*) FROM logros_usuario WHERE completado = 1;
SELECT COUNT(*) FROM usuario_retos WHERE estado = 'COMPLETADO';

```

Validar:

* No hay valoraciones duplicadas para el mismo usuario y película.
* No hay pendientes duplicados para el mismo usuario y película.
* Los usuarios nuevos tienen progreso de logros creado.
* Los usuarios nuevos tienen retos iniciales asignados.
* Los cambios de perfil se reflejan en `usuarios`.

## Checklist de aceptación

* La app arranca desde Docker sin errores.
* Login, registro y recuperación de contraseña funcionan.
* El perfil propio se edita desde pantalla separada.
* El cambio de contraseña olvidada se hace desde login.
* El ranking muestra al usuario actual si está en top 10.
* El usuario actual del ranking no es clicable.
* La sección de perfiles de comunidad no muestra al usuario actual como perfil visitable.
* Catálogo, valoraciones, retos y logros actualizan datos en MySQL.
* No hay contraseñas en texto plano.
* Las pruebas manuales críticas quedan documentadas antes de la entrega.