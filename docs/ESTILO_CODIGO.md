# Guía de comentarios y nomenclatura

## Objetivo

Mantener todos los archivos de CineRoadMap con el mismo criterio de lectura: cada archivo debe explicar su responsabilidad y cada nombre debe describir el concepto de dominio que representa.

## Comentarios por archivo

- Los archivos C# empiezan con `// Archivo: ruta` y una segunda línea que resume su responsabilidad.
- Las vistas Razor empiezan con `@* Archivo: ruta ... *@`.
- Los CSS y JS propios empiezan con un comentario de archivo.
- Los Markdown, YAML y SQL propios empiezan con un título o comentario que indique su objetivo.
- No se comentan librerías externas dentro de `wwwroot/lib`.

## Comentarios dentro del código

- Comentar casos de uso, decisiones, reglas de negocio y consultas complejas.
- Evitar comentarios que repitan literalmente el código.
- Mantener los comentarios en castellano y sin depender de abreviaturas.
- Usar comentarios breves antes de bloques largos, especialmente en SQL, importación TMDB y cálculos de progreso.

## Nomenclatura C#

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Clases, records e interfaces | PascalCase | `MovieService`, `IUserRepository` |
| Métodos públicos | PascalCase | `GetProfileAsync` |
| Variables locales | camelCase descriptivo | `currentUserId`, `watchedMovies` |
| Campos privados | `_camelCase` | `_profileService` |
| DTOs | Sufijo `Dto` | `CommunityMemberDto` |
| Requests | Sufijo `Request` | `ResetPasswordRequest` |
| ViewModels | Sufijo `ViewModel` | `ForgotPasswordViewModel` |
| Filas auxiliares de Dapper | Sufijo `Row` | `CommunityMemberRow` |
| Métodos asíncronos | Sufijo `Async` | `UpdatePasswordAsync` |

## Nombres recomendados

- Usar nombres del dominio: `movie`, `achievement`, `challenge`, `member`, `profile`, `rating`.
- Evitar `x`, `data`, `temp` o `rows` si el alcance no es trivial.
- Preferir `communityRows`, `watchedMovies`, `unlockedAchievements` o `assignedChallenges`.
- Mantener inglés en código C# porque los DTOs, entidades y propiedades ya siguen esa convención.
- Mantener castellano en textos visibles, comentarios, documentos y nombres de tablas existentes.

## Reglas de escritura

- Mantener funciones cortas cuando la regla de negocio lo permita.
- Evitar más de tres niveles de anidación; extraer métodos privados si mejora la lectura.
- Usar constantes para valores fijos de seguridad, paginación, hashes o configuración.
- Gestionar errores con validaciones, resultados de servicio o excepciones controladas.
- Preferir variables inmutables por defecto y reasignar solo cuando sea necesario.

## Vistas Razor

- El `@model` debe describir lo que consume la vista.
- Las variables del bloque `@{ }` deben tener nombres visibles de dominio.
- Evitar formularios mezclados en una misma pantalla si pertenecen a flujos distintos.
- Usar enlaces no clicables cuando un elemento solo debe verse, como el usuario actual en el ranking.

## Base de datos

- Los nombres de tablas existentes se mantienen en castellano para no romper scripts ni consultas.
- Las consultas Dapper deben usar parámetros, nunca concatenar datos de usuario.
- Los alias SQL deben mapear a propiedades C# en PascalCase.
- Las columnas heredadas con acentos o nombres antiguos se aíslan con helpers como `GetPasswordColumnNameAsync`.

## Docker y configuración

- Los compose deben incluir comentarios de propósito al inicio.
- Las variables `.env` deben ser descriptivas y coincidir con los nombres usados por Docker.
- No guardar API keys reales ni contraseñas de producción.
