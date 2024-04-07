using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Management;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using System.Text;

namespace SetHome
{
    public partial class SetHomeForm : Form
    {
        private string[] args;


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg,
            UIntPtr wParam, string lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd,
            uint Msg, UIntPtr wParam, string lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout, out UIntPtr lpdwResult);

        /*[DllImport("shell32.dll")]
        static extern void SHChangeNotify(HChangeNotifyEventID wEventId,
            HChangeNotifyFlags uFlags,
            IntPtr dwItem1,
            IntPtr dwItem2);*/


        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// RefreshPolicyEx() causes group policy to be 
        /// applied immediately on the client machine. 
        /// </summary>
        /// <param name="bMachine">Refresh machine or user policy</param>
        /// <param name="dwOptions">Option specifying the kind 
        /// of refresh that needs to be done.</param>
        /// <returns>TRUE if successful.
        ///  FALSE if not. Call GetLastError() for more details</returns>
        /// <remarks>
        ///  [RefreshPolicyEx Function (Windows)]
        ///  http://msdn.microsoft.com/en-us/library/aa374401(VS.85).aspx 
        /// </remarks>
        [DllImport("Userenv.dll", EntryPoint = "RefreshPolicyEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RefreshPolicyEx([MarshalAs(
            UnmanagedType.Bool)] bool bMachine, RefreshOptions dwOptions);

        public enum RefreshOptions
        {
            /// <summary>Forces immediate Refresh</summary>
            RP_FORCE = 1
        }

        private enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0, SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2, SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        private const int WM_SETTINGCHANGE = 0x1a;
        private const int SMTO_ABORTIFHUNG = 0x0002;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, int fuFlags, int uTimeout, IntPtr lpdwResult);

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        /// <summary>
        /// 移除原路径字符串末尾反斜杠(\)，若原路径字符串不以反斜杠结尾则不作任何操作
        /// </summary>
        /// <param name="origin">原路径字符串</param>
        /// <returns>移除末尾反斜杠后的路径字符串</returns>
        public static string RemovePathEndBackslash(string origin)
        {
            while (origin.EndsWith("\\"))
            {
                origin = origin.Substring(0, origin.Length - 1);
            }
            return origin;
        }

        /// <summary>
        /// 运行setx命令设定环境变量
        /// </summary>
        /// <param name="varName">设定变量名</param>
        /// <param name="value">变量值</param>
        /// <param name="isSysVar">是否是系统变量</param>
        public static void RunSetx(string varName, string value, bool isSysVar)
        {
            /*
            new Thread(() =>
            {
                List<string> args = new List<string>();
                if (isSysVar)
                {
                    args.Add("/m");
                }
                args.Add(varName);
                value = RemovePathEndBackslash(value);
                args.Add(value);
                RunCommand("setx", args.ToArray());
            }).Start();
            */

            List<string> args = new List<string>();
            if (isSysVar)
            {
                args.Add("/m");
            }
            args.Add(varName);
           //  value = RemovePathEndBackslash(value);
            args.Add(value);
            RunCommand("setx", args.ToArray());
        }

        /// <summary>
        /// 使用双引号包围字符串
        /// </summary>
        /// <param name="origin">原字符串</param>
        /// <returns>被双引号包围的字符串</returns>
        public static string SurroundByDoubleQuotes(string origin)
        {
            return "\"" + origin + "\"";
        }

        /// <summary>
        /// 调用命令行并获取执行结果，该方法为同步方法，会堵塞线程
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="args">参数数组。例如命令为7z a -t7z -mx9 a.7z dir，那么args的值应当是：{ "a", "-t7z", "-mx9", "a.7z", "dir" }</param>
        /// <returns>命令输出结果，为string数组，数组的第一个为标准输出流，第二个为标准错误流</returns>
        public static string[] RunCommand(string command, string[] args)
        {
            string[] result = new string[2];
            string output = "";
            string err = "";
            string totalArgs = "";
            foreach (string eachArg in args)
            {
                string trimArg = eachArg.Trim();
                if (!trimArg.Contains(" "))
                {
                    totalArgs = totalArgs + trimArg;
                }
                else
                {
                    totalArgs = totalArgs + SurroundByDoubleQuotes(trimArg);
                }
                totalArgs = totalArgs + " ";
            }
            Process process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = totalArgs.Trim();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            try
            {
                process.Start();
                string outLine = null;
                string errLine = null;
                while ((outLine = process.StandardOutput.ReadLine()) != null || (errLine = process.StandardError.ReadLine()) != null)
                {
                    if (outLine != null)
                    {
                        output = output + outLine + "\r\n";
                    }
                    if (errLine != null)
                    {
                        err = err + errLine + "\r\n";
                    }
                }
                process.WaitForExit();
            }
            catch (Exception)
            {
                //none
            }
            finally
            {
                process.Close();
            }
            result[0] = output;
            result[1] = err;
            return result;
        }

        private static void UpdateEnvPath()
        {
            // SEE: https://support.microsoft.com/en-us/help/104011/how-to-propagate-environment-variables-to-the-system
            // Need to send WM_SETTINGCHANGE Message to 
            //    propagage changes to Path env from registry

            // Update desktop icons
            // SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
            // SendNotifyMessage(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment");


            // Update environment variables
            // SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, null, SMTO_ABORTIFHUNG, 5000, IntPtr.Zero);
            // Update taskbar
            // SendNotifyMessage(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "TraySettings");
            
            
            /*IntPtr HWND_BROADCAST = (IntPtr)0xffff;
            const UInt32 WM_SETTINGCHANGE = 0x001A;
            var sendNotifyMessage = SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
            if (sendNotifyMessage)
            {
                MessageBox.Show("sendNotifyMessage suc ");
            }
            

            sendNotifyMessage = SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "TraySettings");
            if (sendNotifyMessage)
            {
                MessageBox.Show("sendNotifyMessage suc2");
            }

            sendNotifyMessage = PostMessage(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero);
            if (sendNotifyMessage)
            {
                MessageBox.Show("sendNotifyMessage suc3");
            }

            sendNotifyMessage = RefreshPolicyEx(
                true, RefreshOptions.RP_FORCE);
            if (sendNotifyMessage)
            {
                MessageBox.Show("sendNotifyMessage suc4");
            }

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            SHChangeNotify(0x08000000, 0x0000, (IntPtr)null, (IntPtr)null);

            // var sendNotifyMessage = true;
            if (!sendNotifyMessage)
            {
                UIntPtr result;
                IntPtr settingResult
                    = SendMessageTimeout((IntPtr)HWND_BROADCAST,
                        WM_SETTINGCHANGE, (UIntPtr)0,
                        "Environment",
                        SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                        5000, out result);

                if (settingResult == IntPtr.Zero)
                {
                    SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                }
            }*/

            // Environment.SetEnvironmentVariable("count", "1", EnvironmentVariableTarget.Machine);
            // Process.Start(@"C:\Windows\System32\cmd.exe"   + " /c setx /M PATH " + @"%PATH%;1");

            /*PowerShell.Create().AddCommand("setx")
                               .AddParameter("JAVA_HOME", selectedPath)
                               .AddParameter("/M")
                              .Invoke();*/
        }


        private string REG_ENV_PATH = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
        private string QTTabBar = @"Software\QTTabBar";

        ContextMenuStrip PopupMenu = new ContextMenuStrip();



        public SetHomeForm(string[] args)
        {
            this.args = args;
            InitializeComponent();
            //Assembly.GetExecutingAssembly().get
            this.Text = this.Text + Application.ProductVersion;
        }

        private void SetHomeForm_Load(object sender, EventArgs e)
        {
            PopupMenu.BackColor = Color.OrangeRed;

            PopupMenu.ForeColor = Color.Black;

            PopupMenu.Text = "File Menu";

            PopupMenu.Font = new Font("Georgia", 16);

            this.ContextMenuStrip = PopupMenu;

            PopupMenu.Show();

            if (args.Length > 0)
            {
                this.curTextBox.Text = args[0];
            }
            else
            {
                // 获取当前 工作目录
                this.curTextBox.Text = System.Environment.CurrentDirectory;
            }
            // 如果是自动配置

            using (var envKey = Registry.CurrentUser.OpenSubKey(QTTabBar, true))
            {
                if (envKey != null) {
                    var flag = envKey.GetValue("AutoSetHome");
                    if (null != flag && flag.Equals("true"))
                    {
                        this.autoBox.Checked = true;
                    }
                    else
                    {
                        this.autoBox.Checked = false;
                    }
                }
                
                else {
                    this.autoBox.Checked = false;
                }
            }

            if (this.autoBox.Checked) { 
                // guess current dir
                string selectedPath = this.curTextBox.Text.Trim();
                if (Directory.Exists(selectedPath)) {
                    string binPath = Path.Combine(selectedPath, "bin");
                    if (Directory.Exists(binPath))
                    {
                        string javaPath = Path.Combine(binPath, "java.exe");
                        string mvnCmd = Path.Combine(binPath, "mvn.cmd");
                        string mvnBat = Path.Combine(binPath, "mvn.bat");
                        string mvndexe = Path.Combine(binPath, "mvnd.exe");
                        string antCmd = Path.Combine(binPath, "ant.cmd");
                        string gradleBat = Path.Combine(binPath, "gradle.bat");
                        string runserverCmd = Path.Combine(binPath, "runserver.cmd");

                        bool isRun = false;
                        if (File.Exists(javaPath))
                        {
                            java_Click(null, null);
                            isRun = true;
                        }
                        else if (File.Exists(mvnCmd))
                        {
                            mvn_Click(null, null);
                            isRun = true;
                        }
                        else if (File.Exists(mvnBat))
                        {
                            mvn_Click(null, null);
                            isRun = true;
                        }
                        else if (File.Exists(mvndexe))
                        {
                            mvnd_Click(null, null);
                            isRun = true;
                        }
                        else if (File.Exists(antCmd))
                        {
                            ant_Click(null, null);
                            isRun = true;
                        }
                        else if (File.Exists(gradleBat))
                        {
                            gradle_Click(null, null);
                            isRun = true;
                        }
                        else if (File.Exists(runserverCmd))  // 设置 rocketmq_home
                        {
                            rocketmq_click(null, null);
                            isRun = true;
                        }

                        if (isRun) {
                            // Dispose();  // 这里导致插件执行报错
                            Application.Exit();
                        }
                        
                    }
                }
            }
        }



        private string filterEmpty(string ignoreFileName)
        {
            HashSet<string> hs = new HashSet<string>();
            string oldpath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            // Environment.GetEnvironmentVariable("PATH");
            string[] sArray = oldpath.Split(';');
            if (sArray.Length == 0)
            {
                return "";
            }
            foreach (string stmp in sArray)
            {

                if (!string.IsNullOrEmpty(stmp.Trim()) && !File.Exists(Path.Combine(stmp, ignoreFileName)))
                {
                    hs.Add(stmp);
                }
            }
            string[] strs = hs.ToArray();
            oldpath = String.Join(";", strs);
            return oldpath;
        }


        private static string joinDevPath(string oldPath)
        {
            /*
            // 处理一些 oldPath 为空的情况
            HashSet<string> hs = new HashSet<string>();
            oldPath = oldPath.Trim();
            string[] sArray = oldPath.Split(';');
            if (sArray.Length > 0)
            {
                bool existSystem32 = false;
                string str = @"C:\Windows\System32";
                foreach (string stmp in sArray)
                {
                    if (!existSystem32) {
                        // Dir exist
                        bool flag = str.Equals(stmp, StringComparison.OrdinalIgnoreCase);
                        if (flag)
                        {
                            existSystem32 = true;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(stmp.Trim()) && Directory.Exists(stmp))
                    {
                        hs.Add(stmp);
                    }
                }

                if (!existSystem32)
                {
                    hs.Add(str);
                }
                // C:\Windows\System32
                string[] strs = hs.ToArray();
                oldPath = String.Join(";", strs);
                if (!oldPath.EndsWith(";")) {
                    oldPath += ";";
                }
            }

            if (String.IsNullOrEmpty(oldPath.Trim()))
            {
                oldPath = @"C:\Windows\System32;";
            }

            if (oldPath.Trim().Equals(";") || IsSemicolon(oldPath))
            {
                oldPath = @"C:\Windows\System32;";
            }
            */

            return oldPath
                    + @"%JAVA_HOME%\bin;"
                    + @"%M2_HOME%\bin;"
                    + @"%MVND_HOME%\bin;"
                    + @"%ANT_HOME%\bin;"
                    + @"%GRADLE_HOME%\bin;"
                    ;
        }

        private static bool IsSemicolon(string str) {
            var strTrim = str.Trim();
            var chars = strTrim.ToCharArray();
            bool allSemicolons = chars.All(c => c == ';');
            return allSemicolons;
        }

        private static string getOldPath(RegistryKey envKey)
        {
            object value = envKey.GetValue("PATH", "", RegistryValueOptions.DoNotExpandEnvironmentNames);
            string oldPath = value.ToString();
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            var m2Home = Environment.GetEnvironmentVariable("M2_HOME");
            var antHome = Environment.GetEnvironmentVariable("ANT_HOME");
            var mvndHome = Environment.GetEnvironmentVariable("MVND_HOME");
            var gradleHome = Environment.GetEnvironmentVariable("GRADLE_HOME");
           // MessageBox.Show(javaHome + @"\bin;");

            if (null != javaHome) 
                oldPath = oldPath.Replace(javaHome + @"\bin;", "");
            if (null != m2Home) 
                oldPath = oldPath.Replace(m2Home + @"\bin;", "");
            if (null != antHome) 
                oldPath = oldPath.Replace(antHome + @"\bin;", "");
            if (null != mvndHome) 
                oldPath = oldPath.Replace(mvndHome + @"\bin;", "");
            if (null != gradleHome) 
                oldPath = oldPath.Replace(gradleHome + @"\bin;", "");

            if (null != javaHome) 
                oldPath = oldPath.Replace(javaHome + @"\bin", "");
            if (null != m2Home) 
                oldPath = oldPath.Replace(m2Home + @"\bin", "");
            if (null != antHome) 
                oldPath = oldPath.Replace(antHome + @"\bin", "");
            if (null != mvndHome) 
                oldPath = oldPath.Replace(mvndHome + @"\bin", "");
            if (null != gradleHome) 
                oldPath = oldPath.Replace(gradleHome + @"\bin", "");

            oldPath = oldPath.Replace(@"%JAVA_HOME%\bin;", "");
            oldPath = oldPath.Replace(@"%M2_HOME%\bin;", "");
            oldPath = oldPath.Replace(@"%ANT_HOME%\bin;", "");
            oldPath = oldPath.Replace(@"%MVND_HOME%\bin;", "");
            oldPath = oldPath.Replace(@"%GRADLE_HOME%\bin;", "");
            
            oldPath = oldPath.Replace(@"%JAVA_HOME%\bin", "");
            oldPath = oldPath.Replace(@"%M2_HOME%\bin", "");
            oldPath = oldPath.Replace(@"%ANT_HOME%\bin", "");
            oldPath = oldPath.Replace(@"%MVND_HOME%\bin", "");
            oldPath = oldPath.Replace(@"%GRADLE_HOME%\bin", "");


            if (!oldPath.EndsWith(";"))
            {
                oldPath = oldPath + ";";
            }

            // 忽略大小写  
            if (
                // !oldPath.Contains(@"C:\Windows\System32") &&
                // !oldPath.Contains(@"%SystemRoot%\system32")  
                !(oldPath.IndexOf(@"C:\Windows\System32", StringComparison.OrdinalIgnoreCase) >= 0) &&
                !(oldPath.IndexOf(@"%SystemRoot%\system32", StringComparison.OrdinalIgnoreCase) >= 0)
                ) {
                oldPath = oldPath + @"C:\Windows\System32;"; 
            }
            return oldPath;
        }

        private void java_Click(object sender, EventArgs e)
        {
            // 3. 设置当前目录JAVA_HOME
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string libPath = Path.Combine(selectedPath, "lib");
            string javaPath = Path.Combine(binPath, "java.exe");
            string toolsJar = Path.Combine(libPath, "tools.jar");
            string dtJar = Path.Combine(libPath, "dt.jar");


            if (String.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("当前目录已经删除");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
            {
                MessageBox.Show("bin目录不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            {
                MessageBox.Show("Java不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(libPath) || !Directory.Exists(libPath))
            {
                MessageBox.Show("lib目录不存在");
                SystemSounds.Hand.Play();
                return;
            }

            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                oldPath = kill(oldPath, "java.exe");
                envKey.SetValue("JAVA_HOME", selectedPath);
                var devPath = joinDevPath(oldPath);
                // envKey.SetValue("PATH", devPath);
                RunSetx("Path", devPath, true);
                if (File.Exists(toolsJar) && File.Exists(dtJar))
                {
                    envKey.SetValue("CLASSPATH", @".;%JAVA_HOME%\lib\tools.jar;%JAVA_HOME%\lib\dt.jar;");
                }

                // setx "Path" "%Path%;%JAVA_HOME%\bin" /m
                // Environment.SetEnvironmentVariable("JAVA_HOME", selectedPath, EnvironmentVariableTarget.Machine);
                // Environment.SetEnvironmentVariable("PATH", devPath + ";9", EnvironmentVariableTarget.Machine);
                UpdateEnvPath();
                MessageBox.Show("设置JAVA_HOME成功");
            }
        }

        private string kill(string oldPath, string fileName)
        {
            var paths = oldPath.Split(';');
            if (paths != null && paths.Length > 0)
            {
                for (var i = paths.Length - 1; i >= 0; i--)
                {
                    var tempPath = paths[i];
                    if (!string.IsNullOrEmpty(tempPath))
                    {
                        // 如果这个路径不存在则过滤掉
                        if (!Directory.Exists(tempPath))
                        {
                            oldPath = oldPath.Replace(tempPath, "");
                            continue;
                        }

                        // 判断文件是否存在，存在的话 kill 掉
                        var combine = Path.Combine(tempPath, fileName );
                        if (File.Exists(combine))
                        {
                            oldPath = oldPath.Replace(tempPath, "");
                        }

                        
                    }
                }
            }
            // 如果不为空的话， 则判断结尾是否包含多个分号 ;
            if (!string.IsNullOrEmpty(oldPath))
            {
                /*if (oldPath.EndsWith(";;"))
                {
                    oldPath = oldPath.Replace(";;", ";");
                }

                if (oldPath.EndsWith(";;;"))
                {
                    oldPath = oldPath.Replace(";;;", ";");
                }*/
                // 正则替换掉，2个或者以上; 则替换成一个;
                string pattern = @";+;+";
                string replacement = ";";
                oldPath = Regex.Replace(oldPath, pattern, replacement);
            }

            return oldPath;
        }

        private void mvn_Click(object sender, EventArgs e)
        {
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string mvnCmd = Path.Combine(binPath, "mvn.cmd");
            string mvnBat = Path.Combine(binPath, "mvn.bat");

            if (String.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("当前目录已经删除");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
            {
                MessageBox.Show("bin目录不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (
                (String.IsNullOrEmpty(mvnCmd) || !File.Exists(mvnCmd)) 
                &&
                (String.IsNullOrEmpty(mvnBat) || !File.Exists(mvnBat))
                )
            {
                MessageBox.Show("mvn.cmd或mvn.bat不存在");
                SystemSounds.Hand.Play();
                return;
            }


            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                if (File.Exists(mvnCmd))
                {
                    oldPath = kill(oldPath, "mvn.cmd");
                }
                else if (File.Exists(mvnBat))
                {
                    oldPath = kill(oldPath, "mvn.bat");
                }

                envKey.SetValue("M2_HOME", selectedPath);
                // envKey.SetValue("PATH", joinDevPath(oldPath));
                RunSetx("Path", joinDevPath(oldPath), true);
                
                
                MessageBox.Show("设置M2_HOME成功");
                UpdateEnvPath();
            }
        }

        private void mvnd_Click(object sender, EventArgs e)
        {
            // 10. 设置当前目录MVND_HOME
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string mvndexe = Path.Combine(binPath, "mvnd.exe");

            if (String.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("当前目录已经删除");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
            {
                MessageBox.Show("bin目录不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(mvndexe) || !File.Exists(mvndexe))
            {
                MessageBox.Show("mvndexe不存在");
                SystemSounds.Hand.Play();
                return;
            }


            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                oldPath = kill(oldPath, "mvnd.exe");
                envKey.SetValue("MVND_HOME", selectedPath);
                // envKey.SetValue("PATH", joinDevPath(oldPath));
                RunSetx("Path", joinDevPath(oldPath), true);
                
                MessageBox.Show("设置MVND_HOME成功");
                UpdateEnvPath();
            }
        }

        private void ant_Click(object sender, EventArgs e)
        {
            // 设置当前目录ANT_HOME
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string antCmd = Path.Combine(binPath, "ant.cmd");

            if (String.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("当前目录已经删除");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
            {
                MessageBox.Show("bin目录不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(antCmd) || !File.Exists(antCmd))
            {
                MessageBox.Show("antCmd不存在");
                SystemSounds.Hand.Play();
                return;
            }


            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                oldPath = kill(oldPath, "ant.cmd");
                envKey.SetValue("ANT_HOME", selectedPath);
                // envKey.SetValue("PATH", joinDevPath(oldPath));
                RunSetx("Path", joinDevPath(oldPath), true);
                
                MessageBox.Show("设置ANT_HOME成功");
                UpdateEnvPath();
            }
        }

        private void gradle_Click(object sender, EventArgs e)
        {
            // 设置当前目录ANT_HOME
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string gradleBat = Path.Combine(binPath, "gradle.bat");

            if (String.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("当前目录已经删除");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
            {
                MessageBox.Show("bin目录不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(gradleBat) || !File.Exists(gradleBat))
            {
                MessageBox.Show("gradleBat不存在");
                SystemSounds.Hand.Play();
                return;
            }


            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                oldPath = kill(oldPath, "gradle.bat");
                envKey.SetValue("GRADLE_HOME", selectedPath);
                // envKey.SetValue("PATH", joinDevPath(oldPath));
                RunSetx("Path", joinDevPath(oldPath), true);
                MessageBox.Show("设置GRADLE_HOME成功");
                UpdateEnvPath();
            }
        }

        private void autoBox_CheckedChanged(object sender, EventArgs e)
        {
            using (var envKey = Registry.CurrentUser.OpenSubKey(QTTabBar, true))
            {
                if (autoBox.Checked) {
                    envKey.SetValue("AutoSetHome", "true");
                }
                else {
                    envKey.SetValue("AutoSetHome", "true");
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var pathRegAsm = @"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\RegAsm.exe";
            var exists = File.Exists(pathRegAsm);
            if (!exists)
            {
                pathRegAsm = @"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\RegAsm.exe";
            }
            exists = File.Exists(pathRegAsm);
            if (exists)
            {
                List<string> args = new List<string>();
                args.Add("/m");
                // RunCommand("setx", args.ToArray());
            }

            /*using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Toolbar\\ShellBrowser", false))
            {
                using (RegistryKey subKey = Registry.CurrentUser.CreateSubKey("Software\\QTTabBar\\Volatile"))
                {
                    if (registryKey.GetValue("ITBar7Layout") is byte[] numArray)
                    {
                        if (numArray.Length != 0)
                        {
                            subKey.SetValue("ITBar7Layout", (object) numArray, RegistryValueKind.Binary);
                            int num = (int) MessageBox.Show("Success.");
                            this.Close();
                            return;
                        }
                    }
                }
            }
            int num1 = (int) MessageBox.Show("Failed.");
            this.Close();*/
        }


        private static void RestartExplorer()
        {
            foreach (Process process in Process.GetProcessesByName("explorer"))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                }
            }
            Thread.Sleep(1500);
            if (Process.GetProcessesByName("explorer").Length != 0)
                return;
            Process.Start("explorer.exe");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                string sCPUSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get())
                {
                    
                    sCPUSerialNumber = mo["Name"].ToString().Trim();//操作系统名字
                    //sCPUSerialNumber = mo["BootDevice"].ToString().Trim();//系统启动分区
                    //sCPUSerialNumber = mo["NumberOfProcesses"].ToString().Trim();//当前运行的进程数
                    //sCPUSerialNumber = mo["SerialNumber"].ToString().Trim();//操作系统序列号
                    //sCPUSerialNumber = mo["OSLanguage"].ToString().Trim();//操作系统的语言
                    //sCPUSerialNumber = mo["Manufacturer"].ToString().Trim();//
                }
                // MessageBox.Show(sCPUSerialNumber.Substring(10, 10));//分割字符串
                MessageBox.Show(sCPUSerialNumber);//分割字符串
            }
            catch (Exception )
            {
            }
            
            var osVersionVersionString = Environment.OSVersion.VersionString;
            // MessageBox.Show(osVersionVersionString);
            // MessageBox.Show(Environment.OSVersion.Platform.ToString());
            MessageBox.Show("" + Environment.OSVersion.Version.Major);
            MessageBox.Show("" + Environment.OSVersion.Version.Minor);
            MessageBox.Show("" + Environment.OSVersion.Version.Build);
        }

        private void rocketmq_click(object sender, EventArgs e)
        {
            // ROCKETMQ_HOME

            // 设置当前目录ANT_HOME
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string runserverCmd = Path.Combine(binPath, "runserver.cmd");

            if (String.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("当前目录已经删除");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
            {
                MessageBox.Show("bin目录不存在");
                SystemSounds.Hand.Play();
                return;
            }


            if (String.IsNullOrEmpty(runserverCmd) || !File.Exists(runserverCmd))
            {
                MessageBox.Show("runserverCmd不存在");
                SystemSounds.Hand.Play();
                return;
            }


            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                // oldPath = kill(oldPath, "gradle.bat");
                envKey.SetValue("ROCKETMQ_HOME", selectedPath);
                // envKey.SetValue("PATH", joinDevPath(oldPath));
                RunSetx("ROCKETMQ_HOME_INT", "1", true);
                MessageBox.Show("设置ROCKETMQ_HOME成功");
                UpdateEnvPath();
            }
        }
    }
}
