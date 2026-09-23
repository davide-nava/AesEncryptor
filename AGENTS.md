# AGENTS.md - Operational Governance & Architecture Manifesto for Autonomous Agents

This document defines the strict operational boundaries, engineering standards, cryptographic invariants, and verification protocols governing any AI coding assistant or autonomous agent working across the **AesEncryptor** repository.

---

## 1. Solution Architecture & Project Anatomy

AesEncryptor is a multi-project .NET 11 solution providing cryptographic services and user-facing clients:

```text
AesEncryptor/
├── AesEncryptor.slnx                # Visual Studio XML Solution orchestrating all projects
├── Directory.Build.props           # Global MSBuild rules, Roslynator, StyleCop, SonarAnalyzer, zero-warning gate
├── .editorconfig                   # Centralized Roslyn and code styling conventions
├── .roslynatorconfig               # Roslynator analyzer configuration
├── .sonarcloud.properties          # SonarCloud scanner configuration
├── qodana.yaml                     # JetBrains Qodana code quality profile
├── LICENSE                         # MIT License
├── README.md                       # High-level overview, quickstart, and usage documentation
│
├── .github/
│   └── workflows/
│       ├── code_quality.yml        # Qodana inspection workflow
│       ├── publish-nuget.yml       # NuGet package publishing workflow
│       ├── release.yml             # Windows x64 release & Inno Setup installer packaging
│       └── sonarcloud.yml          # SonarCloud static analysis workflow
│
├── assets/
│   ├── stylecop.json               # StyleCop analyzer parameters and company headers
│   └── img/                        # Application icons and branding assets
│
├── installer/
│   └── AesEncryptor.iss            # Inno Setup Windows installer configuration
│
├── src/
│   ├── AesEncryptor/               # Core .NET 11 cryptographic class library & NuGet package
│   │   ├── AesEncryptor.csproj
│   │   └── AesEncryptionService.cs # Stateless, thread-safe AES-GCM and PBKDF2 operations
│   │
│   └── AesEncryptor.Wpf/           # Native Windows 10/11 desktop application
│       ├── AesEncryptor.Wpf.csproj
│       ├── App.xaml / App.xaml.cs
│       └── MainWindow.xaml / cs    # Desktop encryption UI and interaction logic
│
└── tests/
    └── AesEncryptor.UnitTests/     # Comprehensive unit tests (xUnit, Coverlet, Moq)
        ├── AesEncryptor.UnitTests.csproj
        └── AesEncryptionServiceTests.cs
```

---

## 2. Fundamental Architectural & Cryptographic Invariants

### 2.1 Cryptographic Standards & Invariants

1. **Authenticated Encryption (AEAD)**:
   - Prefer **AES-256-GCM** for all primary encryption operations (`EncryptByBytes`, `EncryptByPassword`).
   - Every GCM operation MUST use a unique, randomly generated 96-bit (12-byte) nonce via `RandomNumberGenerator.GetBytes(12)`. Nonce reuse is strictly forbidden as it destroys GCM security.
   - Authentication tags must be 128 bits (16 bytes). Integrity failure during decryption MUST throw a `CryptographicException` and immediately fail closed without revealing sensitive data.

2. **Key Derivation (PBKDF2)**:
   - Password-derived encryption MUST use PBKDF2 with HMAC-SHA256 (`HashAlgorithmName.SHA256`).
   - The iteration count MUST follow current OWASP recommendations: minimum **300,000 iterations**.
   - Every password encryption operation MUST generate a unique 128-bit (16-byte) cryptographic salt using `RandomNumberGenerator.GetBytes(16)`. Static or hardcoded salts are forbidden.

3. **Memory Hygiene & Ephemeral Key Handling**:
   - Cryptographic keys derived from passwords must be explicitly wiped from memory as soon as the cipher operation completes.
   - Always wrap cryptographic key lifecycles in `try / finally` blocks and call `CryptographicOperations.ZeroMemory(key)` in the `finally` block.

4. **Payload Formats**:
   - **Byte-key GCM (`EncryptByBytes`)**: Payload consists of `[12-byte Nonce][16-byte Tag][Ciphertext]` encoded using URL-safe Base64 (`Base64Url.EncodeToString`).
   - **Password GCM (`EncryptByPassword`)**: Payload consists of `[16-byte Salt][12-byte Nonce][16-byte Tag][Ciphertext]` encoded using standard Base64 (`Convert.ToBase64String`).
   - Any payload smaller than the header size must be rejected immediately with a `CryptographicException`.

