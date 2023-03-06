using System.Security.Cryptography;
using System.Text;
namespace MAP.Services;

public interface PasswordHasher
{
    Task<string> Hash(string password);
}


public class PasswordHasherImpl : PasswordHasher
{
    public async Task<string> Hash(string password)
    {
        using var sh = SHA256.Create();
        using var memStream = new MemoryStream(Encoding.UTF8.GetBytes(password));
        var hash = Encoding.UTF8.GetString(await sh.ComputeHashAsync(memStream));
        return hash;
    }
}