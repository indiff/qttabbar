//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
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
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Image = System.Drawing.Image;
namespace QTTabBarLib {
    internal partial class Options12_Plugins : OptionsDialogTab {
        private ObservableCollection<PluginEntry> CurrentPlugins;

        public Options12_Plugins() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            CurrentPlugins = new ObservableCollection<PluginEntry>();
            foreach(PluginAssembly assembly in PluginManager.PluginAssemblies) {
                CreatePluginEntry(assembly, false);
            }
            lstPluginView.ItemsSource = CurrentPlugins;
        }

        public override void ResetConfig() {
            // Should we do anything here?
        }

        public override void CommitConfig() {
            HashSet<string> paths = new HashSet<string>();
            HashSet<PluginAssembly> toDispose = new HashSet<PluginAssembly>();

            // Don't dispose the assemblies here.  That will be done by the plugin manager
            // when the plugins are unloaded.
            for(int i = 0; i < CurrentPlugins.Count; ++i) {
                if(CurrentPlugins[i].UninstallOnClose) {
                    CurrentPlugins[i].Enabled = false;
                    CurrentPlugins.RemoveAt(i--);
                }
            }

            List<string> enabled = new List<string>();
            foreach(PluginEntry entry in CurrentPlugins) {
                paths.Add(entry.PluginAssembly.Path);
                if(entry.DisableOnClose) {
                    entry.Enabled = false;
                }
                else if(entry.EnableOnClose) {
                    entry.Enabled = true;
                }
                else if(entry.InstallOnClose) {
                    entry.Enabled = true;
                    toDispose.Add(entry.PluginAssembly);
                    // Newly installed PluginAssemblies are loaded by the options dialog.
                    // They will also be loaded by the PluginManager, so we have to 
                    // dispose of the ones we loaded here.
                }
                entry.EnableOnClose = entry.DisableOnClose = entry.InstallOnClose = false;

                if(entry.Enabled) enabled.Add(entry.PluginID);
            }
            WorkingConfig.plugin.Enabled = enabled.ToArray();
            foreach(PluginAssembly asm in toDispose) {
                asm.Dispose();
            }
            PluginManager.SavePluginAssemblyPaths(paths.ToList());
            
            // Entries are invalid now, some assemblies may have been Disposed.
            CurrentPlugins = new ObservableCollection<PluginEntry>();
        }

        private void btnPluginOptions_Click(object sender, RoutedEventArgs e) {
            PluginEntry entry = (PluginEntry)((Button)sender).DataContext;
            string pid = entry.PluginID;
            // Unfortunately, we can't call Plugin.OnOption on plugins that are
            // loaded in a non-static context.
            InstanceManager.InvokeMain(tabbar => {
                Plugin p;
                if(!tabbar.pluginServer.TryGetPlugin(pid, out p) || p.Instance == null) return;
                try {
                    p.Instance.OnOption();
                }
                catch(Exception ex) {
                    PluginManager.HandlePluginException(ex, new WindowInteropHelper(Window.GetWindow(this)).Handle,
                            entry.Name, "Open plugin option.");
                }
            });
        }

        private void btnPluginEnableDisable_Click(object sender, RoutedEventArgs e) {
            PluginEntry entry = (PluginEntry)((Button)sender).DataContext; 
            if(entry.DisableOnClose) {
                entry.DisableOnClose = false;
            }
            else if(entry.EnableOnClose) {
                entry.EnableOnClose = false;
            }
            else if(entry.Enabled) {
                entry.DisableOnClose = true;
            }
            else {
                entry.EnableOnClose = true;
            }
        }

        private void btnPluginRemove_Click(object sender, RoutedEventArgs e) {
            PluginEntry entry = (PluginEntry)((Button)sender).DataContext; 
            PluginAssembly pluginAssembly = entry.PluginAssembly;
            if(pluginAssembly.PluginInformations.Count > 1) {
                string plugins = pluginAssembly.PluginInformations.Select(info => info.Name).StringJoin(", ");
                if(MessageBox.Show(
                        QTUtility.TextResourcesDic["Options_Page12_Plugins"][8] +
                        Environment.NewLine + Environment.NewLine + plugins + Environment.NewLine + Environment.NewLine +
                        QTUtility.TextResourcesDic["Options_Page12_Plugins"][9],
                        QTUtility.TextResourcesDic["OptionsDialog"][3],
                        MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) {
                    return;
                }
            }
            for(int i = 0; i < CurrentPlugins.Count; i++) {
                PluginEntry otherEntry = CurrentPlugins[i];
                if(otherEntry.PluginAssembly == entry.PluginAssembly) {
                    if(otherEntry.InstallOnClose) {
                        CurrentPlugins.RemoveAt(i);
                        --i;
                    }
                    else {
                        otherEntry.UninstallOnClose = true;
                    }
                }
            }
            if(entry.InstallOnClose) {
                entry.PluginAssembly.Dispose();
            }
        }

        private void btnBrowsePlugin_Click(object sender, RoutedEventArgs e) {
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = QTUtility.TextResourcesDic["FileFilters"][2] + "|*.dll";
                ofd.RestoreDirectory = true;
                ofd.Multiselect = true;

                if(System.Windows.Forms.DialogResult.OK != ofd.ShowDialog()) return;
                bool fFirst = true;
                foreach(string path in ofd.FileNames) {
                    PluginAssembly pa = new PluginAssembly(path);
                    if(!pa.PluginInfosExist) continue;
                    CreatePluginEntry(pa, true);
                    if(!fFirst) continue;
                    fFirst = false;
                    lstPluginView.SelectedItem = CurrentPlugins[CurrentPlugins.Count - 1];
                    lstPluginView.ScrollIntoView(lstPluginView.SelectedItem);
                }
            }
        }

