// Archivo: CRM.Datos\Context\IConnectionFactory.cs
// Contrato para crear conexiones a base de datos sin acoplar los repositorios a MySQL directamente.

using MySqlConnector;

namespace CRM.Datos.Context;

// Define las operaciones disponibles para IConnectionFactory.

public interface IConnectionFactory
{
    // Crea y abre una conexion MySQL lista para ejecutar comandos Dapper.
    Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
