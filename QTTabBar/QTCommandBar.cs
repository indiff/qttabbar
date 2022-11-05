//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano, indiff
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    [ComVisible(true), Guid("d2bf470e-ed1c-487f-a888-2bd8835eb6ce")]
    public sealed class QTCommandBar : BandObject
    {
        [ComRegisterFunction]
        private static void Register(Type t)
        {
            string name = t.GUID.ToString("B");
            string str = "QT Command Bar 2";
            using (RegistryKey subKey = Registry.ClassesRoot.CreateSubKey("CLSID\\" + name))
            {
                subKey.SetValue((string)null, (object)str);
                subKey.SetValue("MenuText", (object)str);
                subKey.SetValue("HelpText", (object)str);
            }

            using (RegistryKey subKey =
                   Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar"))
                subKey.SetValue(name, (object)"QTCommandBar");
        }


        [ComUnregisterFunction]
        private static void Unregister(Type t)
        {
            string str = t.GUID.ToString("B");
            try
            {
                using (RegistryKey subKey =
                       Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar"))
                    subKey.DeleteValue(str, false);
            }
            catch
            {
            }

            try
            {
                using (RegistryKey subKey = Registry.ClassesRoot.CreateSubKey("CLSID"))
                {
                    // subKey.DeleteSubKeyTree(str, false);
                    subKey.DeleteSubKeyTree(str);
                }
            }
            catch
            {
            }
        }
    }
}