        private void txtUndo_MouseUp(object sender, MouseButtonEventArgs e) {
            PluginEntry entry = (PluginEntry)((TextBlock)sender).DataContext;
            if(entry.UninstallOnClose) {
                foreach(var other in CurrentPlugins) {
                    if(entry.PluginAssembly == other.PluginAssembly) {
                        other.UninstallOnClose = false;       
                    }
                }
            }
            else if(entry.InstallOnClose) {
                entry.IsSelected = true;
                btnPluginRemove_Click(sender, null);
            }
            else if(entry.DisableOnClose) {
                entry.DisableOnClose = false;
            }
            else if(entry.EnableOnClose) {
                entry.EnableOnClose = false;
            }
        }

        private void CreatePluginEntry(PluginAssembly pa, bool fAddedByUser) {
            if(!pa.PluginInfosExist || CurrentPlugins.Any(pe => pe.Path == pa.Path)) {
                return;
            }      
            foreach(PluginInformation pi in pa.PluginInformations) {
                if (null != pi && null != pi.Path) {
                    // Check the file exists by qwop ?
                    if (!File.Exists(pi.Path)) {
                        continue;
                    }
                    // Check the file exists by qwop ?
                    PluginEntry entry = new PluginEntry(this, pi, pa) { InstallOnClose = fAddedByUser };
                    int i = 0;
                    while (i < CurrentPlugins.Count && string.Compare(CurrentPlugins[i].Title, entry.Title, true) <= 0) ++i;
                    CurrentPlugins.Insert(i, entry);
                }
            }
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class PluginEntry : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            private Options12_Plugins parent;
            private PluginInformation PluginInfo;
            public PluginAssembly PluginAssembly { get; private set; }

            public Image Icon { get { return PluginInfo.ImageLarge ?? Resources_Image.imgPlugin24; } }
            public string Name { get { return PluginInfo.Name; } }
            public string Title { get {
                return Name + "  " + PluginInfo.Version;
            } }
            public string Author { get { return PluginInfo.Author; } }
            public string Desc { get { return PluginInfo.Description; } }
            public bool IsSelected { get; set; }
            public double IconOpacity { get { return Enabled ? 1.0 : 0.5; } }
            public bool DisableOnClose { get; set; }
            public bool EnableOnClose { get; set; }
            public bool InstallOnClose { get; set; }
            public bool UninstallOnClose { get; set; }
            public bool Enabled { get { return PluginInfo.Enabled; } set { PluginInfo.Enabled = value; } }
            public string PluginID { get { return PluginInfo.PluginID; } }
            public string Path { get { return PluginInfo.Path; } }
            public Visibility StatusVisibility { get {
                return DisableOnClose || EnableOnClose || InstallOnClose || UninstallOnClose
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }}
            public int StatusTextIdx { get {
                if(UninstallOnClose) return 4;
                if(InstallOnClose)   return 5;
                if(EnableOnClose)    return 6;
                if(DisableOnClose)   return 7;
                return int.MaxValue;
            }}
            public Visibility MainBodyVisibility { get {
                return UninstallOnClose ? Visibility.Collapsed : Visibility.Visible;
            }}
            public Color BackgroundColor { get {
                if(StatusVisibility == Visibility.Visible) return Color.FromArgb(0x10, 0x60, 0xA0, 0xFF);
                if(!Enabled) return Color.FromArgb(0x10, 0x00, 0x00, 0x00);
                return Colors.Transparent;
            }}
            public Color StatusColor { get {
                if(EnableOnClose || InstallOnClose) return Color.FromRgb(0x20, 0x80, 0x20);
                if(DisableOnClose || UninstallOnClose) return Color.FromRgb(0x80, 0x80, 0x80);
                return Colors.Transparent;
            }}
            public bool ShowEnableButton { get {
                if(DisableOnClose) return true;
                if(EnableOnClose) return false;
                return !Enabled;
            }}
            public bool ShowDisabledTitle { get { return !(Enabled || InstallOnClose); } }
            public bool ShowDisableButton { get { return !ShowEnableButton; } }

            private bool cachedHasOptions;
            private bool optionsQueried;

            public bool HasOptions {
                get {
                    if(!Enabled) return false;
                    if(optionsQueried) return cachedHasOptions;
                    Plugin p;
                    if(PluginManager.TryGetStaticPluginInstance(PluginID, out p)) {
                        try {
                            cachedHasOptions = p.Instance.HasOption;
                            optionsQueried = true;
                            return cachedHasOptions;
                        }
                        catch(Exception ex) {
                            PluginManager.HandlePluginException(ex, new WindowInteropHelper(Window.GetWindow(parent)).Handle, Name,
                                    "Checking if the plugin has options.");
                        }
                    }
                    return false;
                }
            }

            public PluginEntry(Options12_Plugins parent, PluginInformation pluginInfo, PluginAssembly pluginAssembly) {
                this.parent = parent;
                PluginInfo = pluginInfo;
                PluginAssembly = pluginAssembly;
            }
        }

        #endregion
    }
}
