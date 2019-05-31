using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QTTabBarLib;
using QTPlugin;
using System.Windows.Forms;
using System.Diagnostics;
namespace QTCmd
{
    [Plugin(PluginType.Background, Author = "qwop", Name = "qt cmd", Version = "0.0.0.1", Description = "Some qttabbar command.")]
    class Program
    {
        static void Main(string[] args)
        {
          // string path = Environment.GetEnvironmentVariable( "systemroot" );

         //  QTTabBarClass qt = new QTTabBarClass();
         // bool ret = QTTabBarClass.CreateTab(qt, new Address(path), -1, false, false);

         // Console.WriteLine(ret);
            
 
//System.Globalization.CultureInfo.InstalledUICulture.NativeName
         //  Console.WriteLine(System.Globalization.CultureInfo.InstalledUICulture.Name);
         //  Console.WriteLine(System.Globalization.CultureInfo.InstalledUICulture.NativeName);
            
            string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "QTTabBar");
            string filename = IntPtr.Size == 8 ? "QTHookLib64.dll" : "QTHookLib32.dll";

            Console.WriteLine(installPath);

            Console.WriteLine(filename);

            //HookLibManager.Initialize_old();
            string path = "c:/windows";


            if (Directory.Exists(path))
            {
                ProcessStartInfo info = new ProcessStartInfo("cmd");
                info.WorkingDirectory = path;
                Process.Start(info);
            }
            
            // Process.Start("cmd /k ");
           // Process.Start("c:/windows/system32/cmd.exe /k cd \"c:/windows\"");
           
        }
    }
}
