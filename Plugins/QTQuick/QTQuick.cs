//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
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
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using QTPlugin;
using QTPlugin.Interop;
using QTTabBarLib;
using System.Diagnostics;

namespace Qwop {
    /// <summary>
    /// Plugin sample.
    /// 
    /// 	PluginAttribute
    ///			PluginType.Interactive      Indicates that the plugin has toolbar item and instantialized only when toolbar item is enabled.
    ///										It needs to inherit IBarButton or IBarCustomItem.
    ///										
    ///			PluginType.Background		Indicates that the plugin is instantialized even if toolbar item is not enabled.
    ///										This type plugin can have no toolbar item.
    ///		
    ///			Author, Name, Version, and Description are used in Options -> Plugins tab.
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Qwop", Name = "快捷", Version = "0.0.0.1", Description = "打开QT选项")]
    public class QTQuickButton : IBarDropButton
    {
        static readonly bool IsWin7 = Environment.OSVersion.Version >= new Version(6, 1);
        static readonly bool IsXP = Environment.OSVersion.Version.Major <= 5;

        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;

        private bool fFirstMenuDropDown = true;
        private string text = "快捷";
        private List<Address> lstSelectedItems = new List<Address>();


        public static void Uninstall() {
            // add codes here to delete saved settings, files, or registry keys if any.
            // see "Uninstallation" in Instructions.txt

            MessageBox.Show("uninstallation");
        }



        #region IPluginClient Members


        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            // called when this plugin class instantialized.

            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;


            // attached events are automatically detached when plugin closes.
            this.pluginServer.TabChanged += pluginServer_TabChanged;
            this.pluginServer.TabAdded += pluginServer_TabAdded;
            this.pluginServer.TabRemoved += pluginServer_TabRemoved;
            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
            this.pluginServer.SettingsChanged += pluginServer_SettingsChanged;
            this.pluginServer.SelectionChanged += pluginServer_SelectionChanged;


      // Do not registering to the qttabbar menu.
         // registering to QTTabBar menu.
   //         this.pluginServer.RegisterMenu(this, MenuType.Both, "SampleSplitButton Menu test", true);

        }

        public void Close(EndCode code) {
            // If endCode is NOT EndCode.Hidden, QTTabBar loses the reference to this plugin instance after this call.
            // Clean up managed/unmanaged resources here if any.
            // 
            // EndCode.Hidden is passed only when this has PluginType.Background attribute and toolbar item is disabled by user.
            // In this case, the plugin is still alive and can interact with user. 
            // Do not clean up resources.
        }

        public bool QueryShortcutKeys(out string[] actions) {
            // to expose shortcut keys, 
            // set action names of key funcion.

            actions = new string[] { "Test MessageBox", "Test Beep" };
            return true;

            // or,

            //actions = null;
            //return false;
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
            // user clicked registered menu.

            if(menuText == "SampleSplitButton Menu test") {
                if(menuType == MenuType.Tab) {
                    MessageBox.Show(tab.Address.Path);
                }
                else if(menuType == MenuType.Bar) {
                    pluginServer.ExecuteCommand(Commands.OpenTabBarOptionDialog, null);
                }
            }
        }

        public bool HasOption {
            get {
                return true;
            }
        }

        public void OnOption() {
            // plugin option button is pressed.

            MessageBox.Show("Option of SampleSplitButton");
        }

        public void OnShortcutKeyPressed(int iIndex) {
            // user pressed registered shortcut key.

            switch(iIndex) {
                case 0:
                    MessageBox.Show("Key shortcut pressed");
                    break;

                case 1:
                    SystemSounds.Beep.Play();
                    break;
            }
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
            // callled every time the interactive item is about to be added to the ToolBar.

            fFirstMenuDropDown = true;
        }

        public Image GetImage(bool fLarge) {
            // QTTabBar gets button image
            
            //   return fLarge ? Resource : Resource.SampleSplitButton_small;
            return fLarge ? Resource.Config_24 : Resource.Config_16;         
        }

        public void OnButtonClick() {
            // user clicked the plugin button.

            /*Address[] addresses;
            if(pluginServer.TryGetSelection(out addresses)) {
                int c = addresses.Length;

                string str = c + " items\r\n\r\n";
                for(int i = 0; i < c; i++) {
                    str += addresses[i].Path + "\r\n";
                }

                MessageBox.Show(str);

                lstSelectedItems.Clear();
                lstSelectedItems.AddRange(addresses);
            }*/

            QTTabBarClass.OpenOptionDialog();
        }

