using System.Security.Cryptography;
using System.Text;

namespace SportChain.WebApi.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password).Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}
