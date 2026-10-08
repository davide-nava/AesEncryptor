// <copyright file="AesEncryptionService.cs" company="Davide Nava">
// Copyright (c) Davide Nava. All rights reserved.
// </copyright>

using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace AesEncryptor;

/// <summary>
/// Provides methods for encrypting and decrypting data using AES-GCM and PBKDF2 key derivation.
/// </summary>
public static class AesEncryptionService
{
    /// <summary>
    /// Gets the size of the encryption key in bytes (256 bits maximum key size for AES).
    /// </summary>
    private const int KEYSIZEBYTES = 32;

    /// <summary>
    /// Gets the size of the salt in bytes (128 bits).
    /// </summary>
    private const int SALTSIZEBYTES = 16;

    /// <summary>
    /// Gets the size of the nonce in bytes (96 bits standard recommended size for GCM).
    /// </summary>
    private const int NONCESIZEBYTES = 12;

    /// <summary>
    /// Gets the size of the authentication tag in bytes (128 bits authentication tag size).
    /// </summary>
    private const int TAGSIZEBYTES = 16;

    /// <summary>
    /// Gets the number of iterations for the PBKDF2 key derivation function (OWASP recommendation for PBKDF2-HMAC-SHA256).
    /// </summary>
    private const int KDFITERATIONS = 300_000;

    /// <summary>
    /// Encrypts the specified text using the provided key and initialization vector.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>The encrypted text.</returns>
    public static string EncryptByKey(string plainText, string key)
    {
        byte[] res;

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            using var encryptor = aes.CreateEncryptor();

            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            res = msEncrypt.ToArray();
        }

