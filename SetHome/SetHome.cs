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
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 只允许一个实例运行
            var exeName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            Process[] processes = Process.GetProcessesByName(exeName);
            if (processes.Length > 1)
            {
                // MessageBox.Show(exeName + "已经启动");
                Thread.Sleep(333);
                System.Environment.Exit(1);
            }

            // 测试DPI兼容 indiff
            // PInvoke.SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
             /**
              * 当前用户是管理员的时候，直接启动应用程序
              * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
              */
             //获得当前登录的Windows用户标示
             System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
             System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
             //判断当前登录用户是否为管理员
             if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
             {
                 //如果是管理员，则直接运行
                 Application.Run(new SetHomeForm(args));
             }
             else
             {
                 //创建启动对象
                 System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                 startInfo.UseShellExecute = true;
                 startInfo.WorkingDirectory = Environment.CurrentDirectory;
                 startInfo.FileName = Application.ExecutablePath;
                 //设置启动动作,确保以管理员身份运行
                 startInfo.Verb = "runas";
                 try
                 {
                     System.Diagnostics.Process.Start(startInfo);
                 }
                 catch
                 {
                     return;
                 }
                 //退出
                 Application.Exit();
             }
        }
    }
}
