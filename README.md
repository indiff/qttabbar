# QTTabBar

[![GitHub all releases](https://img.shields.io/github/downloads/indiff/qttabbar/total)](https://github.com/indiff/qttabbar/releases)
[![Github Stars](https://img.shields.io/github/stars/indiff/qttabbar?logo=github)](https://github.com/indiff/qttabbar)
[![.NET Framework 4.8](https://img.shields.io/badge/.NET-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)

QTTabBar adds tabbed browsing and other enhancements to Windows Explorer. This is a fork of [indiff/qttabbar](https://github.com/indiff/qttabbar), updated for .NET 4.8 and modern Windows.

![qttabbar2](https://user-images.githubusercontent.com/501276/131287626-fe8f1fdd-a894-43f8-9620-b7145d70936d.gif)

## Changes

- [1.5.6.2 (2026)](https://github.com/indiff/qttabbar/releases/tag/v1.5.6.2) Upgrade to .NET 4.8; fix options dialog crash and empty window; bars disabled by default; installer restarts Explorer correctly
- [1.5.6.1-beta (2024)](https://github.com/indiff/qttabbar/releases/tag/v1.5.6.-beta.1) Fix auto select
- [1.5.5.9-beta (2023)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5-beta.9) Capture to select, fix preview text encoding
- [1.5.5.8-beta (2022)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5-beta.8) No Plugins Mini Version
- [1.5.5.7-beta (2022)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5-beta.7) Dark mode support, background image support
- [1.5.5.6-beta (2022)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5-beta.6) Added Brazilian, Spanish, French, Turkish languages
- [1.5.5.5-beta (2022)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5.5-beta) Debug log, DPI adjustment
- [1.5.5.4-beta (2022)](https://github.com/indiff/qttabbar/releases/tag/1.5.5.4-beta) Ignore control panel capture
- [1.5.5.3-beta (2021)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5.3) SetHome tool, Portuguese (Brazil)
- [1.5.5.2-beta (2021)](https://github.com/indiff/qttabbar/releases/tag/1.5.5.2-beta) Built-in German, fix version number
- [1.5.5 (2021)](https://github.com/indiff/qttabbar/releases/tag/1.5.5.1-beta) Fix Explorer crash on options open, mouse hover activation plugin
- [1.5.4 (2021)](https://github.com/indiff/qttabbar/releases/tag/1.5.4-beta) All plugins built-in, fix lock function bug
- [1.5.3 (2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.3-beta) Custom button images, clipboard path in new tab, video preview
- [1.5.2 (2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.2) Fix command prompt exception, exception log improvements
- [1.4 (2020)](https://github.com/indiff/qttabbar/releases/tag/1.4) Fix hotkey conflicts, create empty file
- [1.3](https://github.com/indiff/qttabbar/releases/tag/1.3) Plugin deduplication and sorting
- [1.2](https://github.com/indiff/qttabbar/releases/tag/1.2) Windows 10 support, fix link failures
- [1.1](https://github.com/indiff/qttabbar/releases/tag/1.1) Localized installer UI
- [1.0](https://github.com/indiff/qttabbar/releases/tag/1.0) Built-in Chinese language

## Download

* [Latest release](https://github.com/niklas2233/qttabbar/releases)

## Installation

1. Run `QTTabBar Setup.exe` — it will install .NET Framework 4.8 automatically if not present
2. After installation, open a File Explorer window
3. Enable the toolbars: **View → Options** (Windows 10/11) or right-click the toolbar area and check **QTTabBar** and **QT ButtonBar**

Error logs are written to `%APPDATA%\QTTabBar\QTTabBarException.log`.

## Build

**Requirements:**
- Visual Studio 2022
- [WiX Toolset v3.14](https://github.com/wixtoolset/wix3/releases) (command-line tools)
- [NotifyPropertyWeaver](https://github.com/SimonCropp/NotifyPropertyWeaver) (included in `Tools\`)

**Steps:**

```powershell
# Build the main DLL
msbuild QTTabBar\QTTabBar.csproj /p:Configuration=Release /p:SolutionDir="$pwd\\"

# Build the MSI (run from the Installer\ directory)
cd Installer
$wix = "C:\Program Files (x86)\WiX Toolset v3.14\bin"
& "$wix\candle.exe" -ext "$wix\WixNetFxExtension.dll" -ext "$wix\WixUIExtension.dll" -ext "$wix\WixUtilExtension.dll" -out obj\Release\ Installer.wxs CustomWelcomeEulaDlg.wxs CustomWixUI_Minimal.wxs
& "$wix\light.exe"  -ext "$wix\WixNetFxExtension.dll" -ext "$wix\WixUIExtension.dll" -ext "$wix\WixUtilExtension.dll" -cultures:en-US -loc lang.wxl -sice:ICE80 -sice:ICE61 -out "bin\Release\en-US\QTTabBar Setup.msi" obj\Release\Installer.wixobj obj\Release\CustomWelcomeEulaDlg.wixobj obj\Release\CustomWixUI_Minimal.wixobj

# Build the bootstrapper EXE
& "$wix\candle.exe" -ext "$wix\WixBalExtension.dll" -ext "$wix\WixNetFxExtension.dll" -out obj\Release\Bundle\Bundle.wixobj Bundle.wxs
& "$wix\light.exe"  -ext "$wix\WixBalExtension.dll" -ext "$wix\WixNetFxExtension.dll" -out "bin\Release\QTTabBar Setup.exe" obj\Release\Bundle\Bundle.wixobj
```

Output: `Installer\bin\Release\QTTabBar Setup.exe` (bootstrapper with embedded MSI)

## Thanks

* [Original author Quizo](https://twitter.com/QTTabBar)
* [indiff](https://github.com/indiff/qttabbar) — upstream fork maintainer
* [SF Version Author](https://sourceforge.net/u/masamunexgp/profile)
