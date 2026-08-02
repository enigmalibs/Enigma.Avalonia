# Security Policy

Enigma.Avalonia.Desktop is a control library for Avalonia desktop applications, so its controls sit
directly on the path that user input takes into an application: text and byte values typed into the
editors, file and folder paths returned by the picker services, and the content an application
renders through the dialog, overlay and info-bar hosts. A defect that lets one of those return or
accept something the application did not expect becomes that application's defect. Vulnerability
reports are taken seriously and handled with priority.

## Supported versions

Security fixes are provided for the latest released version. Enigma.Avalonia.Desktop follows
[Semantic Versioning](https://semver.org/), and users are encouraged to stay current with the
newest release.

| Version | Supported          |
|---------|--------------------|
| 1.0.x   | :white_check_mark: |

## Reporting a vulnerability

**Please do not report security vulnerabilities through public GitHub issues, discussions, or pull
requests.** Public disclosure before a fix is available puts every user at risk.

Instead, use **GitHub's private vulnerability reporting**:

1. Go to the repository's **Security** tab.
2. Select **Report a vulnerability** to open a private advisory.
3. Include as much detail as you can — the affected version, the component involved, a description
   of the issue, and, where possible, a minimal reproduction and its impact.

This keeps the report private between you and the maintainers while it is triaged and fixed.

## What to expect

- Your report will be acknowledged and triaged as promptly as possible.
- The issue will be investigated and, once confirmed, a fix prepared and released.
- Coordinated disclosure is preferred: please allow a reasonable period for a fix to ship before
  any public discussion of the vulnerability.
- Your contribution will be credited in the resulting advisory unless you ask to remain anonymous.

## Scope

Reports concerning the controls, the services, the theme dictionary and the public API surface of
Enigma.Avalonia.Desktop are in scope.

Two boundaries are worth naming, because issues on the far side of them belong to another project's
process:

- **Cryptographic and encoding operations reached through `Base64Editor` and `HexadecimalEditor` are
  implemented by [Enigma.Core](https://github.com/enigmalibs/Enigma.Core)**, which in turn builds on
  BouncyCastle. This library only calls those services and renders the result; a defect in the
  encoding or decoding itself should be reported to Enigma.Core, and one rooted in BouncyCastle to
  the BouncyCastle project.
- **Rendering, input handling, and the storage provider behind the file and folder pickers are
  Avalonia's.** Issues that reproduce with plain Avalonia controls belong upstream to Avalonia.

If you are unsure which side of a boundary an issue falls on, report it here — triage is the
maintainers' job, not the reporter's.
