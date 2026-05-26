// Archivo: CRM.Proyecto\Security\PasswordCodec.cs
// Utilidad de seguridad para codificar y verificar contrasenas de usuario.
using System.Security.Cryptography;

namespace CRM.Proyecto.Security;

// Representa la responsabilidad de PasswordCodec dentro de la aplicacion.
public static class PasswordCodec
{
    // Define la constante SaltSize usada por la clase.
    private const int SaltSize = 16;
   
    // Define la constante HashSize usada por la clase.
    private const int HashSize = 32;
   
    // Define la constante Iterations usada por la clase.
    private const int Iterations = 100_000;

    // Genera un hash PBKDF2 con sal aleatoria para guardar contrasenas de forma segura.
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    // Verifica una contrasena contra hashes PBKDF2 almacenados por la aplicacion.
    public static bool Verify(string password, string encodedHash)
    {
        if (string.IsNullOrWhiteSpace(encodedHash))
        {
            return false;
        }

        var parts = encodedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var currentHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(currentHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
