//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010-2022  Quizo, Paul Accisano, indiff
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
using System.Runtime.InteropServices;
using System.Text;
using QTPlugin;
using QTPlugin.Interop;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.Win32;

namespace QuizoPlugins {
    // [Plugin(PluginType.Background, Author = "Quizo", Name = "Show StatusBar", Version = "0.9.0.0", Description = "ShowStatusBar")]
    [Plugin(PluginType.Background, Author = "indiff", Name = "鼠标悬浮激活标签", Version = "1.0.0.1", Description = "鼠标悬浮激活标签;调整悬停5秒生效")]
    public class ActivateByMouseHover : IPluginClient
    {
        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;

        private System.Windows.Forms.Timer timer;

        private ITab previousTab;
        private static int mouseHoverTime = 5000;

        private const string REGNAME = "ActivateByMouseHover";
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, StringBuilder lpszClass, string lpszWindow);

        private static bool fVista = IsVista();
        private const int WM_COMMAND = 0x0111;

        private static bool IsVista() {
            return Environment.OSVersion.Version.Major > 5;
        }

        static ActivateByMouseHover()
		{
			ReadSetting();
		}


        #region IPluginClient members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;
            this.pluginServer.PointedTabChanged += new PluginEventHandler(pluginServer_PointedTabChanged);

        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode endCode) {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        public bool HasOption {
            get {
                return true;
            }
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public void OnOption() {
            using (var sf = new SettingForm(mouseHoverTime))
            {
                if (DialogResult.OK == sf.ShowDialog())
                {
                    mouseHoverTime = sf.Value;
                    SaveSetting();
                    if (this.timer != null)
                    {
                        this.timer.Interval = mouseHoverTime;
                    }
                }
            }
        }

        public void OnShortcutKeyPressed(int index) {
           
        }

        
		private void pluginServer_PointedTabChanged( object sender, PluginEventArgs e )
		{
			if( this.timer == null )
			{
				this.timer = new Timer();
				this.timer.Interval = mouseHoverTime;
				this.timer.Tick += new EventHandler( timer_Tick );
			}

			this.timer.Enabled = false;

			//var tab = this.pluginServer.HitTest( Control.MousePosition );
            // this.pluginServer.GetTabs();
			var tabs = this.pluginServer.GetTabs();
			if( -1 < e.Index && e.Index < tabs.Length )
			{
				this.previousTab = tabs[e.Index];
				this.timer.Enabled = true;
			}
			else
			{
				this.previousTab = null;
			}
		}

		private void timer_Tick( object sender, EventArgs e )
		{
			try
			{
				if( this.previousTab != null )
				{
					this.previousTab.Selected = true;
				}
				this.timer.Enabled = false;
			}
			catch
			{
			}
		}

		private static void ReadSetting()
		{
			using( var rkPlugin = Registry.CurrentUser.CreateSubKey( CONSTANTS.REGISTRY_PLUGINSETTINGS + @"\Quizo\" + REGNAME ) )
			{
				if( rkPlugin != null )
				{
					var obj  =  rkPlugin.GetValue( "MouseHoverTime", 5000 );
					if( obj is int )
					{
						mouseHoverTime = (int)obj;
					}
				}
			}
		}

		private static void SaveSetting()
		{
			using( var rkPlugin = Registry.CurrentUser.CreateSubKey( CONSTANTS.REGISTRY_PLUGINSETTINGS + @"\" + REGNAME ) )
			{
				if( rkPlugin != null )
				{
					rkPlugin.SetValue( "MouseHoverTime", mouseHoverTime );
				}
			}
		}

	}

	sealed class Localizer : LocalizedStringProvider
	{
        private bool fJa; 
        private bool fZh;

		public Localizer()
		{
			fJa = CultureInfo.CurrentCulture.Name == "ja-JP";
            fZh = CultureInfo.CurrentCulture.Parent.Name == "zh-CHS";
		}

		public override string Name
		{
			get
			{
                if (fZh) return "鼠标悬浮激活";
				return fJa ? "マウスホバーでタブを選択" : "Activate By MouseHover";
			}
		}

		public override string Author
		{
			get
			{
                if (fZh) return "indiff";
				return "indiff";
			}
		}

		public override string Description
		{
			get
			{
                if (fZh) return "鼠标悬浮激活可以设置一个延时时间.";
				return fJa ? "マウスカーソルをタブの上に置くだけで、そのタブを選択できるようになります。ウェイト時間設定可。" : "Activate a tab by mouse-hover. To set delay time, press Option. ";
			}
		}

        /*
		public override DateTime LastUpdate
		{
			get
			{
				return new DateTime( 2021, 04, 28 );
			}
		}

		public override string SupportURL
		{
			get
			{
				return "https://github.com/indiff/QTTabbar";
			}
		}*/

		public override void SetKey( int iKey )
		{
		}



        #endregion

    }
}
