using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace RentCarX.Application.Helpers;

internal static class TagHasher
{
    public static string UseSHA256(string claim)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(claim);
        var sha256 = SHA256.Create();
        var computedBytes = sha256.ComputeHash(bytes);

        return BitConverter.ToString(computedBytes)
            .Replace("-", "")
            .ToLowerInvariant();
    }
}
