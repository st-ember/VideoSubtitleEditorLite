namespace SubtitleEditor.Infrastructure.Services;

public interface IEncryptService
{
    string Encrypt(string? rawText);
    string Encrypt(string? rawText, string key);
    string Decrypt(string? encryptedText);
    string Decrypt(string? encryptedText, string key);

    string GenerateNewKey(int length);
}
