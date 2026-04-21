using System.Security.Cryptography;
using System.Text;

namespace MotoRevApi.Services;

public class HashService
{
    public virtual string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }

    public virtual bool VerifyToken(string providedToken, string storedHash)
    {
        if (string.IsNullOrEmpty(providedToken) || string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var hashOfProvidedToken = HashToken(providedToken);
        
        // Decodificar de Base64 para byte array antes da comparação
        var providedHashBytes = Convert.FromBase64String(hashOfProvidedToken);
        var storedHashBytes = Convert.FromBase64String(storedHash);

        return CryptographicOperations.FixedTimeEquals(providedHashBytes, storedHashBytes);
    }
}
