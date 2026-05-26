// Archivo: CRM.AplicacionWeb\Models\ErrorViewModel.cs
// Modelo sencillo para mostrar informacion de errores en la pagina compartida de error.

namespace CRM.AplicacionWeb.Models
{
    // Representa la responsabilidad de ErrorViewModel dentro de la aplicacion.
    public class ErrorViewModel
    {
        // Expone el valor RequestId usado por esta capa de la aplicacion.
        public string? RequestId { get; set; }

        // Ejecuta la operacion ShowRequestId con los parametros recibidos.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
