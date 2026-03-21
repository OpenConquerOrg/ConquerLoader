# AutoPatch Plugin

## What It Does

`AutoPatchPlugin` is an optional plugin for `ConquerLoader` that applies compressed patches before the client is launched.

It no longer downloads and writes individual files one by one. Instead, it works with a JSON manifest that lists `.zip` or `.rar` patch packages, validates them, and extracts them into the final client folder used by the launcher.

## How It Integrates With The Loader

The loader now exposes a pre-launch extension point:

- `ConquerLoader/Core.cs`
- `CLCore/PluginSystem/IPreLaunchPlugin.cs`
- `CLCore/PluginSystem/PluginPreLaunchContext.cs`
- `CLCore/PluginSystem/PluginPreLaunchResult.cs`

Actual flow:

1. `ConquerLoader` loads DLLs from `Plugins`
2. it creates instances for plugins implementing `IPlugin`
3. it runs `Init()` as usual
4. right before `Process.Start(...)`, the loader calls `Core.RunPreLaunchPlugins(...)`
5. `AutoPatchPlugin` receives the `PluginPreLaunchContext`
6. if autopatching fails and configuration requires it, launch is canceled

## Plugin Configuration

The plugin stores its configuration in:

`Plugins/AutoPatchPlugin.settings.json`

Fields:

- `Enabled`: enables or disables autopatching
- `ManifestLocation`: HTTP/HTTPS URL or local path to a manifest JSON file
- `FailLaunchOnError`: when `true`, cancels launch if patching fails
- `RelativeTargetFolder`: optional relative folder inside the client working directory

Applied package state is stored in:

`Plugins/AutoPatchPlugin.state.json`

This state is used to avoid reapplying the same patch package every time if its signature has not changed.

## Manifest Format

The plugin expects JSON in this format:

```json
{
  "version": "1.0.0",
  "baseUrl": "https://cdn.example.com/patch/",
  "packages": [
    {
      "id": "base-client",
      "archive": "base-client-100.zip",
      "format": "zip",
      "extractTo": ".",
      "sha256": "HEX_OR_BASE64_SHA256"
    },
    {
      "id": "hd-textures",
      "url": "patches/hd-textures.rar",
      "format": "rar",
      "extractTo": "data"
    }
  ]
}
```

`archives` is also accepted as an alias for `packages`.

Per-package fields:

- `id`: stable package identifier
- `archive`: file name or relative path to the compressed package
- `url`: optional source; if present, it takes priority over `archive`
- `format`: `zip` or `rar`; if missing, the plugin tries to infer it from the extension
- `extractTo`: relative folder inside the client where the package should be extracted
- `size`: optional compressed package size
- `sha256`: optional compressed package hash
- `enabled`: allows disabling a package from the manifest

## Important Rule For Older Manifests

The new plugin version no longer supports manifests like this:

```json
{
  "files": [...]
}
```

If the manifest still uses `files`, the plugin treats it as a legacy format and fails with a clear message so it can be migrated to the package-based model.

## How Paths Are Resolved

Destination:

- the base destination is the client `WorkingDirectory`
- if `RelativeTargetFolder` has a value, it is combined with that `WorkingDirectory`
- `extractTo` is resolved relative to that destination
- the plugin blocks unsafe paths using `..` if they attempt to escape the target folder

Source:

- if the manifest is loaded over HTTP/HTTPS, packages can also come from HTTP/HTTPS
- if the manifest is local, packages can also be local
- `baseUrl` can be either a URL or a local path
- `baseUrl` and `archive` or `url` are combined relatively when needed

## ZIP And RAR

### ZIP

`.zip` packages are extracted directly through .NET using `System.IO.Compression`.

### RAR

`.rar` packages are supported through a tool installed on the machine:

- `UnRAR.exe`
- `WinRAR.exe`

The plugin tries to locate them in `PATH` and in common WinRAR installation paths.

If neither tool exists and the manifest includes a `.rar` package, autopatching fails with a clear message.

## Exact Launch Order

In both `MainLite` and `Main`, the flow becomes:

1. prepare DX8 or DX9 environment if needed
2. regenerate `server.dat` or pre-launch DLLs if required
3. build the `PluginPreLaunchContext`
4. execute `Core.RunPreLaunchPlugins(...)`
5. `AutoPatchPlugin` applies pending patch packages
6. if everything succeeds, the client is started

That means autopatching runs against the real final client folder that will be used for launch.

## What "Already Applied" Means

The plugin stores a signature per package and destination. That signature includes:

- `manifest.version`
- `id`
- `archive`
- `url`
- `format`
- `extractTo`
- `size`
- `sha256`

If that signature does not change, the package is considered already applied and is skipped.

The cleanest way to force reapplication is to change `version`, `sha256`, or both.

## Important Files

- `AutoPatchPlugin/AutoPatchPlugin.cs`: main autopatch logic
- `AutoPatchPlugin/AutoPatchManifest.cs`: new manifest model
- `AutoPatchPlugin/AutoPatchState.cs`: applied package state
- `AutoPatchPlugin/AutoPatchSettings.cs`: persisted configuration
- `AutoPatchPlugin/AutoPatchSettingsStore.cs`: JSON settings persistence
- `AutoPatchPlugin/AutoPatchConfigurationWindow.xaml`: WPF UI
- `AutoPatchPlugin/AutoPatchConfigurationWindow.xaml.cs`: WPF code-behind
- `AutoPatchPlugin/manifest.sample.json`: sample manifest

## Build And Deployment

The project:

- is included in `ConquerLoader.sln`
- builds as a .NET Framework 4.6.2 library
- automatically copies `AutoPatchPlugin.dll` to `ConquerLoader/bin/<Configuration>/Plugins/`

## Current Limitations

- there is no binary delta patching
- there is no automatic rollback
- `.rar` support depends on an external tool installed on Windows
- applied-package detection is based on stored package signatures, not on full verification of already extracted content
