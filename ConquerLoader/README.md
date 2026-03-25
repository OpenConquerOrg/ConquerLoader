# ConquerLoader

`ConquerLoader` is a launcher for Conquer Online clients with a WPF-based main UI, compatibility with legacy WinForms forms, and a plugin system that now includes pre-launch autopatching.

## Current Status

- The main launcher used by the app is WPF.
- WPF windows live under `ConquerLoader/Forms/WPF/`.
- Legacy WinForms forms are still kept under `ConquerLoader/Forms/` for compatibility and behavioral reference.
- The plugin system still works for regular plugins and now also supports pre-launch hooks.

## Solution Structure

- `ConquerLoader/`
  Main application, WPF launcher, legacy WinForms forms, and launch logic.
- `CLCore/`
  Shared models, constants, plugin system, and common contracts.
- `CLServer/`
  Supporting components related to server-side tooling in the ecosystem.
- `CLAutoPatchPlugin/`
  Official autopatch plugin based on compressed `.zip` or `.rar` packages.
- `ExamplePlugin/`
  Example plugin for simple extensions.

## Main UI

The application starts from:

- `ConquerLoader/Forms/WPF/MainLite.xaml`
- `ConquerLoader/Forms/WPF/MainLite.xaml.cs`

The current launcher layout is built around:

- server selection on the left
- the main enter action in the center
- launch options on the right
- a guided first-run wizard when the user has not configured any servers yet

## Plugin System

Plugins are discovered from the `Plugins` folder and still implement `IPlugin`.

There is also a pre-launch extension point for tasks that must run before the client is started:

- `CLCore/PluginSystem/IPreLaunchPlugin.cs`
- `CLCore/PluginSystem/PluginPreLaunchContext.cs`
- `CLCore/PluginSystem/PluginPreLaunchResult.cs`

Execution of those plugins is handled from:

- `ConquerLoader/Core.cs`

## CLAutoPatchPlugin

`CLAutoPatchPlugin` is the official autopatch plugin included in the solution.

Current capabilities:

- JSON manifest based on `packages` or `archives`
- compressed package application instead of individual file entries
- native support for `.zip`
- support for `.rar` through `UnRAR.exe` or `WinRAR.exe` installed on Windows
- persistent state so the same package is not reapplied when its signature has not changed
- its own WPF configuration window

Key files:

- `CLAutoPatchPlugin/CLAutoPatchPlugin.cs`
- `CLAutoPatchPlugin/AutoPatchManifest.cs`
- `CLAutoPatchPlugin/AutoPatchState.cs`
- `CLAutoPatchPlugin/AutoPatchConfigurationWindow.xaml`
- `CLAutoPatchPlugin/README.md`
- `CLAutoPatchPlugin/manifest.sample.json`

Important:

- Older manifests using `files` are no longer valid for the new autopatch flow.
- The current plugin format is documented in detail in `CLAutoPatchPlugin/README.md`.

## Launch Flow

The overall launcher flow is:

1. load loader configuration
2. load and initialize plugins
3. prepare the launch environment depending on client version, DX8 or DX9, and required resources
4. execute pre-launch plugins with `Core.RunPreLaunchPlugins(...)`
5. if everything succeeds, start the client

If `CLAutoPatchPlugin` is enabled and configured to block on error, a patch failure cancels the launch.

## Build

Relevant projects:

- `ConquerLoader.sln`
- `ConquerLoader/ConquerLoader.csproj`
- `CLAutoPatchPlugin/CLAutoPatchPlugin.csproj`

The autopatch plugin automatically copies `CLAutoPatchPlugin.dll` to:

`ConquerLoader/bin/<Configuration>/Plugins/`

## Additional Documentation

For autopatch implementation details:

- `CLAutoPatchPlugin/README.md`

For a sample manifest:

- `CLAutoPatchPlugin/manifest.sample.json`
