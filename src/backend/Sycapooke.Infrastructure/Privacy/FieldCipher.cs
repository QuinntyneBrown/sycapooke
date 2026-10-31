using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sycapooke.Infrastructure.Privacy;

/// <summary>
/// Field-level encryption of personal data at rest (L2-054). Values are AES-256-GCM sealed under the
/// current key and stored as <c>enc1:{keyId}:{base64}</c>. Older keys stay in the ring so rotated data
/// still reads. A stored value without the prefix is read as plain text, so rows written before the
/// key was set keep working and are sealed the next time they are saved. With no key configured the
/// cipher passes values through unchanged, which is only meant for development.
/// </summary>
public sealed class FieldCipher(IReadOnlyList<(string Id, byte[] Key)> keys)
{
    private const string Prefix = "enc1:";
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private static FieldCipher _current = new([]);

    /// <summary>The process-wide cipher the EF converters use; set once at startup.</summary>
    public static FieldCipher Current => _current;

    /// <summary>Installs the cipher; a host without keys never switches off one that has them, so hosts sharing a process cannot lose each other's data.</summary>
    public static void Use(FieldCipher cipher)
    {
        if (cipher.IsEnabled || !_current.IsEnabled)
        {
            _current = cipher;
        }
    }

    /// <summary>Reads <c>Privacy:FieldEncryptionKeys</c>: <c>id:base64key</c> pairs separated by commas, the first being current.</summary>
    public static FieldCipher FromConfiguration(string? setting)
    {
        var keys = new List<(string, byte[])>();
        foreach (var part in (setting ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var split = part.IndexOf(':', StringComparison.Ordinal);
            if (split <= 0 || split == part.Length - 1)
            {
                throw new InvalidOperationException("Privacy:FieldEncryptionKeys must be id:base64key pairs separated by commas");
            }

            byte[] key;
            try
            {
                key = Convert.FromBase64String(part[(split + 1)..]);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Privacy:FieldEncryptionKeys has a key that is not base64");
            }

            keys.Add((part[..split], key.Length == 32 ? key : throw new InvalidOperationException("Privacy:FieldEncryptionKeys keys must be 32 bytes")));
        }

        return new FieldCipher(keys);
    }

    public bool IsEnabled => keys.Count > 0;

    public string? Protect(string? plain)
    {
        if (plain is null || keys.Count == 0)
        {
            return plain;
        }

        var (id, key) = keys[0];
        var data = Encoding.UTF8.GetBytes(plain);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[data.Length];
        var tag = new byte[TagSize];
        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, data, cipher, tag, Encoding.UTF8.GetBytes(id));
        return $"{Prefix}{id}:{Convert.ToBase64String([.. nonce, .. tag, .. cipher])}";
    }

    public string? Unprotect(string? stored)
    {
        if (stored is null || !stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return stored;
        }

        var rest = stored[Prefix.Length..];
        var split = rest.IndexOf(':', StringComparison.Ordinal);
        var id = rest[..split];
        var key = keys.FirstOrDefault(k => k.Id == id).Key
            ?? throw new CryptographicException($"The field encryption key '{id}' is not in the key ring");
        var raw = Convert.FromBase64String(rest[(split + 1)..]);
        var nonce = raw[..NonceSize];
        var tag = raw[NonceSize..(NonceSize + TagSize)];
        var cipher = raw[(NonceSize + TagSize)..];
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain, Encoding.UTF8.GetBytes(id));
        return Encoding.UTF8.GetString(plain);
    }
}

/// <summary>Seals a string column through <see cref="FieldCipher.Current"/>.</summary>
internal sealed class EncryptedStringConverter() : ValueConverter<string, string>(
    plain => FieldCipher.Current.Protect(plain)!,
    stored => FieldCipher.Current.Unprotect(stored)!);
