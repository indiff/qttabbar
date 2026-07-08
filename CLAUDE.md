# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

QTTabBar adds tabbed browsing and other enhancements to Windows Explorer. It's a Windows Explorer **band object / deskband** COM extension (not a standalone app) — it gets loaded into every `explorer.exe` process. This fork targets .NET Framework 4.8 and modern Windows (10/11); it descends from `indiff/qttabbar`, itself a fork of the original Quizo project.

## Build

Requires Visual Studio 2022, [WiX Toolset v3.14](https://github.com/wixtoolset/wix3/releases) (for the installer), and NotifyPropertyWeaver (bundled in `Tools\`). Windows-only — build/run everything through PowerShell.

```powershell
# Main DLL
msbuild QTTabBar\QTTabBar.csproj /p:Configuration=Release /p:SolutionDir="$pwd\\"

# Full solution
msbuild "QTTabBar Rebirth.sln" /p:Configuration=Release
```

MSI/bootstrapper build steps (WiX `candle`/`light` invocations) are in README.md under "Build" — needed only when packaging a release, not for day-to-day code changes.

## Tests

```powershell
dotnet restore QTTabBar.Tests\QTTabBar.Tests.csproj
msbuild QTTabBar.Tests\QTTabBar.Tests.csproj /p:Configuration=Release
dotnet test QTTabBar.Tests\QTTabBar.Tests.csproj --no-build --configuration Release

# Single test
dotnet test QTTabBar.Tests\QTTabBar.Tests.csproj --no-build --configuration Release --filter "FullyQualifiedName~EnumContractTests.BindAction_GoBack_Is0"
```

MSTest, `net48`. The test project is currently thin (`QTTabBar.Tests\EnumContractTests.cs`, `XmlSerializableFontTests.cs`) and references `QTTabBar.csproj` directly — most of it is UI/COM interop that isn't unit-testable, so tests focus on things that silently corrupt user data if changed carelessly.

**`EnumContractTests` exists because enum ordinals are persisted directly to user settings/registry.** `BindAction` and `MouseChord` values must never be renumbered/reordered — doing so silently corrupts every existing user's keybindings. If you add new enum members, append them; if you touch these enums, update/extend this test rather than removing it.

CI (`.github/workflows/QTTabBar.yml`) runs `nuget restore` → builds the test project → `dotnet test` on every push/PR to `master`, and additionally builds/uploads the release DLL and publishes it on tag pushes. CodeQL (`ql.yml`) runs separately on the main `QTTabBar.csproj`.

## Architecture

### Process model
QTTabBar is not one executable — it's a DLL (`QTTabBar\QTTabBar.csproj`) registered as a COM deskband/band object that Explorer loads into its own process (potentially many `explorer.exe` / `dllhost.exe` instances at once). `QTTabBarClass` (`QTTabBar\QTTabBarClass.cs`), extending `TabBarBase`, is the main band object entry point. `BandObjectLib` provides the generic COM deskband scaffolding (`IDeskBand2`, `IObjectWithSite`, etc.) that `QTTabBarClass` builds on.

Because the DLL runs *inside* Explorer, most bugs here are about COM lifetime, thread affinity (STA), and not crashing the host process — an unhandled exception can take down the user's Explorer window. Exceptions are logged to `%APPDATA%\QTTabBar\QTTabBarException.log`.

### Native interop layer
Deep Explorer integration (subclassing list views, tracking navigation, injecting into the shell's internal window messages) requires calling into undocumented/private Windows shell interfaces. This is split across:
- `QTTabBar\Interop\` — P/Invoke signatures, private shell COM interfaces (`IShellBrowser`, `IFolderView2`, `ITravelLogEntry`, etc.), Win32 structs.
- `QTTabBar\Common\` — a large embedded copy of the Windows API Code Pack shell library (`ShellObject`, `KnownFolders`, property system, thumbnails) — treat as vendored, not first-party.
- `QTHookLib` (C++, `vcxproj`) + `MinHook` (C++, vendored inline-hooking library) — a native DLL injected into Explorer to hook window procedures Explorer doesn't expose any other way. `HookLibManager.cs` (`QTTabBar\HookLibManager.cs`) is the managed side: it loads the native hook DLL, registers C-callable delegates (`HookLibCallback`, `NewWindowCallback`) as callbacks, and communicates hook results back across the native/managed boundary.
- `Register` and `InstallerHelper` (C++, `vcxproj`) — native helpers invoked by the WiX installer for COM registration/custom actions, not part of the runtime app.

When editing anything under `Interop\`/`Common\`/`QTHookLib`, assume the target APIs are undocumented and version-sensitive across Windows releases — test on the actual Explorer shell, not just compile-clean.

### Plugin system
`QTPluginLib` defines the plugin contract (`IPluginClient`, `IPluginServer`, `IBarButton`/`IBarDropButton`/`IBarCustomItem`, `PluginAttribute`) as a separate assembly so third-party plugin DLLs can reference just the contract. `QTTabBar\PluginManager.cs` and `PluginServer.cs` discover, load, and host plugins at runtime. Built-in plugins live under `Plugins\` (one `csproj` per plugin: `QTClock`, `QTFileTools`, `ShowStatusBar`, `MigemoLoader`, `Memo`, etc.) — each compiles to its own DLL and is loaded the same way a third-party plugin would be. `Plugins\Sample\` is the reference implementation to copy when adding a new plugin.

### Settings & options UI
User settings persist to the registry (`RegistryUtil.cs`, `StaticReg.cs`, `Config.cs`). The options UI is WPF (`QTTabBar\OptionsDialog\Options0N_*.xaml[.cs]`, one file pair per settings tab: Window, Tabs, Tweaks, Tooltips, General, Appearance, Mouse, Keys, Groups, Apps, ButtonBar, Plugins, Language, About) hosted from a WinForms-based host app (most of the rest of the UI, e.g. `QTabControl.cs`, `Toolbar.cs`, `SubDirTipForm.cs`, is WinForms). Expect to bridge between WPF and WinForms when touching options-related UI.

### Localization
`Resources_String*.resx`/`.cs` — one resx per built-in language (`de_DE`, `es_ES`, `fr_FR`, `pt_BR`, `ru_RU`, `tr_TR`, plus `zh_CN` handled separately via `Resource_String_zh_CN.*`). `Translations\` (repo root) holds source translation files: `.txt` resgen input for the built-in `.resx` resources (via an old `resgen.bat`, hardcoded to an obsolete 2010-era path — not part of the current build) and `.xml` runtime-loadable language packs matching `Options13_Language.xaml`'s Import/Export Language feature. `QTTabBar\Multilang\` is unrelated — COM interop for `IMultiLanguage`/charset conversion, not translation content.

## Working conventions

- This is old, actively-being-modernized code (recent history: migrated `AsyncHelper` → `Task.Run`, removed dead code, fixed options-dialog crashes). Favor small, targeted diffs over broad rewrites — the codebase has decades of undocumented Explorer-version-specific behavior baked in, and "obviously dead" code near shell interop is often a workaround for a specific Windows version.
- Don't touch `QTTabBar\Common\` shell library internals unless the bug is actually there — it's vendored (Windows API Code Pack) rather than original QTTabBar code.
- `QTTabBar\todo.txt` currently only tracks a pending translation string — not a general task list.