        return Convert.ToBase64String(res);
    }

    /// <summary>
    /// Decrypts the specified Base64-encoded cipher text using the provided key and initialization vector.
    /// </summary>
    /// <param name="cipherTextBase64">The Base64-encoded cipher text to decrypt.</param>
    /// <param name="key">The decryption key.</param>
    /// <param name="iv">The initialization vector.</param>
    /// <returns>The decrypted text.</returns>
    public static string DecryptByKey(string cipherTextBase64, string key, string iv)
    {
        if (string.IsNullOrWhiteSpace(cipherTextBase64))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream msDecrypt = new(Convert.FromBase64String(cipherTextBase64));
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    /// <summary>
    /// Encrypts sensitive web parameters or settings using AES-256-GCM.
    /// Execution time: ~0.01 ms (Instantaneous).
    /// Output: URL-safe Base64 string.
    /// </summary>
    /// <param name="plainText">The value to encrypt.</param>
    /// <param name="raw256BitKey">Pre-shared 32-byte (256-bit) secret key.</param>
    /// <returns>The URL-safe Base64-encoded encrypted text.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is invalid.</exception>
    public static string EncryptByBytes(string plainText, byte[] raw256BitKey)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        if (raw256BitKey is not { Length: KEYSIZEBYTES })
        {
            throw new ArgumentException("Key must be exactly 32 bytes (256 bits).", nameof(raw256BitKey));
        }

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NONCESIZEBYTES);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TAGSIZEBYTES];

        using (var aesGcm = new AesGcm(raw256BitKey, TAGSIZEBYTES))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        var payload = new byte[NONCESIZEBYTES + TAGSIZEBYTES + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NONCESIZEBYTES);
        Buffer.BlockCopy(tag, 0, payload, NONCESIZEBYTES, TAGSIZEBYTES);
        Buffer.BlockCopy(cipherBytes, 0, payload, NONCESIZEBYTES + TAGSIZEBYTES, cipherBytes.Length);

        return Base64Url.EncodeToString(payload);
    }

    /// <summary>
    /// Decrypts sensitive web parameters or settings using AES-256-GCM.
    /// </summary>
    /// <param name="urlSafeCipherText">The URL-safe Base64-encoded ciphertext to decrypt.</param>
    /// <param name="raw256BitKey">The 32-byte (256-bit) secret key.</param>
    /// <returns>The decrypted plain text.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is invalid.</exception>
    /// <exception cref="CryptographicException">Thrown when the ciphertext is invalid or has been tampered with.</exception>
    public static string DecryptByBytes(string urlSafeCipherText, byte[] raw256BitKey)
    {
        if (string.IsNullOrWhiteSpace(urlSafeCipherText))
        {
            return string.Empty;
        }

        if (raw256BitKey is not { Length: KEYSIZEBYTES })
        {
            throw new ArgumentException("Key must be exactly 32 bytes (256 bits).", nameof(raw256BitKey));
        }

        var payload = Base64Url.DecodeFromChars(urlSafeCipherText);
        const int headerSize = NONCESIZEBYTES + TAGSIZEBYTES;

        if (payload.Length < headerSize)
        {
            throw new CryptographicException("Ciphertext payload is invalid or truncated.");
        }

        var nonce = new byte[NONCESIZEBYTES];
        var tag = new byte[TAGSIZEBYTES];

        Buffer.BlockCopy(payload, 0, nonce, 0, NONCESIZEBYTES);
        Buffer.BlockCopy(payload, NONCESIZEBYTES, tag, 0, TAGSIZEBYTES);

        var cipherSize = payload.Length - headerSize;
        var cipherBytes = new byte[cipherSize];
        var plainBytes = new byte[cipherSize];
        Buffer.BlockCopy(payload, headerSize, cipherBytes, 0, cipherSize);

        using (var aesGcm = new AesGcm(raw256BitKey, TAGSIZEBYTES))
        {
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Encrypts the specified plain text using AES-256-GCM with a key derived from the user password.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="userPassword">The user password used to derive the encryption key.</param>
    /// <returns>The combined salt, nonce, tag, and ciphertext as a Base64-encoded string.</returns>
    /// <exception cref="CryptographicException">Thrown when the ciphertext is invalid or the password is incorrect.</exception>
    /// <exception cref="AuthenticationTagMismatchException">Thrown when the authentication tag is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when the cipher text or password is null or empty.</exception>
    public static string EncryptByPassword(string plainText, string userPassword)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(userPassword))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(userPassword));
        }

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var salt = RandomNumberGenerator.GetBytes(SALTSIZEBYTES);
        var nonce = RandomNumberGenerator.GetBytes(NONCESIZEBYTES);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TAGSIZEBYTES];

        var key = Rfc2898DeriveBytes.Pbkdf2(userPassword, salt, KDFITERATIONS, HashAlgorithmName.SHA256, KEYSIZEBYTES);

        using (var aesGcm = new AesGcm(key, TAGSIZEBYTES))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        var result = new byte[SALTSIZEBYTES + NONCESIZEBYTES + TAGSIZEBYTES + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, result, 0, SALTSIZEBYTES);
        Buffer.BlockCopy(nonce, 0, result, SALTSIZEBYTES, NONCESIZEBYTES);
        Buffer.BlockCopy(tag, 0, result, SALTSIZEBYTES + NONCESIZEBYTES, TAGSIZEBYTES);
        Buffer.BlockCopy(cipherBytes, 0, result, SALTSIZEBYTES + NONCESIZEBYTES + TAGSIZEBYTES, cipherBytes.Length);

        CryptographicOperations.ZeroMemory(key);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts the specified Base64-encoded cipher text using AES-256-GCM and the user password.
    /// </summary>
    /// <param name="cipherTextBase64">The Base64-encoded payload containing salt, nonce, tag, and ciphertext.</param>
    /// <param name="userPassword">The user password used to derive the decryption key.</param>
    /// <returns>The decrypted plain text.</returns>
    /// <exception cref="CryptographicException">Thrown when the ciphertext is invalid or the password is incorrect.</exception>
    /// <exception cref="AuthenticationTagMismatchException">Thrown when the authentication tag is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when the cipher text or password is null or empty.</exception>
    public static string DecryptByPassword(string cipherTextBase64, string userPassword)
    {
        if (string.IsNullOrWhiteSpace(cipherTextBase64))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(userPassword))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(userPassword));
        }

        var rawPayload = Convert.FromBase64String(cipherTextBase64);
        const int headerSize = SALTSIZEBYTES + NONCESIZEBYTES + TAGSIZEBYTES;

        if (rawPayload.Length < headerSize)
        {
            throw new CryptographicException("Ciphertext payload is invalid or truncated.");
        }

        var salt = new byte[SALTSIZEBYTES];
        var nonce = new byte[NONCESIZEBYTES];
        var tag = new byte[TAGSIZEBYTES];

        Buffer.BlockCopy(rawPayload, 0, salt, 0, SALTSIZEBYTES);
        Buffer.BlockCopy(rawPayload, SALTSIZEBYTES, nonce, 0, NONCESIZEBYTES);
        Buffer.BlockCopy(rawPayload, SALTSIZEBYTES + NONCESIZEBYTES, tag, 0, TAGSIZEBYTES);

        var cipherSize = rawPayload.Length - headerSize;
        var cipherBytes = new byte[cipherSize];
        var plainBytes = new byte[cipherSize];
        Buffer.BlockCopy(rawPayload, headerSize, cipherBytes, 0, cipherSize);

        var key = Rfc2898DeriveBytes.Pbkdf2(userPassword, salt, KDFITERATIONS, HashAlgorithmName.SHA256, KEYSIZEBYTES);

        try
        {
            using var aesGcm = new AesGcm(key, TAGSIZEBYTES);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }
}
