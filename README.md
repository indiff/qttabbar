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
3. Enable the toolbars: **View → Options** (Windows 10/11) or right-click the toolbar area and check **QTTabBar** and **QT ButtonBar**

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
