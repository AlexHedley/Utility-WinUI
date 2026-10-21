# Utility-WinUI

Utility is a Windows desktop collection of practical text, data, and formatting tools, built with WinUI 3. Work with input locally in a simple, keyboard-friendly workspace; the app does not send input to a service.

![Utility logo](assets/utility-logo.svg)

![Utility social preview](assets/social-preview.png)

## Features

- Encode and decode HTML entities and URL components
- Convert 3- or 6-digit HEX colors to RGB
- Build escaped SQL `IN` clauses from comma- or line-separated values
- Generate GUIDs
- Format or minify JSON and XML
- Format common SQL clauses

See the [user guide](docs/USER_GUIDE.md) for tool behavior and examples, and the [architecture guide](ARCHITECTURE.md) for the app structure and data flow.

## Build and run

For Visual Studio workloads, SDK requirements, and troubleshooting missing MSVC tools, see the [development requirements](docs/USER_GUIDE.md#requirements). Open `Utility-WinUI.sln`, restore NuGet packages, and run the app.

Run the cross-platform core tests from the repository root:

```sh
dotnet test tests/Utility.Core.Tests/Utility.Core.Tests.csproj --configuration Release
```

## CI and releases

The **Build and test** GitHub Actions workflow runs core tests, builds the Windows app, and uploads a self-contained `win-x64` ZIP artifact for pushes to `main` or `master`, pull requests, and manual dispatch. The **Release** workflow runs on `v*` tags or can be started manually with an existing tag; it attaches the self-contained ZIP to a GitHub Release. See [GitHub Actions](.github/workflows/) and [release instructions](docs/USER_GUIDE.md#ci-and-releases).

## Project documentation

- [User guide](docs/USER_GUIDE.md)
- [Architecture](ARCHITECTURE.md)
- [Changelog](CHANGELOG.md)
- [Brand assets](assets/)

## License

See [LICENSE](LICENSE).