        public string Text {
            get {
                // text for button label, button tooltip, buttonbar option.

                return text;
            }
        }

        public bool ShowTextLabel {
            get {
                return false;
            }
        }

        #endregion


        #region IBarDropButton Members

        public bool IsSplitButton {
            get {
                return true;
            }
        }


        private ToolStripDropDownMenu menu;

        public void OnDropDownOpening(ToolStripDropDownMenu menu) {
            // Called the dropdown menu is about to open.
            // No need to call "menu.SuspendLayout" or "menu.ResumeLayout".
            this.menu = menu;

            if(fFirstMenuDropDown) {
                menu.Items.Add(new ToolStripMenuItem("我的文档"));
                menu.Items.Add(new ToolStripMenuItem("控制面板\\所有控制面板项\\系统"));
                menu.Items.Add(new ToolStripMenuItem("控制面板\\所有控制面板项\\个性化"));
                // menu.Items.Add(new ToolStripMenuItem("Test selection"));

                fFirstMenuDropDown = false;

            }
           
        }

      
        public void OnDropDownItemClick(ToolStripItem item, MouseButtons mouseButton) {
            // user clicked the dropdown menu item of this plugin button dropdown.

            int idx = menu.Items.IndexOf(item);
            string path = null;
            if (mouseButton == MouseButtons.Left)
            {
                switch (idx)
                {
                    case 0: {
                            // 0. 我的文档
                            path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            break;
                    }
                    case 1 :{
                            // 1. 系统
                            if ( IsWin7 ) {
                                path = "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{BB06C0E4-D293-4F75-8A90-CB05B6477EEE}";
                                string spa_exe = Environment.GetEnvironmentVariable("systemroot") + "\\System32\\SystemPropertiesAdvanced.exe";
                                Process.Start(spa_exe);
                                return;
                            }
                            else if (IsXP) { }
                                //path = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\::{21EC2020-3AEA-1069-A2DD-08002B30309D}";
                            break;
                    }
                    case 2 :{
                            // 2. 显示
                            if ( IsWin7 )
                                path = "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{ED834ED6-4B5A-4BFE-8F11-A626DCB6A921}";
                            else if ( IsXP )
                                path = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\::{21EC2020-3AEA-1069-A2DD-08002B30309D}";
                            break;
                    }

                        
                }
                
                // pluginServer.CreateTab(new Address(mydocument), -1, false, true);
                if (null != path)
                pluginServer.CreateTab(new Address(path), -1, false, false);

                
            }
            else if (mouseButton == MouseButtons.Right)
            {
                //SystemSounds.Asterisk.Play();

                 // MessageBox.Show("Hello World");


                switch (idx)
                {
                    case 0:
                        {
                            // 0. 我的文档
                            path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            break;
                        }
                    case 1:
                        {
                            // 1. 系统
                            if (IsWin7)
                            {
                                path = "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{BB06C0E4-D293-4F75-8A90-CB05B6477EEE}";
                            }
                            else if (IsXP) { }
                            //path = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\::{21EC2020-3AEA-1069-A2DD-08002B30309D}";
                            break;
                        }
                    case 2:
                        {
                            // 2. 显示
                            if (IsWin7)
                                path = "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{ED834ED6-4B5A-4BFE-8F11-A626DCB6A921}";
                            else if (IsXP)
                                path = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\::{21EC2020-3AEA-1069-A2DD-08002B30309D}";
                            break;
                        }


                }

                if (null != path)
                    pluginServer.CreateTab(new Address(path), -1, false, false);
            }
           
          /*  if(item.Text == "Open folder") {
                if(mouseButton == MouseButtons.Left) {
                    string mydocument = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    pluginServer.CreateTab(new Address(mydocument), -1, false, true);
                }
                else if(mouseButton == MouseButtons.Right) {
                    SystemSounds.Asterisk.Play();
                }
            }
            else if(item.Text == "Test selection") {
                if(lstSelectedItems.Count > 0)
                    pluginServer.TrySetSelection(lstSelectedItems.ToArray(), false);
            }*/
        }

        #endregion


        #region Event handlers

        private void pluginServer_SettingsChanged(object sender, PluginEventArgs e) {
        }

        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
        }

        private void pluginServer_TabChanged(object sender, PluginEventArgs e) {
        }

        private void pluginServer_TabAdded(object sender, PluginEventArgs e) {
        }

        private void pluginServer_TabRemoved(object sender, PluginEventArgs e) {
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
        }

        #endregion



    }
}