// <copyright file="AesEncryptionServiceTests.cs" company="Davide Nava">
// Copyright (c) Davide Nava. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace AesEncryptor.UnitTests;

/// <summary>
/// Contains unit tests for the <see cref="AesEncryptionService"/> class.
/// </summary>
public class AesEncryptionServiceTests
{
    private const string PASSWORD = "MyStrongPassword123!";

    private const string PLAINTEXT = "Hello World";

#pragma warning disable IDE1006
    private static readonly byte[] ValidKey = "12345678901234567890123456789012"u8.ToArray();
#pragma warning restore IDE1006

    /// <summary>
    /// Verifies that text encrypted with a valid AES-256 key can be successfully decrypted using the same key.
    /// </summary>
    [Fact]
    public void EncryptByBytes_ThenDecryptByBytes_ReturnsOriginalText()
    {
        // Arrange

        // Act
        var encrypted = AesEncryptionService.EncryptByBytes(PLAINTEXT, ValidKey);
        var decrypted = AesEncryptionService.DecryptByBytes(encrypted, ValidKey);

        // Assert
        Assert.Equal(PLAINTEXT, decrypted);
    }

    /// <summary>
    /// Verifies that an empty or null plaintext returns an empty string without performing encryption.
    /// </summary>
    [Fact]
    public void EncryptByBytes_WithNullOrEmptyText_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, AesEncryptionService.EncryptByBytes(string.Empty, ValidKey));
        Assert.Equal(string.Empty, AesEncryptionService.EncryptByBytes(null!, ValidKey));
    }

    /// <summary>
    /// Verifies that encryption throws an <see cref="ArgumentException"/> when the provided key length is different from 32 bytes.
    /// </summary>
    [Fact]
    public void EncryptByBytes_WithInvalidKey_ThrowsArgumentException()
    {
        // Arrange
        var invalidKey = new byte[16];

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => AesEncryptionService.EncryptByBytes("test", invalidKey));
    }

    /// <summary>
    /// Verifies that encryption throws an <see cref="ArgumentException"/> when the provided key is null.
    /// </summary>
    [Fact]
    public void EncryptByBytes_WithNullKey_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => AesEncryptionService.EncryptByBytes("test", null!));

    /// <summary>
    /// Verifies that decryption throws an <see cref="ArgumentException"/> when the provided key length is different from 32 bytes.
    /// </summary>
    [Fact]
    public void DecryptByBytes_WithInvalidKey_ThrowsArgumentException()
    {
        // Arrange
        var invalidKey = new byte[16];

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => AesEncryptionService.DecryptByBytes("abc", invalidKey));
    }

    /// <summary>
    /// Verifies that decryption throws an <see cref="ArgumentException"/> when the provided key is null.
    /// </summary>
    [Fact]
    public void DecryptByBytes_WithNullKey_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => AesEncryptionService.DecryptByBytes("abc", null!));

    /// <summary>
    /// Verifies that an empty or whitespace ciphertext returns an empty string.
    /// </summary>
    [Fact]
    public void DecryptByBytes_WithEmptyCipher_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByBytes(string.Empty, ValidKey));
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByBytes(" ", ValidKey));
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByBytes(null!, ValidKey));
    }

    /// <summary>
    /// Verifies that decryption fails when the encrypted payload has been modified or tampered with.
    /// </summary>
    [Fact]
    public void DecryptByBytes_WithTamperedCipher_ThrowsCryptographicException()
    {
        // Arrange
        var encrypted = AesEncryptionService.EncryptByBytes(PLAINTEXT, ValidKey);

        var chars = encrypted.ToCharArray();
        chars[^1] = chars[^1] == 'A' ? 'B' : 'A';

        var tampered = new string(chars);

        // Act & Assert
        _ = Assert.ThrowsAny<CryptographicException>(() => AesEncryptionService.DecryptByBytes(tampered, ValidKey));
    }

    /// <summary>
    /// Verifies that text encrypted with a password can be decrypted using the same password.
    /// </summary>
    [Fact]
    public void EncryptByPassword_ThenDecryptByPassword_ReturnsOriginalText()
    {
        // Arrange

        // Act
        var encrypted = AesEncryptionService.EncryptByPassword(PLAINTEXT, PASSWORD);
        var decrypted = AesEncryptionService.DecryptByPassword(encrypted, PASSWORD);

        // Assert
        Assert.Equal(PLAINTEXT, decrypted);
    }

    /// <summary>
    /// Verifies that an empty or null plaintext returns an empty string without performing encryption.
    /// </summary>
    [Fact]
    public void EncryptByPassword_WithNullOrEmptyText_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, AesEncryptionService.EncryptByPassword(string.Empty, PASSWORD));
        Assert.Equal(string.Empty, AesEncryptionService.EncryptByPassword(null!, PASSWORD));
    }

    /// <summary>
    /// Verifies that encryption throws an <see cref="ArgumentException"/> when the password is empty.
    /// </summary>
    [Fact]
    public void EncryptByPassword_WithEmptyPassword_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => AesEncryptionService.EncryptByPassword("text", string.Empty));

    /// <summary>
    /// Verifies that encryption throws an <see cref="ArgumentException"/> when the password is null.
    /// </summary>
    [Fact]
    public void EncryptByPassword_WithNullPassword_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => AesEncryptionService.EncryptByPassword("text", null!));

    /// <summary>
    /// Verifies that decryption throws an <see cref="ArgumentException"/> when the password is empty.
    /// </summary>
    [Fact]
    public void DecryptByPassword_WithEmptyPassword_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => AesEncryptionService.DecryptByPassword("cipher", string.Empty));

    /// <summary>
    /// Verifies that decryption throws an <see cref="ArgumentException"/> when the password is null.
    /// </summary>
    [Fact]
    public void DecryptByPassword_WithNullPassword_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() => AesEncryptionService.DecryptByPassword("cipher", null!));

    /// <summary>
    /// Verifies that an empty, null, or whitespace ciphertext returns an empty string without attempting decryption.
    /// </summary>
    [Fact]
    public void DecryptByPassword_WithEmptyCipher_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByPassword(string.Empty, PASSWORD));
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByPassword(" ", PASSWORD));
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByPassword(null!, PASSWORD));
    }

    /// <summary>
    /// Verifies that decryption fails when the encrypted payload has been tampered with.
    /// </summary>
    [Fact]
    public void DecryptByPassword_WithTamperedCipher_ThrowsCryptographicException()
    {
        // Arrange
        var encrypted = AesEncryptionService.EncryptByPassword(PLAINTEXT, PASSWORD);
        var bytes = Convert.FromBase64String(encrypted);
        bytes[^1] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        // Act & Assert
        _ = Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionService.DecryptByPassword(tampered, PASSWORD));
    }

    /// <summary>
    /// Verifies that decryption fails when an incorrect password is provided.
    /// </summary>
    [Fact]
    public void DecryptByPassword_WithWrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        var encrypted = AesEncryptionService.EncryptByPassword(PLAINTEXT, PASSWORD);

        // Act & Assert
        _ = Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionService.DecryptByPassword(encrypted, "WrongPassword"));
    }

    /// <summary>
    /// Verifies that decryption fails when the encrypted payload is shorter than the minimum required size.
    /// </summary>
    [Fact]
    public void DecryptByPassword_WithTruncatedPayload_ThrowsCryptographicException()
    {
        // Arrange
        var invalidPayload = Convert.ToBase64String(new byte[10]);

        // Act & Assert
        _ = Assert.Throws<CryptographicException>(() =>
            AesEncryptionService.DecryptByPassword(invalidPayload, PASSWORD));
    }

    /// <summary>
    /// Verifies that an empty, null, or whitespace ciphertext returns an empty string.
    /// </summary>
    [Fact]
    public void DecryptByKey_WithEmptyCipher_ReturnsEmptyString()
    {
        // Arrange
        const string key = "12345678901234567890123456789012";
        const string iv = "1234567890123456";

        // Act & Assert
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByKey(string.Empty, key, iv));
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByKey(" ", key, iv));
        Assert.Equal(string.Empty, AesEncryptionService.DecryptByKey(null!, key, iv));
    }

    /// <summary>
    /// Verifies that encrypting the same plaintext multiple times with the same password produces different ciphertext values due to the use of random salt and nonce values.
    /// </summary>
    [Fact]
    public void EncryptByPassword_SameInput_ProducesDifferentCipherText()
    {
        // Act
        var encrypted1 = AesEncryptionService.EncryptByPassword(PLAINTEXT, PASSWORD);
        var encrypted2 = AesEncryptionService.EncryptByPassword(PLAINTEXT, PASSWORD);

        // Assert
        Assert.NotEqual(encrypted1, encrypted2, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that encrypting the same plaintext multiple times with the same key produces different ciphertext values due to the use of random nonce values.
    /// </summary>
    [Fact]
    public void EncryptByBytes_SameInput_ProducesDifferentCipherText()
    {
        // Act
        var encrypted1 = AesEncryptionService.EncryptByBytes(PLAINTEXT, ValidKey);
        var encrypted2 = AesEncryptionService.EncryptByBytes(PLAINTEXT, ValidKey);

        // Assert
        Assert.NotEqual(encrypted1, encrypted2, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that EncryptByKey returns a valid Base64-encoded string.
    /// </summary>
    [Fact]
    public void EncryptByKey_ReturnsBase64String()
    {
        // Arrange
        const string key = "12345678901234567890123456789012";

        // Act
        var cipher = AesEncryptionService.EncryptByKey(PLAINTEXT, key);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(cipher));
        _ = Convert.FromBase64String(cipher); // will throw if not valid Base64
    }

    /// <summary>
    /// Verifies that EncryptByKey returns a valid Base64-encoded string when given an empty string.
    /// </summary>
    [Fact]
    public void EncryptByKey_WithEmptyText_ReturnsBase64String()
    {
        // Arrange
        const string key = "12345678901234567890123456789012";

        // Act
        var cipher = AesEncryptionService.EncryptByKey(string.Empty, key);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(cipher));
        _ = Convert.FromBase64String(cipher);
    }

    /// <summary>
    /// Verifies that data encrypted with Aes using a specific IV can be decrypted with DecryptByKey using the same key and IV.
    /// </summary>
    [Fact]
#pragma warning disable CA5390, CA5401
    public void EncryptByKey_ThenDecryptByKey_ReturnsOriginalText()
    {
        // Arrange
        const string key = "12345678901234567890123456789012";
        const string iv = "1234567890123456";

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(PLAINTEXT);
        }

        var cipherBase64 = Convert.ToBase64String(ms.ToArray());

        // Act
        var decrypted = AesEncryptionService.DecryptByKey(cipherBase64, key, iv);

        // Assert
        Assert.Equal(PLAINTEXT, decrypted);
    }

    /// <summary>
    /// Verifies that DecryptByBytes throws when the decoded payload is shorter than the expected header.
    /// </summary>
    [Fact]
    public void DecryptByBytes_WithTruncatedPayload_ThrowsCryptographicException()
    {
        // Arrange
        var invalidPayload = Convert.ToBase64String(new byte[5]).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Act & Assert
        _ = Assert.ThrowsAny<CryptographicException>(() => AesEncryptionService.DecryptByBytes(invalidPayload, ValidKey));
    }
}
