using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace SetHome
{
    static class SetHome
    {
        /// <summary>
        /// The application's main entry point.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Only allow one instance to run
            var exeName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            Process[] processes = Process.GetProcessesByName(exeName);
            if (processes.Length > 1)
            {
                // MessageBox.Show(exeName + "is already running");
                Thread.Sleep(333);
                System.Environment.Exit(1);
            }

            // Test DPI compatibility, indiff
            // PInvoke.SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
             /**
              * If the current user is an administrator, start the application directly
              * If not, use a launch object to start the program, to ensure it runs as administrator
              */
             //Get the currently logged-in Windows user identity
             System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
             System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
             //Check whether the current user is an administrator
             if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
             {
                 //If administrator, run directly
                 Application.Run(new SetHomeForm(args));
             }
             else
             {
                 //Create the launch object
                 System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                 startInfo.UseShellExecute = true;
                 startInfo.WorkingDirectory = Environment.CurrentDirectory;
                 startInfo.FileName = Application.ExecutablePath;
                 //Set the launch verb, to ensure it runs as administrator
                 startInfo.Verb = "runas";
                 try
                 {
                     System.Diagnostics.Process.Start(startInfo);
                 }
                 catch
                 {
                     return;
                 }
                 //Exit
                 Application.Exit();
             }
        }
    }
}
