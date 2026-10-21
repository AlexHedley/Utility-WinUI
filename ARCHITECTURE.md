# Architecture

Utility-WinUI is a Windows desktop application. Its UI is a thin WinUI 3 shell over a cross-platform .NET core library. Utility operations are synchronous, local transformations; there is no network or persistence layer.

## Components

```mermaid
flowchart LR
    User([User])
    subgraph Desktop["Utility.WinUI · WinUI 3"]
        App["App<br/>application resources and launch"]
        Window["MainWindow<br/>NavigationView and Frame"]
        Page["UtilityPage<br/>welcome hero and tool input/actions/output"]
    end
    subgraph Core["Utility.Core · .NET 10"]
        Operations["UtilityOperations.Run"]
        Html["HTML / URL"]
        Color["HEX to RGB"]
        Sql["SQL helpers"]
        JsonXml["JSON / XML"]
        Guid["GUID"]
    end
    User --> Window
    App --> Window
    Window --> Page
    Page -->|"select a Welcome tool"| Page
    Page -->|"tool, action, input"| Operations
    Operations --> Html
    Operations --> Color
    Operations --> Sql
    Operations --> JsonXml
    Operations --> Guid
    Operations -->|"result or exception"| Page
    Page --> User
```

### WinUI application (`src/`)

- `App.xaml` merges `XamlControlsResources`; `App.xaml.cs` creates and activates the main window on launch.
- `MainWindow.xaml` defines the navigation menu and content frame. `MainWindow.xaml.cs` routes the selected tool to the utility page.
- `UtilityPage.xaml` provides the interactive Welcome hero and reusable input/action/output layout. `WelcomeHero` animates the available tools with Windows Composition, supports direct tool navigation and replay, and honors the Windows animation-effects setting. The page code-behind chooses tool descriptions and actions, calls the core library, and displays expected validation errors.
- `Utility.WinUI.csproj` targets `net10.0-windows10.0.19041.0`, uses Windows App SDK 1.6, and references the core project.

### Core library (`src/Utility.Core/`)

`UtilityOperations.Run(tool, action, input)` is the dispatch boundary. It delegates to focused transformations implemented in `UtilityOperations.cs` and returns a string result. Invalid input can raise format/parsing exceptions; unavailable tool/action pairs raise `InvalidOperationException`. The core project targets `net10.0` and has no additional package references.

### Tests (`tests/`)

`Utility.Core.Tests` is an xUnit test project targeting `net10.0`. It references the core project and covers the individual operations, including malformed inputs and preservation of user data in SQL/XML formatting.

## Navigation and operation flow

```mermaid
sequenceDiagram
    actor User
    participant Nav as MainWindow / NavigationView
    participant Page as UtilityPage
    participant Core as UtilityOperations

    User->>Nav: Select a tool
    Nav->>Page: Navigate(tool)
    Page->>Page: Show description and available actions
    User->>Page: Enter input and invoke action
    Page->>Core: Run(tool, action, input)
    alt Valid input
        Core-->>Page: Result string
        Page-->>User: Display output
    else Invalid input
        Core-->>Page: Expected exception
        Page-->>User: Display validation message
    end
```

## Data and trust boundaries

```mermaid
flowchart TB
    Input["User-entered text"]
    UI["WinUI input control"]
    Dispatch["UtilityOperations.Run"]
    Transform["In-process transformation"]
    Output["WinUI output control"]
    Error["Validation message"]
    Input --> UI --> Dispatch --> Transform
    Transform --> Output
    Transform -. "format / parse failure" .-> Error
    Output --> User([User])
    Error --> User
```

Input remains in process memory for the duration of the operation and is not uploaded or stored by the app. The SQL `IN` builder escapes apostrophes in values, but its output is a text convenience and is not a substitute for parameterized SQL in application code. The SQL formatter handles common tokens and clauses; it is not a full SQL parser.

## Build and delivery

```mermaid
flowchart LR
    Change["Push / pull request"] --> Tests[".NET 10 core tests"]
    Tests --> Build["Windows app build"]
    Build --> Publish["Self-contained win-x64 publish"]
    Publish --> Artifact["Workflow ZIP artifact"]
    Tag["v* tag / manual release"] --> ReleaseTests["Core tests"]
    ReleaseTests --> ReleasePublish["Self-contained win-x64 publish"]
    ReleasePublish --> GitHubRelease["GitHub Release ZIP"]
```

The build/test workflow runs on Windows and publishes a downloadable workflow artifact. The release workflow reruns the tests and attaches the ZIP to a GitHub Release. Workflow definitions are in `.github/workflows/`.

## Repository map

| Path | Responsibility |
| --- | --- |
| `src/` | WinUI application and shared core project |
| `src/WelcomeHero.*` | Composition-based Welcome scene and tool shortcuts |
| `tests/Utility.Core.Tests/` | Cross-platform tests for core operations |
| `docs/USER_GUIDE.md` | User, tool, developer, and release instructions |
| `assets/` | Original logo, icon, and social preview artwork |
| `.github/workflows/` | Windows build/test and release automation |
