using System.Security.Cryptography;
using System.Text;

namespace SubtitleEditor.ActivationKeyGenerator.Infrastructure;

public class EncryptService
{
    protected static readonly int _saltSize = 4;
    protected static readonly string _authTokenKey = "5p2M{D[/";

    public static string Encrypt(string? rawText)
    {
        return Encrypt(rawText, _authTokenKey);
    }

    public static string Encrypt(string? rawText, string key)
    {
        var originalBytes = Encoding.UTF8.GetBytes(rawText ?? "");
        var passwordBytes = Encoding.UTF8.GetBytes(key);
        byte[]? encryptedBytes = null;

        using (var sha256 = SHA256.Create())
        {
            passwordBytes = sha256.ComputeHash(passwordBytes);

            var saltBytes = GetRandomBytes();
            var bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
            for (var i = 0; i < saltBytes.Length; i++)
            {
                bytesToBeEncrypted[i] = saltBytes[i];
            }

            for (var i = 0; i < originalBytes.Length; i++)
            {
                bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];
            }

            encryptedBytes = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    protected static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[]? encryptedBytes = null;

        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (var ms = new MemoryStream())
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;

            using (var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000))
            {
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
            }

            aes.Mode = CipherMode.CBC;

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            }
            encryptedBytes = ms.ToArray();
        }

        return encryptedBytes;
    }

    public static string Decrypt(string? encryptedText)
    {
        return Decrypt(encryptedText, _authTokenKey);
    }

    public static string Decrypt(string? encryptedText, string key)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            throw new Exception("Encrypted Text is empty!");
        }

        var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
        var passwordBytes = Encoding.UTF8.GetBytes(key);
        byte[]? originalBytes = null;

        using (var sha256 = SHA256.Create())
        {
            passwordBytes = sha256.ComputeHash(passwordBytes);

            var decryptedBytes = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            originalBytes = new byte[decryptedBytes.Length - _saltSize];
            for (var i = _saltSize; i < decryptedBytes.Length; i++)
            {
                originalBytes[i - _saltSize] = decryptedBytes[i];
            }
        }

        return Encoding.UTF8.GetString(originalBytes);
    }

    public static string GenerateNewKey(int length)
    {
        const string alphas = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "1234567890";
        const string sp = "@!#$%&*^?[]{}/<>";

        var selecteds = new List<char>();
        var random = new Random();

        for (var i = 0; i < 2; i++)
        {
            selecteds.Add(sp[random.Next(sp.Length)]);
        }

        var numberLength = (int)Math.Round(((double)length - 2) / 3);
        for (var i = 0; i < numberLength; i++)
        {
            selecteds.Add(numbers[random.Next(numbers.Length)]);
        }

        for (var i = 0; i < length - numberLength - 2; i++)
        {
            selecteds.Add(alphas[random.Next(alphas.Length)]);
        }

        return new string(selecteds.OrderBy(o => random.Next()).ToArray());
    }

    protected static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[]? decryptedBytes = null;

        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (var ms = new MemoryStream())
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;

            using (var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000))
            {
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
            }

            aes.Mode = CipherMode.CBC;

            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
            }
            decryptedBytes = ms.ToArray();
        }

        return decryptedBytes;
    }

    protected static byte[] GetRandomBytes()
    {
        var ba = new byte[_saltSize];
        using (var randomGen = RandomNumberGenerator.Create())
        {
            randomGen.GetBytes(ba);
        }
        return ba;
    }
}
