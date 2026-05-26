// Archivo: CRM.Datos\Context\DapperRepositoryBase.cs
// Clase base para repositorios Dapper con helpers comunes de conexion y consulta.

using MySqlConnector;

namespace CRM.Datos.Context;

// Representa la responsabilidad de DapperRepositoryBase dentro de la aplicacion.

public abstract class DapperRepositoryBase
{
    // Guarda la dependencia ConnectionFactory recibida por inyeccion.
    protected readonly IConnectionFactory ConnectionFactory;

    protected DapperRepositoryBase(IConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory;
    }

    // Abre una conexion MySQL reutilizando la fabrica comun de la capa de datos.
    protected async Task<MySqlConnection> OpenConnectionAsync(CancellationToken cancellationToken) =>
        await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
}
