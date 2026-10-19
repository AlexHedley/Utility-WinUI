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

On Windows, install Visual Studio 2022 with the **.NET desktop development** workload and the Windows 10/11 SDK. Open `src/Utility.WinUI.csproj`, restore NuGet packages, and run the project. The project targets .NET 8 and uses the Windows App SDK 1.6.

## Tests

Run the cross-platform utility tests with:

```sh
dotnet test tests/Utility.Core.Tests/Utility.Core.Tests.csproj
```
