<#
.SYNOPSIS
    Bumps QTTabBar's version everywhere it needs to live, then builds the DLL,
    MSI, and bootstrapper. Run from any directory.

.EXAMPLE
    .\Installer\Build-Installer.ps1 -Version 1.6.0 -Auto
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(\.\d+)?$')]
    [string]$Version = "1.6.0",
	
	
    [Parameter(Mandatory = $false)]
    [switch]$Auto
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$installerDir = $PSScriptRoot

# The CLR always pads an assembly identity to 4 parts (AssemblyVersion "1.5.7"
# loads as "1.5.7.0"), and the installer's StrongName must match that padded
# identity exactly or COM/assembly binding breaks. So a 3-part release version
# stays 3-part everywhere user-facing (product name, MSI filename, MSI/bundle
# ProductVersion) but the assembly identity and StrongName get the .0.
$assemblyVersion = if ($Version -match '^\d+\.\d+\.\d+$') { "$Version.0" } else { $Version }

# --- 1. Stamp the version into the three files that still need it ---------

$Force_Remove_Path = Join-Path $root "MinHook\bin\"
if (Test-Path $Force_Remove_Path) {
    Remove-Item -Path $Force_Remove_Path -Recurse -Force -ErrorAction SilentlyContinue
}
$Force_Remove_Path = Join-Path $root "MinHook\obj\"
if (Test-Path $Force_Remove_Path) {
    Remove-Item -Path $Force_Remove_Path -Recurse -Force -ErrorAction SilentlyContinue
}
$Force_Remove_Path = Join-Path $root "QTHookLib\obj\"
if (Test-Path $Force_Remove_Path) {
    Remove-Item -Path $Force_Remove_Path -Recurse -Force -ErrorAction SilentlyContinue
}
$Force_Remove_Path = Join-Path $root "QTHookLib\bin\"
if (Test-Path $Force_Remove_Path) {
    Remove-Item -Path $Force_Remove_Path -Recurse -Force -ErrorAction SilentlyContinue
}

$assemblyInfo = Join-Path $root "QTTabBar\Properties\AssemblyInfo.cs"
(Get-Content $assemblyInfo -Raw) `
    -replace 'AssemblyVersion\("[\d.]+"\)', "AssemblyVersion(`"$assemblyVersion`")" `
    -replace 'AssemblyFileVersion\("[\d.]+"\)', "AssemblyFileVersion(`"$assemblyVersion`")" `
    | Set-Content $assemblyInfo -NoNewline
Write-Host "Updated $assemblyInfo"

$installerWxs = Join-Path $installerDir "Installer.wxs"
(Get-Content $installerWxs -Raw) `
    -replace '(?<=<\?define ProductVersion=")[\d.]+', $Version `
    -replace '(?<=<\?define VersionString=")[\d.]+', $Version `
    -replace '(?<=StrongName="QTTabBar, Version=)[\d.]+', $assemblyVersion `
    | Set-Content $installerWxs -NoNewline
Write-Host "Updated $installerWxs"

$uninstallDirectoriesFile = Join-Path $installerDir "UninstallDirectories.txt"
$uninstallDirectories = if (Test-Path $uninstallDirectoriesFile) {
    (Get-Content $uninstallDirectoriesFile | Where-Object {
        $line = $_.Trim()
        $line.Length -gt 0 -and -not $line.StartsWith('#')
    }) -join "`r`n"
} else {
    ""
}
$uninstallDirectories = $uninstallDirectories.Replace('&', '&amp;').Replace('"', '&quot;')
Write-Host "Loaded uninstall directory configuration from $uninstallDirectoriesFile"

$bundleWxs = Join-Path $installerDir "Bundle.wxs"
(Get-Content $bundleWxs -Raw) `
    -replace '(?<=<Bundle Name="QTTabBar )[\d.]+', $Version `
    -replace '(?<=\n            Version=")[\d.]+(?=")', $Version `
    -replace '(?<=QTTabBar Setup )[\d.]+(?=\.msi")', $Version `
    | Set-Content $bundleWxs -NoNewline
Write-Host "Updated $bundleWxs"

 
$QTUtility = Join-Path $root "QTTabBar\QTUtility.cs"
$BuildVerion = if ((Get-Content $QTUtility -Raw) -match 'BuildVerion = "\d+"; // (\d+)') { [int]$matches[1] } else { 0 }

if ($Auto) {
	$BuildVerion = $BuildVerion + 1
	(Get-Content $QTUtility -Raw) `
		-replace '(BuildVerion = ")\d+("; // )\d+(.*)', "`${1}$BuildVerion`${2}$BuildVerion`${3}" `
		| Set-Content $QTUtility -NoNewline
	Write-Host "Updated $QTUtility (BuildVerion -> $BuildVerion)" -ForegroundColor Green
} else {
	(Get-Content $QTUtility -Raw) `
		-replace '(BuildVerion = ")\d+("; // )(\d+.*)', "`${1}0`${2}`${3}" `
		| Set-Content $QTUtility -NoNewline
	Write-Host "Updated $QTUtility (BuildVerion -> 0)" -ForegroundColor Red
}

# --- 2. Build every payload the installer packages ---------------------

