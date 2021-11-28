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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace QTTabBarLib {
    internal partial class Options07_Mouse : OptionsDialogTab {

        #region Dictionaries

        // Mappings to Resx indices.
        private readonly static Dictionary<MouseTarget, int> MouseTargetResx
                = new Dictionary<MouseTarget, int> {
            {MouseTarget.Anywhere,              6},
            {MouseTarget.Tab,                   7},
            {MouseTarget.TabBarBackground,      8},
            {MouseTarget.FolderLink,            9},
            {MouseTarget.ExplorerItem,          10},
            {MouseTarget.ExplorerBackground,    11},
        };
        private static readonly Dictionary<MouseChord, int> MouseButtonResx
                = new Dictionary<MouseChord, int> {
            {MouseChord.Left,   12},
            {MouseChord.Right,  13},
            {MouseChord.Middle, 14},
            {MouseChord.Double, 15},
            {MouseChord.X1,     16},
            {MouseChord.X2,     17},
        };

        // I don't think these need to be localized.
        private static readonly Dictionary<MouseChord, string> MouseModifierStrings
                = new Dictionary<MouseChord, string> {
            {MouseChord.None,   ""},
            {MouseChord.Shift,  "Shift + "},
            {MouseChord.Ctrl,   "Ctrl + "},
            {MouseChord.Alt,    "Alt + "},
        };

        // Which actions are valid for which targets?
        private readonly static Dictionary<MouseTarget, BindAction[]> MouseTargetActions
                = new Dictionary<MouseTarget, BindAction[]> {
            {MouseTarget.Anywhere, new BindAction[] {
                BindAction.Nothing,
                BindAction.GoBack,
                BindAction.GoFirst,
                BindAction.GoForward,
                BindAction.GoLast,
                BindAction.NextTab,
                BindAction.PreviousTab
            }},
            {MouseTarget.Tab, new BindAction[] {
                BindAction.Nothing,
                BindAction.CloseTab,
                BindAction.CloseAllButThis,
                BindAction.UpOneLevelTab,
                BindAction.LockTab,
                BindAction.ShowTabMenu,
                BindAction.CloneTab,
                BindAction.TearOffTab,
                BindAction.CopyTabPath,
                BindAction.TabProperties,
                BindAction.ShowTabSubfolderMenu,
            }},
            {MouseTarget.TabBarBackground, new BindAction[] {
                BindAction.Nothing,
                BindAction.NewTab,
                BindAction.NewWindow,
                BindAction.UpOneLevel,
                BindAction.CloseAllButCurrent,
                BindAction.CloseWindow,
                BindAction.RestoreLastClosed,
                BindAction.CloneCurrent,
                BindAction.TearOffCurrent,
                BindAction.LockAll,
                BindAction.BrowseFolder,
                BindAction.ShowOptions,
                BindAction.ShowToolbarMenu,
                BindAction.SortTabsByName,
                BindAction.SortTabsByPath,
                BindAction.SortTabsByActive,
            }},
            {MouseTarget.FolderLink, new BindAction[] {
                BindAction.Nothing,
                BindAction.ItemOpenInNewTab,
                BindAction.ItemOpenInNewTabNoSel,
                BindAction.ItemOpenInNewWindow,
                BindAction.ItemProperties,
                BindAction.CopyItemPath,
                BindAction.CopyItemName,
            }},
            {MouseTarget.ExplorerItem, new BindAction[] {
                BindAction.Nothing,
                BindAction.ItemOpenInNewTab,
                BindAction.ItemOpenInNewTabNoSel,
                BindAction.ItemOpenInNewWindow,
                BindAction.ItemCut,
                BindAction.ItemCopy,        
                BindAction.ItemDelete,
                BindAction.ItemProperties,
                BindAction.CopyItemPath,
                BindAction.CopyItemName,
                BindAction.ChecksumItem,
            }},
            {MouseTarget.ExplorerBackground, new BindAction[] { // 初始化在背景区域的工作
                BindAction.Nothing,
                BindAction.BrowseFolder,
                BindAction.NewFolder,
                BindAction.NewFile,
                BindAction.UpOneLevel,
                BindAction.Refresh,
                BindAction.Paste,
                BindAction.Maximize,
                BindAction.Minimize,

                // add by qwop start  comment 2012, 08,12, 23:13
                BindAction.OpenCmd,
                BindAction.ItemsOpenInNewTabNoSel
                // add by qwop end.
            }},
        };

        // Which buttons are valid for which targets?
        private static readonly Dictionary<MouseTarget, MouseChord[]> MouseTargetButtons
                = new Dictionary<MouseTarget, MouseChord[]> {
            {MouseTarget.Anywhere, new MouseChord[] {
                MouseChord.X1,
                MouseChord.X2,
            }},
            {MouseTarget.ExplorerBackground, new MouseChord[] {
                MouseChord.Middle,
                MouseChord.Double,
            }},
            {MouseTarget.ExplorerItem, new MouseChord[] {
                MouseChord.Middle,
            }},
            {MouseTarget.FolderLink, new MouseChord[] {
                MouseChord.Left,
                MouseChord.Middle,
            }},
            {MouseTarget.Tab, new MouseChord[] {
                MouseChord.Left,
                MouseChord.Middle,
                MouseChord.Double,
            }},
            {MouseTarget.TabBarBackground, new MouseChord[] {
                MouseChord.Left,
                MouseChord.Middle,
                MouseChord.Double,
            }},
        };

        private static Dictionary<MouseButton, MouseChord> MouseButtonMappings =
                new Dictionary<MouseButton, MouseChord> {
                    { MouseButton.Left, MouseChord.Left },
                    { MouseButton.Right, MouseChord.Right },
                    { MouseButton.Middle, MouseChord.Middle },
                    { MouseButton.XButton1, MouseChord.X1 },
                    { MouseButton.XButton2, MouseChord.X2 }
        };

        #endregion

        private ObservableCollection<MouseEntry> MouseBindings;
        private DispatcherTimer mouseTimer;

        private static int ActionToResx(BindAction ba) {
            int i = (int)ba;
            return i < (int)BindAction.KEYBOARD_ACTION_COUNT
                    ? i
                    : i + (int)BindAction.KEYBOARD_ACTION_COUNT - QTUtility.FIRST_MOUSE_ONLY_ACTION;
        }

        private static BindAction ResxToAction(int i) {
            return i < (int)BindAction.KEYBOARD_ACTION_COUNT
                    ? (BindAction)i
                    : (BindAction)(i - (int)BindAction.KEYBOARD_ACTION_COUNT + QTUtility.FIRST_MOUSE_ONLY_ACTION);
        }

        public Options07_Mouse() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            MouseBindings = new ObservableCollection<MouseEntry>();
            foreach(var p in WorkingConfig.mouse.GlobalMouseActions) {
                MouseBindings.Add(new MouseEntry(MouseTarget.Anywhere, p.Key, p.Value));
            }
            foreach(var p in WorkingConfig.mouse.MarginActions) {
                MouseBindings.Add(new MouseEntry(MouseTarget.ExplorerBackground, p.Key, p.Value));
            }
            foreach(var p in WorkingConfig.mouse.ItemActions) {
                MouseBindings.Add(new MouseEntry(MouseTarget.ExplorerItem, p.Key, p.Value));
            }
            foreach(var p in WorkingConfig.mouse.LinkActions) {
                MouseBindings.Add(new MouseEntry(MouseTarget.FolderLink, p.Key, p.Value));
            }
            foreach(var p in WorkingConfig.mouse.TabActions) {
                MouseBindings.Add(new MouseEntry(MouseTarget.Tab, p.Key, p.Value));
            }
            foreach(var p in WorkingConfig.mouse.BarActions) {
                MouseBindings.Add(new MouseEntry(MouseTarget.TabBarBackground, p.Key, p.Value));
            }
            ICollectionView view = CollectionViewSource.GetDefaultView(MouseBindings);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("TargetIdx");
            foreach(MouseTarget target in Enum.GetValues(typeof(MouseTarget))) {
                groupDescription.GroupNames.Add(MouseTargetResx[target]);
            }

            view.GroupDescriptions.Add(groupDescription);
            lvwMouseBindings.ItemsSource = view;
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.mouse = new Config._Mouse();
            InitializeConfig();
        }

        public override void CommitConfig() {
            WorkingConfig.mouse.GlobalMouseActions = MouseBindings
                    .Where(e => e.Action != BindAction.Nothing && e.Target == MouseTarget.Anywhere)
                    .ToDictionary(e => e.Chord, e => e.Action);
            WorkingConfig.mouse.MarginActions = MouseBindings
                    .Where(e => e.Action != BindAction.Nothing && e.Target == MouseTarget.ExplorerBackground)
                    .ToDictionary(e => e.Chord, e => e.Action);
            WorkingConfig.mouse.ItemActions = MouseBindings
                    .Where(e => e.Action != BindAction.Nothing && e.Target == MouseTarget.ExplorerItem)
                    .ToDictionary(e => e.Chord, e => e.Action);
            WorkingConfig.mouse.LinkActions = MouseBindings
                    .Where(e => e.Action != BindAction.Nothing && e.Target == MouseTarget.FolderLink)
                    .ToDictionary(e => e.Chord, e => e.Action);
            WorkingConfig.mouse.TabActions = MouseBindings
                    .Where(e => e.Action != BindAction.Nothing && e.Target == MouseTarget.Tab)
                    .ToDictionary(e => e.Chord, e => e.Action);
            WorkingConfig.mouse.BarActions = MouseBindings
                    .Where(e => e.Action != BindAction.Nothing && e.Target == MouseTarget.TabBarBackground)
                    .ToDictionary(e => e.Chord, e => e.Action);
        }


        private void rctAddMouseAction_MouseDown(object sender, MouseButtonEventArgs e) {
            FrameworkElement control = ((FrameworkElement)sender);
            MouseChord chord = MouseChord.None;
            if((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                chord |= MouseChord.Shift;
            }
            if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                chord |= MouseChord.Ctrl;
            }
            if((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) {
                chord |= MouseChord.Alt;
            }
            // ugh.  wish there was a better way to do this, but I don't think there is one...
            MouseTarget target = MouseTargetResx.First(kv => kv.Value == (int)control.Tag).Key;

            // watch out for double clicks
            if(e.ChangedButton == MouseButton.Left) {
                if(mouseTimer == null || mouseTimer.Tag != control.Tag) {
                    mouseTimer = new DispatcherTimer {
                        Tag = control.Tag,
                        Interval = TimeSpan.FromMilliseconds(
                                System.Windows.Forms.SystemInformation.DoubleClickTime)
                    };
                    mouseTimer.Tick += (sender2, e2) => {
                        mouseTimer.IsEnabled = false;
                        mouseTimer = null;
                        chord |= MouseChord.Left;
                        AddMouseAction(chord, target);
                    };
                    mouseTimer.IsEnabled = true;
                }
                else {
                    mouseTimer.IsEnabled = false;
                    mouseTimer = null;
                    chord |= MouseChord.Double;
                    AddMouseAction(chord, target);
                }
            }
            else {
                if(mouseTimer != null) {
                    mouseTimer.IsEnabled = false;
                    mouseTimer = null;
                }
                chord |= MouseButtonMappings[e.ChangedButton];
                AddMouseAction(chord, target);
            }
        }

        private void AddMouseAction(MouseChord chord, MouseTarget target) {
            MouseChord button = chord & ~(MouseChord.Alt | MouseChord.Ctrl | MouseChord.Shift);
            if(!MouseTargetButtons[target].Contains(button)) {
                var dict = QTUtility.TextResourcesDic["Options_Page07_Mouse"];
                MessageBox.Show(
                        dict[18] + Environment.NewLine +
                        dict[19] + Environment.NewLine +
                        MouseTargetButtons[target].Select(k => "  " + dict[MouseButtonResx[k]])
                            .StringJoin(Environment.NewLine),
                        dict[20], MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            MouseEntry entry = MouseBindings.FirstOrDefault(e => e.Chord == chord && e.Target == target);
            if(entry == null) {
                entry = new MouseEntry(target, chord, BindAction.Nothing);
                MouseBindings.Add(entry);
            }
            entry.IsSelected = true;
            lvwMouseBindings.UpdateLayout();
            lvwMouseBindings.ScrollIntoView(entry);
            // Need to wait for ScrollIntoView to finish, or the dropdown will open in the wrong place.
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => { entry.IsEditing = true; }));
        }

        private void lvwMouseBindings_KeyDown(object sender, KeyEventArgs e) {
            MouseEntry entry = lvwMouseBindings.SelectedItem as MouseEntry;
            if(entry == null) return;
            if(e.Key == Key.Delete) {
                MouseBindings.Remove(entry);
            }
            else if(e.Key == Key.Space || e.Key == Key.Enter) {
                entry.IsEditing = true;
            }
        }

        private void cmbInlineMouseAction_Loaded(object sender, RoutedEventArgs e) {
            // For some reason, SelectedValue gets wonky when the config is reinitialized.
            // This seems to fix it.
            ((ComboBox)sender).GetBindingExpression(Selector.SelectedValueProperty).UpdateTarget();
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class MouseEntry : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            private bool isSelected;
            public bool IsSelected {
                get {
                    return isSelected;
                }
                set {
                    isSelected = value;
                    if(!isSelected) IsEditing = false;
                }
            }
            private bool isEditing;
            public bool IsEditing {
                get {
                    return isEditing;
                }
                set {
                    isEditing = value;
                    if(isEditing) IsSelected = true;
                }
            }
            public IEnumerable<int> ComboBoxItems {
                get { return MouseTargetActions[Target].Select(ActionToResx); }
            }
            public string GestureModifiers {
                get {
                    return new MouseChord[] {MouseChord.Ctrl, MouseChord.Shift, MouseChord.Alt}
                            .Where(mod => (Chord & mod) == mod)
                            .Select(c => MouseModifierStrings[c])
                            .StringJoin("");
                }
            }
            public int ButtonIdx { get {
                return MouseButtonResx[Chord & ~(MouseChord.Alt | MouseChord.Ctrl | MouseChord.Shift)];
            }}
            public int TargetIdx { get { return MouseTargetResx[Target]; } }
            public MouseTarget Target { get; private set; }
            public BindAction Action { get; set; }
            public MouseChord Chord { get; private set; }
            public int ActionIdx {
                get { return ActionToResx(Action); }
                set { Action = ResxToAction(value); }
            }

            public MouseEntry(MouseTarget target, MouseChord chord, BindAction action) {
                Target = target;
                Action = action;
                Chord = chord;
            }
        }

        #endregion
    }
}
