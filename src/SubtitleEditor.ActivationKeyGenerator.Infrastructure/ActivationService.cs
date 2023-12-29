using RSAExtensions;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SubtitleEditor.ActivationKeyGenerator.Infrastructure;

public class ActivationService
{
    private static string _publicKey => $"<RSAKeyValue><Modulus>{ActivationKeys.Publish}</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    private static string _privateKey => $"<RSAKeyValue><Modulus>{ActivationKeys.Publish}</Modulus><Exponent>AQAB</Exponent>{_private}</RSAKeyValue>";

    private const string _private = "<P>7mADggYvBVKn4GNGM/CNrc7sHf9oEsp1x+kM/Dv5xH5wZGEfeb3ZDFf/zXveqLYuJmWyGCgjVYiei5IzFQoUFw==</P><Q>6ClcpDQ+tYX7kijs8rtpnjitKEeZXgHxYWanqKLjP/FIx86LtvJsNf9VsvuFr0lX0VD8WdLHMKq56ZO3pb9Grw==</Q><DP>PsssIWRfnpdXgdSk+am0qMSJjp1pXQnYXQEXWpqyAQENumObVwab1pSX3hlzXh4fqh2//H0WUcHbggjRTAmLoQ==</DP><DQ>z+5ju+nTFG+IzvJ6rjgus3gdlkryUOE6mLsBhKdHE7j+L36NYfCj/ITQ87oUlNcCoUWAjs6aerseQdZ8kCXNnw==</DQ><InverseQ>BEDIX/dW2OAEUYbHGi6EybUrGz3XkyGfiFjo6XQkopI2M0TQbOjTyqojYNAVc2PYITAxvaTj0T+m13C+YDJ1PQ==</InverseQ><D>sOD3Wfxo9uQSS0MB+W0cbpnAJQQhADNZgHRClIvLsnuMmkLNhz/TyAPIF+vq/aHW0wOpAiZPG/+vi4Wpl9p5To4SLwp4T6S1nIU224rCqlK+m61+ebwxpAd7CEH3jkbSulT25pJJ0gYbORre9YqP6UEeoXIqOFJ8hZxHS9Mqq20=</D>";

    public static ActivationData? ResolveKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || !key.Contains(":::"))
        {
            return null;
        }

        try
        {
            using var provider = new RSACryptoServiceProvider();
            provider.FromXmlString(_publicKey);

            var array = key.Split(":::");
            var encryptedData = array[0];
            var encryptedKey = array[1];

            var encryptKey = Encoding.Default.GetString(provider.PublicDecryption(Convert.FromBase64String(encryptedKey)));
            var rawKey = EncryptService.Decrypt(encryptedData, encryptKey);

            return _deserializeActivationData(rawKey);
        }
        catch
        {
            return null;
        }
    }

    public static ActivationData Generate(string publisher, string target, bool asrAccess, DateTime? due = null, uint? calCount = null, string? editions = null, string? meta = null)
    {
        return new ActivationData
        {
            Version = 1,
            Publisher = publisher,
            Target = target,
            Editions = editions,
            AsrAccess = asrAccess,
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            DueDate = due,
            CalCount = calCount ?? 0,
            Meta = meta
        };
    }

    public static string GenerateKey(ActivationData activationData)
    {
        var rawKey = _serializeActivationData(activationData);
        var encryptKey = EncryptService.GenerateNewKey(12);

        using var provider = new RSACryptoServiceProvider();
        provider.FromXmlString(_privateKey);

        var encryptedKey = Convert.ToBase64String(provider.PrivareEncryption(Encoding.Default.GetBytes(encryptKey)));
        var encryptedData = EncryptService.Encrypt(rawKey, encryptKey);

        return $"{encryptedData}:::{encryptedKey}";
    }

    private static string _serializeActivationData(ActivationData activationData)
    {
        return JsonSerializer.Serialize(activationData);
    }

    private static ActivationData? _deserializeActivationData(string key)
    {
        return !string.IsNullOrWhiteSpace(key) ? JsonSerializer.Deserialize<ActivationData>(key) : null;
    }
}
