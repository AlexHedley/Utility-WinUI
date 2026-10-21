# Utility-WinUI user and contributor guide

Utility is a small, local-first Windows desktop toolbox. Select a tool in the navigation pane, enter or paste text, choose an action, and read the result in the output field. Use **Clear** to reset the current input and output.

## Tools

### HTML encoder

- **Encode** converts characters to HTML entities using .NET HTML encoding.
- **Decode** converts HTML entities back to characters.

Use this for HTML text or fragments; it does not validate or sanitize an entire web page.

### URL encoder

- **Encode** percent-encodes a URL component (for example, a query parameter value), not a complete URL.
- **Decode** percent-decodes a URL component.

### HEX to RGB

Converts three- or six-digit hexadecimal colors, optionally prefixed with `#`, to `rgb(r, g, b)`. For example, `#ABC` becomes `rgb(170, 187, 204)`. Invalid values show a validation message.

### SQL IN builder

Enter values separated by commas or line breaks. The tool trims surrounding whitespace, removes one matching pair of single or double quote wrappers, escapes embedded apostrophes by doubling them, and returns an `IN (...)` clause.

```text
alpha
O'Reilly
```

Produces:

```sql
IN ('alpha', 'O''Reilly')
```

This is a convenience for preparing text, not a replacement for parameterized SQL when executing queries.

### GUID generator

Select **Generate GUID** to produce a new GUID string. The input field is not used.

### JSON formatter

- **Format** parses JSON and emits an indented representation.
- **Minify** parses JSON and emits compact JSON.

Both actions validate the input; malformed JSON produces a validation message.

### XML formatter

- **Format** parses XML and emits an indented representation.
- **Minify** removes insignificant indentation whitespace while retaining meaningful mixed-content whitespace.

Both actions validate the input; malformed XML produces a validation message.

### SQL formatter

Formats common SQL keywords and puts recognized clause keywords on separate lines. Quoted strings, quoted identifiers, comments, and complete identifier tokens are preserved. This is a lightweight formatter, not a dialect-aware SQL parser; inspect the output before using it.

## Privacy and data handling

All transformations run locally in the application. Utility does not send input to a web service or persist it. Avoid treating the tool as a secure vault: input and output are present in the app process and visible on screen until cleared or the app closes.

## Requirements

- Windows 10 version 1809 (build 17763) or newer.
- For development, Visual Studio 2026 (18.0 or newer) with the **.NET desktop development** and **Desktop development with C++** workloads, the .NET 10 SDK, and a Windows 10/11 SDK (10.0.19041.0 or newer).
- In Visual Studio Installer, include the MSVC C++ x64/x86 build tools. The WinUI XAML compiler invokes MSVC discovery during build, even though this app is written in C#.
- The app targets `net10.0-windows10.0.19041.0` and uses Windows App SDK 1.6. CI publishes a self-contained `win-x64` application.

If the build reports `MSB4018` in `GetLatestMSVCVersion` and says the `VC\Tools\MSVC` directory is missing, the C++ toolchain is not installed for that Visual Studio instance. Open Visual Studio Installer, select **Modify**, install **Desktop development with C++** and the MSVC x64/x86 build tools, then restart Visual Studio and rebuild. Do not create the missing directory manually.

## Build, run, and test

From the repository root:

```sh
dotnet restore src/Utility.WinUI.csproj
dotnet run --project src/Utility.WinUI.csproj
```

Run the platform-independent core tests:

```sh
dotnet test tests/Utility.Core.Tests/Utility.Core.Tests.csproj --configuration Release
```

The WinUI application requires Windows to run and build in its supported environment. `Utility.Core` and its xUnit tests target `net10.0`, so the tests can run cross-platform with the .NET 10 SDK.

## Project layout

- `src/Utility.WinUI.csproj`: WinUI 3 desktop executable and Windows App SDK reference.
- `src/MainWindow.*`: navigation shell and content frame.
- `src/UtilityPage.*`: reusable tool page, descriptions, and actions.
- `src/Utility.Core/`: cross-platform operation dispatch and transformations.
- `tests/Utility.Core.Tests/`: operation-level xUnit tests.
- `ARCHITECTURE.md`: component, data-flow, navigation, and delivery diagrams.

## CI and releases

The **Build and test** workflow runs for pull requests, pushes to `main` or `master`, and manual dispatch. On a Windows runner it tests the core, builds the app, publishes a self-contained `win-x64` directory, and uploads `Utility-WinUI-win-x64.zip` as an artifact.

To publish a release, push a version tag such as `v1.0.0`. The **Release** workflow can also be started manually by providing an existing tag. It checks out that tag, reruns the tests, publishes the self-contained app, and attaches `Utility-WinUI-win-x64.zip` to a GitHub Release. See `.github/workflows/build-test.yml` and `.github/workflows/release.yml` for the authoritative workflow configuration.

## Brand assets

Original project artwork is in [`assets/`](../assets/): an SVG logo, an SVG app mark, a Windows `.ico` application icon, and a 1280×640 PNG social preview. The social preview can be selected in the repository's GitHub social preview settings. The application icon is configured in the WinUI project.
