# AesEncryptor

[![Build & Test](https://img.shields.io/badge/build-passing-brightgreen.svg)](#verification-and-testing)
[![.NET 11](https://img.shields.io/badge/.NET-11.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Security: AES--256--GCM](https://img.shields.io/badge/Security-AES--256--GCM-red.svg)](#cryptographic-design)
[![Key Derivation: PBKDF2](https://img.shields.io/badge/KDF-PBKDF2--SHA256-orange.svg)](#cryptographic-design)

**AesEncryptor** is a high-performance, enterprise-grade cryptographic library and desktop application suite built with **.NET 11**. It provides state-of-the-art authenticated encryption (AES-256-GCM) with OWASP-recommended PBKDF2 key derivation, zero-allocation cryptographic memory handling, a modern **WPF Windows Desktop** client, and an Inno Setup installer.

---

## Key Features

- **Authenticated Encryption with Associated Data (AEAD)**: Implements AES-256-GCM for tamper-proof data encryption with built-in integrity and authenticity verification.
- **OWASP-Compliant Key Derivation**: Uses PBKDF2 with HMAC-SHA256, 16-byte random salts, and 300,000 iterations to protect password-derived encryption keys against brute-force and dictionary attacks.
- **Ultra-Fast URL-Safe Key-Based Encryption**: Instantaneous (~0.01 ms) AES-256-GCM encryption with pre-shared 32-byte keys formatted using URL-safe Base64 (`Base64Url`), ideal for tokens, query parameters, and session cookies.
- **Secure Memory Management**: Derived key buffers are wiped from memory immediately using `CryptographicOperations.ZeroMemory` upon operation completion.
- **Legacy AES Compatibility**: Retains CBC-mode key-and-IV methods (`EncryptByKey`, `DecryptByKey`) for backward compatibility with existing systems.
- **WPF Desktop Client**: Native Windows desktop application targeting modern Windows 10/11 environments with responsive layout and input validation.
- **Zero-Warning Code Quality**: Built under strict Roslyn, StyleCop, SonarAnalyzer, Roslynator, and Meziantou analyzer rules with `TreatWarningsAsErrors` enabled.

---

## Solution Architecture

The solution (`AesEncryptor.slnx`) is organized into clean, focused projects:

```text
AesEncryptor/
├── .github/workflows/          # CI/CD pipelines (SonarCloud, Qodana, NuGet publish, Release)
├── assets/                     # Icons, branding assets, and StyleCop configuration
├── installer/                  # Inno Setup Windows installer script (AesEncryptor.iss)
├── src/
│   ├── AesEncryptor/           # Core .NET 11 class library & NuGet package
│   │   └── AesEncryptionService.cs
│   └── AesEncryptor.Wpf/       # Windows Presentation Foundation (WPF) desktop application
│       ├── MainWindow.xaml     # Desktop GUI layout
│       └── MainWindow.xaml.cs  # Desktop GUI logic
└── tests/
    └── AesEncryptor.UnitTests/ # Unit tests (xUnit, Coverlet, Moq)
```

---

## Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download) (or preview runtime supporting .NET 11)
- **Supported Operating Systems**:
  - **Core Library**: Cross-platform (Windows, Linux, macOS).
  - **WPF Desktop Client & Installer**: Windows 10 (v10.0.19041.0+) or Windows 11.

---

## Installation & Usage

### 1. Password-Based Encryption (Recommended)

Derives a 256-bit AES key from a passphrase using PBKDF2 with HMAC-SHA256, 300,000 iterations, and a unique 16-byte cryptographically secure salt:

```csharp
using AesEncryptor;

string secretText = "Confidential business intelligence";
string password = "StrongUserPassword#2026";

// Encrypt: Generates combined payload (salt + nonce + tag + ciphertext) encoded in Base64
string cipherText = AesEncryptionService.EncryptByPassword(secretText, password);

// Decrypt: Authenticates payload and recovers original plain text
string decryptedText = AesEncryptionService.DecryptByPassword(cipherText, password);

Console.WriteLine(decryptedText); // "Confidential business intelligence"
```

### 2. Pre-Shared Raw Key Encryption (Fast & URL-Safe)

Uses a pre-shared 32-byte (256-bit) raw key with AES-256-GCM, returning a URL-safe Base64 encoded string:

```csharp
using System.Security.Cryptography;
using AesEncryptor;

// Generate or supply a 32-byte (256-bit) secret key
byte[] secretKey = RandomNumberGenerator.GetBytes(32);
string plainText = "user_id=12345&role=admin";

// Encrypt: Combines 12-byte nonce, 16-byte tag, and ciphertext with Base64Url encoding
string urlSafeToken = AesEncryptionService.EncryptByBytes(plainText, secretKey);

// Decrypt: Validates authenticity and decrypts payload
string recoveredText = AesEncryptionService.DecryptByBytes(urlSafeToken, secretKey);

Console.WriteLine(recoveredText); // "user_id=12345&role=admin"
```

### 3. Legacy AES-CBC Key/IV Encryption

For legacy interoperability requiring AES-CBC:

```csharp
using AesEncryptor;

string key = "12345678901234567890123456789012"; // 32-character key
string iv = "1234567890123456";                 // 16-character IV

string cipherText = AesEncryptionService.EncryptByKey("Legacy plain text", key);
string decryptedText = AesEncryptionService.DecryptByKey(cipherText, key, iv);
```

---

## Cryptographic Design & Specifications

| Parameter | Specification | Purpose |
| :--- | :--- | :--- |
| **Cipher Algorithm** | AES-256-GCM (Galois/Counter Mode) | Authenticated encryption preventing tampering |
| **Key Size** | 256 bits (32 bytes) | Maximum security strength for AES |
| **Nonce / IV** | 96 bits (12 bytes) | Cryptographically randomized per operation via `RandomNumberGenerator` |
| **Auth Tag** | 128 bits (16 bytes) | Authenticates ciphertext and nonce integrity |
| **KDF Algorithm** | PBKDF2 (HMAC-SHA256) | Password hashing and key stretching |
| **KDF Iterations** | 300,000 iterations | Complies with current OWASP recommendations |
| **Salt Size** | 128 bits (16 bytes) | Unique randomized salt per encryption |
| **Key Zeroing** | `CryptographicOperations.ZeroMemory` | Prevents key leakage in managed memory heap |

---

## Running the Application

### Running the WPF Windows Application

```bash
dotnet run --project src/AesEncryptor.Wpf/AesEncryptor.Wpf.csproj
```

### Packaging & Installer

To publish the self-contained single-file executable for Windows x64:

```bash
dotnet publish src/AesEncryptor.Wpf/AesEncryptor.Wpf.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

To compile the Windows installer with Inno Setup:

```bash
iscc installer/AesEncryptor.iss
```

---

## Verification and Testing

The test suite thoroughly verifies:
- Round-trip encryption and decryption for password-based and byte-based modes.
- Authenticity checks: verification failure and exceptions on tampered ciphertexts or truncated payloads.
- Randomness verification: ensuring identical plaintext encrypted multiple times yields distinct ciphertexts due to fresh salts/nonces.
- Edge cases: null or empty inputs, invalid key sizes, and incorrect passwords.

Run all tests from the repository root:

```bash
# Build with zero warnings
dotnet build AesEncryptor.slnx

# Execute unit tests
dotnet test AesEncryptor.slnx
```

---

## Code Quality & Static Analysis

This repository enforces stringent quality rules via `Directory.Build.props`:
- **TreatWarningsAsErrors**: All compiler and analyzer warnings are treated as build-breaking errors.
- **Roslyn Analyzers**: `Microsoft.CodeAnalysis.NetAnalyzers`, `SonarAnalyzer.CSharp`, `StyleCop.Analyzers`, `Roslynator`, `SecurityCodeScan`, `IDisposableAnalyzers`, `AsyncFixer`, and `Meziantou.Analyzer`.
- **CI Pipelines**: Continuous code scanning with **SonarCloud** and **JetBrains Qodana**.

---

## License

This project is licensed under the [MIT License](LICENSE) - see the `LICENSE` file for details.

**Author**: [Davide Nava](https://github.com/davide-nava)
