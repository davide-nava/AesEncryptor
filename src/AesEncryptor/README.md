# AesEncryptor

A high-performance, enterprise-grade cryptographic library for .NET providing authenticated encryption (**AES-256-GCM**), OWASP-compliant key derivation (**PBKDF2-HMAC-SHA256**), and strict memory hygiene.

---

## Package Overview

**AesEncryptor** simplifies robust data protection for modern .NET applications. Cryptographic operations often suffer from critical implementation pitfalls—such as insecure cipher modes (e.g., ECB/CBC without authentication), nonce reuse, static salts, insufficient key derivation iterations, and sensitive keys lingering in managed memory.

`AesEncryptor` addresses these challenges by delivering:
- **Security by Default**: Implements Authenticated Encryption with Associated Data (AEAD) using AES-256-GCM. Each encryption operation cryptographically enforces unique 96-bit nonces and 128-bit authentication tags to prevent replay and ciphertext tampering.
- **Robust Key Derivation**: Implements OWASP-recommended PBKDF2 key stretching (300,000 iterations, HMAC-SHA256, 16-byte cryptographically secure random salt) for password-based encryption.
- **Proactive Memory Hygiene**: Cryptographic keys derived in memory are explicitly wiped using `CryptographicOperations.ZeroMemory` in `finally` blocks, reducing exposure to heap inspections or memory dump attacks.
- **Developer-Friendly & High Performance**: Clean, static, and stateless API designed for high-concurrency microservices, web apps, and desktop software, with zero third-party runtime dependencies.

---

## Installation

Install the package via the .NET CLI:

```bash
dotnet add package AesEncryptor
```

Or via the NuGet Package Manager Console in Visual Studio:

```powershell
Install-Package AesEncryptor
```

---

## Quick Start & Code Example

`AesEncryptionService` provides thread-safe, static methods for both password-based and pre-shared key encryption.

### 1. Password-Based Encryption (Recommended for User Credentials & Secrets)

Generates a unique salt and nonce per operation, derives a 256-bit key using PBKDF2, and securely bundles the salt, nonce, tag, and ciphertext into a standard Base64 string.

```csharp
using AesEncryptor;

string secretData = "Sensitive customer payload or secret string";
string password = "YourStrongMasterPassword#2026!";

// Encrypt: returns a Base64-encoded string containing [Salt + Nonce + Tag + Ciphertext]
string cipherText = AesEncryptionService.EncryptByPassword(secretData, password);

// Decrypt: validates payload integrity, verifies password, and restores original plain text
string decryptedText = AesEncryptionService.DecryptByPassword(cipherText, password);

Console.WriteLine(decryptedText); // "Sensitive customer payload or secret string"
```

### 2. Pre-Shared Raw Key Encryption (Fast & URL-Safe)

Uses a pre-shared 32-byte (256-bit) raw key with AES-256-GCM. The output is encoded as URL-safe Base64 (`Base64Url`), making it ideal for query parameters, tokens, cookies, and web payloads.

```csharp
using System.Security.Cryptography;
using AesEncryptor;

// Generate or retrieve a 32-byte (256-bit) secret key
byte[] secretKey = RandomNumberGenerator.GetBytes(32);
string plainText = "order_id=45902&user_role=administrator";

// Encrypt: instantaneous (~0.01 ms), returns URL-safe Base64 string [Nonce + Tag + Ciphertext]
string urlSafeToken = AesEncryptionService.EncryptByBytes(plainText, secretKey);

// Decrypt: verifies authentication tag and decrypts token
string recoveredText = AesEncryptionService.DecryptByBytes(urlSafeToken, secretKey);

Console.WriteLine(recoveredText); // "order_id=45902&user_role=administrator"
```

---

## Key Features

- **Authenticated Encryption (AES-256-GCM)**: Guarantees both confidentiality and authenticity. Any payload tampering immediately throws a `CryptographicException`.
- **OWASP-Compliant Key Derivation**: Uses PBKDF2 with `HMAC-SHA256`, 300,000 iterations, and a cryptographically secure 16-byte salt per encryption call.
- **Zero-Allocation Memory Hygiene**: Derived keys are guaranteed to be purged from memory via `CryptographicOperations.ZeroMemory` upon completion.
- **URL-Safe Output**: Byte-key encryption produces RFC-compliant URL-safe Base64 strings without padding characters, ready for web queries and headers.
- **Stateless & Thread-Safe**: Static service methods (`AesEncryptionService`) designed for concurrent, multi-threaded scenarios.
- **Zero External Dependencies**: Built entirely on top of the native .NET Base Class Library (`System.Security.Cryptography` and `System.Buffers.Text`) with no third-party supply-chain dependencies.
- **Legacy CBC Compatibility**: Retains backwards-compatible methods (`EncryptByKey`, `DecryptByKey`) for existing AES-CBC integrations.

---

## Requirements / Target Frameworks

- **Target Framework**: `.NET 11.0` (`net11.0`)
- **Language Version**: C# 14 / Preview
- **Supported Platforms**: Cross-platform (Windows, Linux, macOS)
- **Dependencies**: None (0 external runtime dependencies)
