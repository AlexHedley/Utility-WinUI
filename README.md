# Utility-WinUI

A Windows desktop version of Utility, built with WinUI 3 and a WinUI Gallery-style navigation layout.

## Utilities

- HTML encode and decode
- URL encode and decode
- HEX to RGB
- SQL `IN` clause builder
- GUID generator
- JSON formatter and minifier
- XML formatter and minifier
- SQL formatter

All processing happens locally in the app.

## Build and run

On Windows, install Visual Studio 2026 (18.0 or newer) with the **.NET desktop development** workload, the .NET 10 SDK, and the Windows 10/11 SDK. Open `src/Utility.WinUI.csproj`, restore NuGet packages, and run the project. The project targets .NET 10 and uses the Windows App SDK 1.6.

## Tests

Run the cross-platform utility tests with:

```sh
dotnet test tests/Utility.Core.Tests/Utility.Core.Tests.csproj
```

## GitHub Actions

The **Build and test** workflow runs on pushes to `main` or `master`, pull requests, and manual dispatch. It runs the tests, builds and publishes a self-contained `win-x64` app, and uploads `Utility-WinUI-win-x64.zip` as a workflow artifact.

The **Release** workflow runs when a `v*` tag is pushed, or can be started manually with an existing version tag. It reruns the tests and attaches a self-contained `win-x64` ZIP to a GitHub Release. To publish a release, push a version tag such as `v1.0.0`.