# On CI, microsoft/setup-msbuild puts msbuild on PATH; locally fall back to the
# Visual Studio 2022 install.
$msbuild = (Get-Command msbuild -ErrorAction SilentlyContinue).Source
if (-not $msbuild) {
    $msbuild = Get-ChildItem "C:\Program Files*\Microsoft Visual Studio\2022\*\MSBuild\Current\Bin\MSBuild.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $msbuild) { throw "MSBuild.exe not found" }

# Build the code projects explicitly rather than the whole .sln: the solution
# also contains Installer.wixproj, which msbuild builds in parallel before the
# plugin DLLs exist (WiX projects have no project references to them), so a
# solution build fails with LGHT0103. QTTabBar.csproj pulls in BandObjectLib and
# QTPluginLib via project references; the plugins and SetHome are standalone.
# (Sample/SamplePlugin is intentionally omitted - the installer doesn't ship it.)
$managedProjects = @(
    "QTTabBar\QTTabBar.csproj"
    "Plugins\CreateNewItem\CreateNewItem.csproj"
    "Plugins\QTClock\QTClock.csproj"
    "Plugins\QTFileTools\QTFileTools.csproj"
    "Plugins\QTFolderButton\QTFolderButton.csproj"
    "Plugins\QTViewModeButton\ViewModeButton.csproj"
    "Plugins\QTWindowManager\QTWindowManager.csproj"
    "Plugins\ShowStatusBar\ShowStatusBar.csproj"
    "Plugins\MigemoLoader\MigemoLoader.csproj"
    "Plugins\Memo\Memo.csproj"
    "Plugins\QTQuick\QTQuick.csproj"
    "Plugins\TurnOffRepeat\TurnOffRepeat.csproj"
    "Plugins\ActivateByMouseHover\ActivateByMouseHover.csproj"
    "SetHome\SetHome.csproj"
)
foreach ($proj in $managedProjects) {
    & $msbuild "$root\$proj" /p:Configuration=Release "/p:SolutionDir=$root\" /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw "build failed: $proj" }
}

# Native hook, both platforms. The vcxproj files don't pin a PlatformToolset, so
# on a runner without VS2010 they default to v100 and fail with MSB8020 - force a
# modern toolset (v143 covers every VS2022+ MSVC). QTHookLib links libMinHook, so
# build that first for each platform. Win32 -> QTHookLib32.dll, x64 -> ...64.dll.
$vcProps = @('/p:PlatformToolset=v143', '/p:WindowsTargetPlatformVersion=10.0')
foreach ($plat in @('Win32', 'x64')) {
    & $msbuild "$root\MinHook\libMinHook.vcxproj" /p:Configuration=Release /p:Platform=$plat $vcProps /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw "libMinHook $plat build failed" }
    & $msbuild "$root\QTHookLib\QTHookLib.vcxproj" /p:Configuration=Release /p:Platform=$plat $vcProps /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw "QTHookLib $plat build failed" }
}

    & $msbuild "$root\InstallerHelper\InstallerHelper.vcxproj" /p:Configuration=Release /p:Platform=Win32 $vcProps /nologo /v:minimal


# --- 3. Build the MSI ---------------------------------------------------

$wix = Get-ChildItem "C:\Program Files*\WiX Toolset v3.*\bin\candle.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty DirectoryName
if (-not $wix) { throw "WiX Toolset v3.x not found (expected under 'WiX Toolset v3.*\bin')" }
Push-Location $installerDir
try {
    New-Item -ItemType Directory -Force -Path "obj\Release", "bin\Release\en-US", "bin\Release\zh-CN" | Out-Null

    & "$wix\candle.exe" -dQTUninstallDirectories="$uninstallDirectories" -ext "$wix\WixNetFxExtension.dll" -ext "$wix\WixUIExtension.dll" -ext "$wix\WixUtilExtension.dll" -out obj\Release\ Installer.wxs CustomWelcomeEulaDlg.wxs CustomWixUI_Minimal.wxs
    if ($LASTEXITCODE -ne 0) { throw "candle.exe failed on Installer.wxs" }

    & "$wix\light.exe" -ext "$wix\WixNetFxExtension.dll" -ext "$wix\WixUIExtension.dll" -ext "$wix\WixUtilExtension.dll" -cultures:en-US  `
		-loc lang.wxl `
		-sice:ICE80 -sice:ICE61  `
		-out "bin\Release\en-US\QTTabBar Setup $Version.msi" obj\Release\Installer.wixobj obj\Release\CustomWelcomeEulaDlg.wixobj  `
		obj\Release\CustomWixUI_Minimal.wixobj
    if ($LASTEXITCODE -ne 0) { throw "light.exe failed on Installer.wxs" }

    # --- 4. Build the bootstrapper EXE ---------------------------------

    New-Item -ItemType Directory -Force -Path "obj\Release\Bundle" | Out-Null

    & "$wix\candle.exe" -ext "$wix\WixBalExtension.dll" -ext "$wix\WixNetFxExtension.dll" -out obj\Release\Bundle\Bundle.wixobj Bundle.wxs
    if ($LASTEXITCODE -ne 0) { throw "candle.exe failed on Bundle.wxs" }
		
    & "$wix\light.exe" -ext "$wix\WixBalExtension.dll" -ext "$wix\WixNetFxExtension.dll" -out "bin\Release\QTTabBar Setup $Version.exe" obj\Release\Bundle\Bundle.wixobj
    if ($LASTEXITCODE -ne 0) { throw "light.exe failed on Bundle.wxs" }
}
finally {
    Pop-Location
}

$builtInstaller = Join-Path $installerDir "bin\Release\QTTabBar Setup $Version.exe"
Write-Host "`nBuilt: $builtInstaller"

if ($Auto) {
	# $BuildVerion = $BuildVerion + 1
	$newName = "QTTabBar Setup ${Version}_build${BuildVerion}.exe"
    
    Rename-Item -Path $builtInstaller -NewName $newName -Force
    
	$builtInstaller = Join-Path $installerDir "bin\Release\$newName"
    Write-Host "Renamed to: $builtInstaller" -ForegroundColor Green
}
