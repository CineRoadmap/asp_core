// Archivo: CRM.Datos\Context\MySqlConnectionFactory.cs
// Implementacion de fabrica de conexiones MySQL usando la cadena configurada.

using MySqlConnector;

namespace CRM.Datos.Context;

// Representa la responsabilidad de MySqlConnectionFactory dentro de la aplicacion.

public sealed class MySqlConnectionFactory : IConnectionFactory
{
    // Guarda la dependencia _connectionString recibida por inyeccion.
    private readonly string _connectionString;

    // Inicializa MySqlConnectionFactory con las dependencias necesarias.
    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Instancia MySqlConnection con la cadena configurada y la devuelve abierta.
    public async Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
