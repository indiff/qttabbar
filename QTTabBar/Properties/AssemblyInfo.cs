//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("QTTabBar")]
[assembly: AssemblyDescription("QTTabBar Plugin")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("indiff")]
[assembly: AssemblyProduct("QTTabBar")]
[assembly: AssemblyCopyright("Copyright (C)  2007-2026")]
[assembly: AssemblyTrademark("indiff")]
[assembly: AssemblyCulture("")]

// Resources for the neutral culture live in this assembly, not a satellite.
[assembly: NeutralResourcesLanguage("zh-CN")]

// The band object is a COM server, so the assembly's types must be visible to COM
// and the typelib id must stay stable - the installer registers it by this GUID.
[assembly: ComVisible(true)]
[assembly: Guid("76430850-7643-0850-7643-2bd8835eb6ce")]

// Installer\Build-Installer.ps1 rewrites both of these from its -Version argument,
// and stamps the same value into Installer.wxs as StrongName. They must stay in this
// exact AssemblyVersion("x.y.z.w") shape or the regex silently misses and the MSI ends
// up registering a strong name the built assembly does not have.
[assembly: AssemblyVersion("1.6.1.0")]
[assembly: AssemblyFileVersion("1.6.1.0")]
