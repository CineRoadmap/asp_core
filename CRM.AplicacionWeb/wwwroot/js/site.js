// Archivo: CRM.AplicacionWeb\wwwroot\js\site.js
// Script global reservado para comportamientos comunes de la aplicacion.

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

window.addEventListener('pageshow', (event) => {
    // Recarga la comunidad al volver desde cache para mostrar ranking y perfiles actualizados.
    if (event.persisted && window.location.pathname.toLowerCase().startsWith('/community')) {
        window.location.reload();
    }
});
