# QTTabBar

[![GitHub all releases](https://img.shields.io/github/downloads/niklas2233/qttabbar/total)](https://github.com/niklas2233/qttabbar/releases)
[![Github Stars](https://img.shields.io/github/stars/niklas2233/qttabbar?logo=github)](https://github.com/niklas2233/qttabbar)
[![.NET Framework 4.8](https://img.shields.io/badge/.NET-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)

QTTabBar adds tabbed browsing and other enhancements to Windows Explorer. This is a fork of [indiff/qttabbar](https://github.com/indiff/qttabbar), updated for .NET 4.8 and modern Windows.

![qttabbar2](https://user-images.githubusercontent.com/501276/131287626-fe8f1fdd-a894-43f8-9620-b7145d70936d.gif)

## Changes

See [CHANGELOG.md](CHANGELOG.md) for the full version history.

## Download

* [Latest release](https://github.com/niklas2233/qttabbar/releases)

## Installation

1. Run `QTTabBar Setup.exe` — it will install .NET Framework 4.8 automatically if not present
2. After installation, open a File Explorer window
3. Activate QTTabBar — the steps differ by Windows version (see below)

### Activating on Windows 10

Explorer still has a classic toolbar, so enable the bands the normal way: right-click an empty part of the toolbar area (or use **View → Options**) and tick **QTTabBar** and **QT ButtonBar**. This gives the full experience, including QTTabBar's own tab bar.

### Activating on Windows 11

Windows 11 removed the classic Explorer toolbar, so there's no toolbar menu to enable QTTabBar through. Instead, turn on the auto-attach option:

1. Right-click an empty area inside any folder and choose **QTTabBar Options**.
2. On the **Window** tab, tick **“Enable QTTabBar on every Explorer window (experimental, must restart Explorer)”**.
3. Click **OK**, then restart Explorer — either sign out and back in, or open Task Manager, find **Windows Explorer**, and click **Restart**.

![QTTabBar Options — Enable QTTabBar on every Explorer window](docs/images/win11-enable-on-every-window.png)

On Windows 11, QTTabBar works alongside Explorer's own native tabs rather than showing its own tab bar. Once enabled you get:

- **Double-click** an empty area of a folder to go up one level
- **Hover previews** of text files and photos/media files

Error logs are written to `%APPDATA%\QTTabBar\QTTabBarException.log`.

## Build

**Requirements:**
- Visual Studio 2022
- [WiX Toolset v3.14](https://github.com/wixtoolset/wix3/releases) (command-line tools)
- [NotifyPropertyWeaver](https://github.com/SimonCropp/NotifyPropertyWeaver) (included in `Tools\`)

**Day-to-day code changes** (no installer needed):

```powershell
msbuild QTTabBar\QTTabBar.csproj /p:Configuration=Release /p:SolutionDir="$pwd\\"
```

**Cutting a versioned release installer:**

```powershell
.\Installer\Build-Installer.ps1 -Version 1.5.6.4
```

This is the *only* place the version number needs to be typed — the script stamps it into `AssemblyInfo.cs`, `Installer.wxs`, and `Bundle.wxs`, then builds the DLL, MSI, and bootstrapper. (`QTUtility.CurrentVersion`, shown in the About tab, reads the version from the built assembly at runtime instead of being hardcoded separately.)

Output: `Installer\bin\Release\QTTabBar Setup <version>.exe` (bootstrapper with embedded MSI)

## Thanks

* [Original author Quizo](https://twitter.com/QTTabBar)
* [indiff](https://github.com/indiff/qttabbar) — upstream fork maintainer
* [SF Version Author](https://sourceforge.net/u/masamunexgp/profile)
