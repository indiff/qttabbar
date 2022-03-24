using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;

namespace SetHome
{
    public partial class SetHomeForm : Form
    {
        private string[] args;


        const int HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001a;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg,
            UIntPtr wParam, string lParam);
        private string REG_ENV_PATH = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
        private string QTTabBar = @"Software\QTTabBar";
        
        public SetHomeForm(string[] args)
        {
            this.args = args;
            InitializeComponent();
            //Assembly.GetExecutingAssembly().get
            this.Text = this.Text + Application.ProductVersion;
        }

        private void SetHomeForm_Load(object sender, EventArgs e)
        {
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
                        string mvndexe = Path.Combine(binPath, "mvnd.exe");
                        string antCmd = Path.Combine(binPath, "ant.cmd");
                        string gradleBat = Path.Combine(binPath, "gradle.bat");
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

                        if (isRun) {
                            Dispose();
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
            return oldPath
                    + @"%JAVA_HOME%\bin;"
                    + @"%M2_HOME%\bin;"
                    + @"%MVND_HOME%\bin;"
                    + @"%ANT_HOME%\bin;"
                    + @"%GRADLE_HOME%\bin;"
                    ;
        }

        private static string getOldPath(RegistryKey envKey)
        {
            object value = envKey.GetValue("PATH");
            string oldPath = value.ToString();
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            var m2Home = Environment.GetEnvironmentVariable("M2_HOME");
            var antHome = Environment.GetEnvironmentVariable("ANT_HOME");
            var mvndHome = Environment.GetEnvironmentVariable("MVND_HOME");
            var gradleHome = Environment.GetEnvironmentVariable("GRADLE_HOME");
           // MessageBox.Show(javaHome + @"\bin;");

            oldPath = oldPath.Replace(javaHome + @"\bin;", "");
            oldPath = oldPath.Replace(m2Home + @"\bin;", "");
            oldPath = oldPath.Replace(antHome + @"\bin;", "");
            oldPath = oldPath.Replace(mvndHome + @"\bin;", "");
            oldPath = oldPath.Replace(gradleHome + @"\bin;", "");


            oldPath = oldPath.Replace(javaHome + @"\bin", "");
            oldPath = oldPath.Replace(m2Home + @"\bin", "");
            oldPath = oldPath.Replace(antHome + @"\bin", "");
            oldPath = oldPath.Replace(mvndHome + @"\bin", "");
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
                envKey.SetValue("JAVA_HOME", selectedPath);
                envKey.SetValue("PATH", joinDevPath(oldPath));
                if (File.Exists(toolsJar) && File.Exists(dtJar))
                {
                    envKey.SetValue("CLASSPATH", @".;%JAVA_HOME%\lib\tools.jar;%JAVA_HOME%\lib\dt.jar;");
                }
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                MessageBox.Show("设置JAVA_HOME成功");
            }
        }

        private void mvn_Click(object sender, EventArgs e)
        {
            string selectedPath = this.curTextBox.Text.Trim();
            string binPath = Path.Combine(selectedPath, "bin");
            string mvnCmd = Path.Combine(binPath, "mvn.cmd");


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


            if (String.IsNullOrEmpty(mvnCmd) || !File.Exists(mvnCmd))
            {
                MessageBox.Show("mvnCmd不存在");
                SystemSounds.Hand.Play();
                return;
            }
            using (var envKey = Registry.LocalMachine.OpenSubKey(REG_ENV_PATH, true))
            {
                string oldPath = getOldPath(envKey);
                envKey.SetValue("PATH", joinDevPath(oldPath));

                envKey.SetValue("M2_HOME", selectedPath);

                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                MessageBox.Show("设置M2_HOME成功");
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
                envKey.SetValue("MVND_HOME", selectedPath);
                envKey.SetValue("PATH", joinDevPath(oldPath));
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                MessageBox.Show("设置MVND_HOME成功");
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
                envKey.SetValue("ANT_HOME", selectedPath);
                envKey.SetValue("PATH", joinDevPath(oldPath));
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                MessageBox.Show("设置ANT_HOME成功");
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
                envKey.SetValue("GRADLE_HOME", selectedPath);
                envKey.SetValue("PATH", joinDevPath(oldPath));
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                MessageBox.Show("设置GRADLE_HOME成功");
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
    }
}