5. **Legacy CBC Compatibility**:
   - Legacy methods (`EncryptByKey`, `DecryptByKey`) using AES-CBC are retained strictly for backward compatibility. Do not deprecate or break them unless explicitly directed.

---

### 2.2 Engineering & Code Quality Standards

1. **Zero-Warning Policy (`TreatWarningsAsErrors`)**:
   - The repository configures `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>`.
   - The build enforces analyzers for `CA*`, `IDE*`, `SA*`, `S*`, `RCS*`, and `CS*`.
   - Under no circumstances may code be committed that introduces compiler warnings or analyzer violations.

2. **Language & Documentation Mandate**:
   - **English Only**: All identifiers, types, methods, comments, XML docs, exceptions, and documentation must be in English.
   - **Comprehensive XML Documentation**: `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is globally enforced. Every public and internal type, method, parameter, return value, and exception MUST contain clear, well-formed XML documentation comments.
   - Maintain the standard file header:
     ```csharp
     // <copyright file="<FileName>.cs" company="Davide Nava">
     // Copyright (c) Davide Nava. All rights reserved.
     // </copyright>
     ```

3. **Modern C# 14 / .NET 11 Idioms**:
   - Language version is set to `preview` (`<LangVersion>preview</LangVersion>`).
   - Use file-scoped namespaces (`namespace AesEncryptor;`).
   - Use collection expressions (`[..]`), UTF-8 string literals (`"..."u8`), pattern matching, and target-typed `new()`.
   - Nullable reference types are strictly enabled (`<Nullable>enable</Nullable>`). Null-suppression operators (`!`) must be avoided except in proven test contexts.

---

## 3. Project-Specific Stack Guidelines

### 3.1 Core Library (`src/AesEncryptor`)

- **Dependencies**: Keep zero third-party runtime dependencies. Rely entirely on the .NET Base Class Library (`System.Security.Cryptography`, `System.Buffers.Text`).
- **Design**: The `AesEncryptionService` is a static, thread-safe, and stateless utility.
- **Performance**: Minimize allocations; use stack allocation, spans, and array pooling when dealing with cryptographic buffers where appropriate.

### 3.2 WPF Desktop Client (`src/AesEncryptor.Wpf`)

- **Target Framework**: Windows 10/11 (`net11.0-windows10.0.19041.0`).
- **UI & Interaction**: Maintain clean XAML design, responsive layout, proper button enablement states based on input presence, and informative error dialogs.

### 3.3 Packaging & Installer (`installer/`)

- **Inno Setup Script**: `installer/AesEncryptor.iss` packages the self-contained single-file publish output into a Windows setup executable (`AesEncryptor-Setup.exe`).
- **Publish Profile**: Single-file, self-contained Windows x64 executable (`win-x64`).

### 3.4 Unit Tests (`tests/AesEncryptor.UnitTests`)

- **Test Framework**: xUnit with Moq and Coverlet.
- **Naming Convention**: Test methods must follow `MethodName_Condition_ExpectedBehavior`.
- **Mandatory Test Cases**:
  - Valid round-trip encryption and decryption.
  - Edge cases: null, empty, and whitespace input handling.
  - Boundary validations: invalid key lengths (e.g., non-32-byte keys for AES-256).
  - Security verifications: tampered payloads and wrong passwords must throw `CryptographicException`.
  - Nonce & Salt randomness: identical plaintexts encrypted multiple times must produce distinct ciphertexts.

---

## 4. Verification & Testing Protocols

Prior to completing any task or proposing changes, autonomous agents must execute the following verification steps:

```bash
# 1. Solution build with zero errors and zero warnings
dotnet build AesEncryptor.slnx

# 2. Run all unit test suites
dotnet test AesEncryptor.slnx
```

### Verification Checklist:
- [ ] Build completes with **0 Error(s)** and **0 Warning(s)**.
- [ ] All unit tests pass with 100% success rate.
- [ ] Any new or modified public/internal member includes full XML documentation.
- [ ] No hardcoded secrets, fixed nonces, or static salts have been introduced.
- [ ] Derived keys are zeroed with `CryptographicOperations.ZeroMemory`.
