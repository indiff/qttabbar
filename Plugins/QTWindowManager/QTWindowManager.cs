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
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "QT Window Manager", Version = "1.1.0.0", Description = "Window manager")]
    public class QTWindowManager : IBarDropButton {
        private IPluginServer pluginServer;
        private string[] ResStrs;

        private byte[] ConfigValues = { 0, 0, 0, 0 };
        private Size sizeInitial = new Size(800, 600);
        private Point pntInitial = new Point(0, 0);
        private int ResizeDelta = 8;
        private Dictionary<string, Rectangle> dicPresets = new Dictionary<string, Rectangle>();
        private string startingPreset = String.Empty;

        private const int RES_COUNT = 15;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint WS_MAXIMIZE = 0x01000000;

        private bool fNowOptionShowing;
        private bool fNowTiled;

        private Dictionary<IntPtr, RECT> dicTiledRectangle;

        private const string CN_CabinetWClass = "CabinetWClass";
        private const string CN_Explorer = "";

        private bool fFillAllScreen;
        private int windowColumnLength = 2;
        private int windowRowLength = 3;


        public QTWindowManager() {
            ReadSettings();
        }


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, RES_COUNT, out ResStrs)) {
                if(CultureInfo.CurrentCulture.Parent.Name == "ja")
                    ResStrs = Resource.ResStrs_ja.Split(new char[] { ';' });
                else
                    ResStrs = Resource.ResStrs.Split(new char[] { ';' });
            }

            RestoreInitialSize();
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[RES_COUNT + dicPresets.Count];

            for(int i = 0; i < RES_COUNT; i++) {
                actions[i] = ResStrs[i];
            }

            int iPresets = RES_COUNT;
            foreach(string name in dicPresets.Keys) {
                actions[iPresets++] = name;
            }

            return true;
        }

        public void Close(EndCode code) {
            if(code != EndCode.Hidden)
                pluginServer = null;
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return true;
            }
        }

        public void OnOption() {
            ShowSettingWindow();
        }

        public void OnShortcutKeyPressed(int index) {
            if(index == 0)
                ShowSettingWindow();
            else if(index < 8)
                ResizeWindow(index);
            else if(index < 12)
                MoveWindow(index);
            else if(index < 15)
                MaxMinWindow(index);
            else
                DoPresets(index);
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
            // when split button enabled by user
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.QTWindowManager_large : Resource.QTWindowManager_small;
        }

        public void OnButtonClick() {
            ShowSettingWindow();
        }

        public bool ShowTextLabel {
            get {
                return false;
            }
        }

        public string Text {
            get {
                return ResStrs[15];
            }
        }

        #endregion


        #region IBarDropButton Members

        public bool IsSplitButton {
            get {
                return true;
            }
        }

        public void OnDropDownOpening(ToolStripDropDownMenu menu) {
            if(menu.Items.Count != dicPresets.Count + 4) {
                menu.Items.Clear();
                menu.ShowImageMargin = false;

                //7,12,13,14,presets

                menu.Items.Add(ResStrs[7]);
                menu.Items.Add(ResStrs[12]);
                menu.Items.Add(ResStrs[13]);
                menu.Items.Add(ResStrs[14]);
                menu.Items.Add("Tile");

                foreach(string name in dicPresets.Keys) {
                    ToolStripMenuItem tsmi = new ToolStripMenuItem(name);
                    tsmi.Tag = true;
                    menu.Items.Add(tsmi);
                }
            }
        }

        public void OnDropDownItemClick(ToolStripItem item, MouseButtons mouseButton) {
            ((ToolStripDropDown)item.Owner).Close(ToolStripDropDownCloseReason.ItemClicked);

            if(item.Tag != null && item.Tag is bool) {
                Rectangle rct;
                if(dicPresets.TryGetValue(item.Text, out rct))
                    DoPresetsCore(rct);

                return;
            }

            if(item.Text == ResStrs[7]) {
                ResizeWindow(7);
            }
            else if(item.Text == ResStrs[12]) {
                MaxMinWindow(12);
            }
            else if(item.Text == ResStrs[13]) {
                MaxMinWindow(13);
            }
            else if(item.Text == ResStrs[14]) {
                MaxMinWindow(14);
            }
            else {
                TileExplorers();
            }
        }


        #endregion



        public static void Uninstall() {
            using(RegistryKey rkPlugin = Registry.CurrentUser.OpenSubKey(CONSTANTS.REGISTRY_PLUGINSETTINGS, true)) {
                if(rkPlugin != null) {
                    try {
                        rkPlugin.DeleteSubKeyTree("QTWindowManager");
                    }
                    catch {
                    }
                }
            }
        }

        private void ReadSettings() {
            using(RegistryKey rkPluginQTWM = Registry.CurrentUser.OpenSubKey(CONSTANTS.REGISTRY_PLUGINSETTINGS + @"\QTWindowManager")) {
                if(rkPluginQTWM != null) {
                    ConfigValues = (byte[])rkPluginQTWM.GetValue("Config", new byte[] { 0, 0, 0, 0 });
                    int w = (int)rkPluginQTWM.GetValue("InitialWidth", 800);
                    int h = (int)rkPluginQTWM.GetValue("InitialHeight", 600);
                    int x = (int)rkPluginQTWM.GetValue("InitialX", 0);
                    int y = (int)rkPluginQTWM.GetValue("InitialY", 0);

                    sizeInitial = new Size(w, h);
                    pntInitial = new Point(x, y);
                    ResizeDelta = (int)rkPluginQTWM.GetValue("ResizeDelta", 3);
                    startingPreset = (string)rkPluginQTWM.GetValue("StartingPreset", String.Empty);

                    using(RegistryKey rkPresets = rkPluginQTWM.OpenSubKey("Presets")) {
                        if(rkPresets != null) {
                            foreach(string name in rkPresets.GetValueNames()) {
                                string val = (string)rkPresets.GetValue(name);
                                if(!String.IsNullOrEmpty(val)) {
                                    string[] vals = val.Split(new char[] { ',' });
                                    if(vals.Length == 4) {
                                        int[] nums = new int[4];
                                        bool fFail = false;

                                        for(int i = 0; i < 4; i++) {
                                            string strNum = vals[i].Trim();

                                            if(!int.TryParse(strNum, out nums[i])) {
                                                fFail = true;
                                                break;
                                            }
                                        }

                                        if(!fFail) {
                                            dicPresets[name] = new Rectangle(nums[0], nums[1], nums[2], nums[3]);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if(!string.IsNullOrEmpty(startingPreset)) {
                        if(!dicPresets.ContainsKey(startingPreset))
                            startingPreset = String.Empty;
                    }
                }
            }
        }

        private void SaveSettings() {
            using(RegistryKey rkPluginQTWM = Registry.CurrentUser.CreateSubKey(CONSTANTS.REGISTRY_PLUGINSETTINGS + @"\QTWindowManager")) {
                if(rkPluginQTWM != null) {
                    rkPluginQTWM.SetValue("Config", ConfigValues);
                    rkPluginQTWM.SetValue("InitialWidth", sizeInitial.Width);
                    rkPluginQTWM.SetValue("InitialHeight", sizeInitial.Height);
                    rkPluginQTWM.SetValue("InitialX", pntInitial.X);
                    rkPluginQTWM.SetValue("InitialY", pntInitial.Y);
                    rkPluginQTWM.SetValue("ResizeDelta", ResizeDelta);

                    rkPluginQTWM.DeleteSubKey("Presets", false);
                    if(dicPresets.Count > 0) {
                        using(RegistryKey rkPresets = rkPluginQTWM.CreateSubKey("Presets")) {
                            foreach(string name in dicPresets.Keys) {
                                Rectangle rct = dicPresets[name];

                                string val = rct.X + "," + rct.Y + "," + rct.Width + "," + rct.Height;

                                rkPresets.SetValue(name, val);
                            }
                        }
                    }

                    if(!string.IsNullOrEmpty(startingPreset)) {
                        if(dicPresets.ContainsKey(startingPreset)) {
                            rkPluginQTWM.SetValue("StartingPreset", startingPreset);
                        }
                        else {
                            rkPluginQTWM.SetValue("StartingPreset", String.Empty);
                            startingPreset = String.Empty;
                        }
                    }
                    else {
                        rkPluginQTWM.SetValue("StartingPreset", String.Empty);
                        startingPreset = String.Empty;
                    }
                }
            }
        }



        private void RestoreInitialSize() {
            bool fLoc = (ConfigValues[0] & 0x20) != 0;
            bool fSiz = (ConfigValues[0] & 0x80) != 0;
            bool fPreset = (ConfigValues[0] & 0x10) != 0;

            if(fLoc || fSiz || fPreset) {
                IntPtr hwnd = pluginServer.ExplorerHandle;
                if(hwnd != IntPtr.Zero) {
                    if(PInvoke_QTWM.IsZoomed(hwnd)) {
                        const int SW_RESTORE = 9;
                        PInvoke_QTWM.ShowWindow(hwnd, SW_RESTORE);
                    }

                    if(fPreset) {
                        if(!String.IsNullOrEmpty(startingPreset) && dicPresets.ContainsKey(startingPreset)) {
                            Rectangle rctPreset = dicPresets[startingPreset];
                            PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, rctPreset.X, rctPreset.Y, rctPreset.Width, rctPreset.Height, SWP_NOZORDER);
                            RemoveMAXIMIZE(hwnd);
                        }
                        return;
                    }

                    uint uFlags = SWP_NOZORDER | (fLoc ? 0 : SWP_NOMOVE) | (fSiz ? 0 : SWP_NOSIZE);

                    PInvoke_QTWM.SetWindowPos(
                        hwnd,
                        IntPtr.Zero,
                        pntInitial.X,
                        pntInitial.Y,
                        sizeInitial.Width,
                        sizeInitial.Height,
                        uFlags);

                    RemoveMAXIMIZE(hwnd);
                }
            }
        }

        private void ResizeWindow(int index) {
            IntPtr hwnd = pluginServer.ExplorerHandle;

            if(hwnd == IntPtr.Zero)
                return;

            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwnd, out rct);

            bool fAuto = (ConfigValues[0] & 0x40) == 0;

            int x = rct.left;
            int y = rct.top;
            int w = rct.Width;
            int h = rct.Height;
            uint uFlags = SWP_NOZORDER;

            switch(index) {
                case 1:
                    //Enlarge window
                    if(fAuto) {
                        x -= ResizeDelta;
                        y -= ResizeDelta;
                        w += ResizeDelta * 2;
                        h += ResizeDelta * 2;
                    }
                    else {
                        w += ResizeDelta;
                        h += ResizeDelta;
                        uFlags |= SWP_NOMOVE;
                    }
                    break;

                case 2:
                    //Shrink window
                    if(fAuto) {
                        x += ResizeDelta;
                        y += ResizeDelta;
                        w -= ResizeDelta * 2;
                        h -= ResizeDelta * 2;
                    }
                    else {
                        w -= ResizeDelta;
                        h -= ResizeDelta;
                        uFlags |= SWP_NOMOVE;
                    }
                    break;

                case 3:
                    //Widen window
                    if(fAuto) {
                        x -= ResizeDelta;
                        w += ResizeDelta * 2;
                    }
                    else {
                        w += ResizeDelta;
                        uFlags |= SWP_NOMOVE;
                    }
                    break;

                case 4:
                    //Narrow widnow
                    if(fAuto) {
                        x += ResizeDelta;
                        w -= ResizeDelta * 2;
                    }
                    else {
                        w -= ResizeDelta;
                        uFlags |= SWP_NOMOVE;
                    }
                    break;

                case 5:
                    //Heighten window
                    if(fAuto) {
                        y -= ResizeDelta;
                        h += ResizeDelta * 2;
                    }
                    else {
                        h += ResizeDelta;
                        uFlags |= SWP_NOMOVE;
                    }
                    break;
                case 6:
                    //Lower window
                    if(fAuto) {
                        y += ResizeDelta;
                        h -= ResizeDelta * 2;
                    }
                    else {
                        h -= ResizeDelta;
                        uFlags |= SWP_NOMOVE;
                    }
                    break;

                case 7:
                    //Restore size
                    w = sizeInitial.Width;
                    h = sizeInitial.Height;
                    uFlags |= SWP_NOMOVE;
                    break;
            }


            if(fAuto) {
                Rectangle rctScreen = Screen.FromHandle(hwnd).Bounds;

                if(x < rctScreen.X) {
                    x = rctScreen.X;

                    if(index == 7)
                        uFlags &= ~SWP_NOMOVE;
                }
                if(y < rctScreen.Y) {
                    y = rctScreen.Y;

                    if(index == 7)
                        uFlags &= ~SWP_NOMOVE;
                }
                if(x + w > rctScreen.Right) {
                    if(index == 7) {
                        x = rctScreen.Right - w;
                        uFlags &= ~SWP_NOMOVE;
                    }
                    else {
                        w = rctScreen.Right - x;
                    }
                }
                if(y + h > rctScreen.Bottom) {
                    if(index == 7) {
                        y = rctScreen.Bottom - h;
                        uFlags &= ~SWP_NOMOVE;
                    }
                    else {
                        h = rctScreen.Bottom - y;
                    }
                }
            }

            if(h > 150 && w > 122) {
                PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h, uFlags);

                RemoveMAXIMIZE(hwnd);
            }
        }

        private void MoveWindow(int index) {
            IntPtr hwnd = pluginServer.ExplorerHandle;

            if(hwnd == IntPtr.Zero)
                return;

            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwnd, out rct);

            int x = rct.left;
            int y = rct.top;

            switch(index) {
                case 8:
                    // left
                    x -= ResizeDelta;
                    break;

                case 9:
                    // right
                    x += ResizeDelta;
                    break;

                case 10:
                    // up
                    y -= ResizeDelta;
                    break;

                case 11:
                    // down
                    y += ResizeDelta;
                    break;
            }

            PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

            RemoveMAXIMIZE(hwnd);
        }

        internal static void RemoveMAXIMIZE(IntPtr hwnd) {
            PInvoke_QTWM.SetWindowLongPtr(hwnd, -16, PInvoke_QTWM.Ptr_OP_AND(PInvoke_QTWM.GetWindowLongPtr(hwnd, -16), ~WS_MAXIMIZE));
        }

        private void MaxMinWindow(int index) {
            //const int SW_MAXIMIZE = 3;
            //const int SW_MINIMIZE = 6;
            const int SW_RESTORE = 9;
            const int SW_SHOWMINIMIZED = 2;
            const int SW_SHOWMAXIMIZED = 3;

            IntPtr hwnd = pluginServer.ExplorerHandle;

            if(hwnd != IntPtr.Zero) {
                int nCmdShow;
                switch(index) {
                    case 13:
                        // Minimize
                        nCmdShow = SW_SHOWMINIMIZED;
                        break;

                    case 14:
                        // Restore
                        nCmdShow = SW_RESTORE;
                        break;

                    default:	//12
                        // Maximize
                        nCmdShow = SW_SHOWMAXIMIZED;
                        break;
                }

                PInvoke_QTWM.ShowWindow(hwnd, nCmdShow);
            }
        }

        private void DoPresets(int index) {
            int i = 0;
            foreach(Rectangle rct in dicPresets.Values) {
                if(i == index - RES_COUNT) {
                    DoPresetsCore(rct);
                    return;
                }
                i++;
            }
        }

        private void DoPresetsCore(Rectangle rct) {
            IntPtr hwnd = pluginServer.ExplorerHandle;
            if(hwnd != IntPtr.Zero) {
                uint uFlags = SWP_NOZORDER;

                //if( rct.X == 0 && rct.Y == 0 )
                //    uFlags |= SWP_NOMOVE;
                //if( rct.Width == 0 && rct.Height == 0 )
                //    uFlags |= SWP_NOSIZE;

                PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, rct.X, rct.Y, rct.Width, rct.Height, uFlags);
                RemoveMAXIMIZE(hwnd);
            }
        }

        private void ShowSettingWindow() {
            try {
                if(!fNowOptionShowing) {
                    using(SettingWindow sw = new SettingWindow(new Rectangle(pntInitial, sizeInitial), ConfigValues, ResizeDelta, pluginServer.ExplorerHandle, dicPresets, startingPreset)) {
                        fNowOptionShowing = true;
                        if(sw.ShowDialog() == DialogResult.OK) {
                            sizeInitial = sw.InitialSize;
                            pntInitial = sw.InitialLocation;
                            ConfigValues = sw.ConfigValues;
                            ResizeDelta = sw.ResizeDelta;
                            dicPresets = sw.Presets;
                            startingPreset = sw.StartingPreset ?? String.Empty;

                            SaveSettings();
                        }
                        fNowOptionShowing = false;
                    }
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }


        private void TileExplorers() {
            IntPtr hwndCurrent = pluginServer.ExplorerHandle;
            List<IntPtr> hwnds = EnumExplorer(false);
            int c = hwnds.Count;

            if(c > 1) {
                if(!fNowTiled) {
                    RECT rctCurrentWindow;
                    PInvoke_QTWM.GetWindowRect(hwndCurrent, out rctCurrentWindow);
                    Rectangle rctCurrentScreen = Screen.FromPoint(new Point(rctCurrentWindow.left, rctCurrentWindow.top)).WorkingArea;

                    List<IntPtr> lstFillingHWNDs = new List<IntPtr>();
                    Queue<IntPtr> qHwnds = new Queue<IntPtr>(hwnds);

                    dicTiledRectangle = new Dictionary<IntPtr, RECT>();

                    if(!fFillAllScreen) {
                        int w = rctCurrentScreen.Width / windowColumnLength;
                        int h = rctCurrentScreen.Height / windowRowLength;

                        int remain = c % windowColumnLength;
                        if(remain > 0) {
                            for(int i = 0; i < remain; i++) {
                                lstFillingHWNDs.Add(qHwnds.Dequeue());
                            }
                        }

                        int x = 0;
                        int y = 0;
                        uint uFlag = SWP_NOZORDER | SWP_NOACTIVATE;

                        if(lstFillingHWNDs.Count > 0) {
                            // Set position of left column
                            int hFilling = rctCurrentScreen.Height / lstFillingHWNDs.Count;
                            foreach(IntPtr hwnd in lstFillingHWNDs) {
                                RECT rctTMP;
                                PInvoke_QTWM.GetWindowRect(hwnd, out rctTMP);
                                dicTiledRectangle[hwnd] = rctTMP;

                                PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, 0, y, w, hFilling, hwnd == hwndCurrent ? SWP_NOZORDER : uFlag);
                                y += hFilling;
                            }
                            x = w;
                            y = 0;
                        }

                        // Set others 
                        foreach(IntPtr hwnd in qHwnds) {
                            RECT rctTMP;
                            PInvoke_QTWM.GetWindowRect(hwnd, out rctTMP);
                            dicTiledRectangle[hwnd] = rctTMP;

                            PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h, hwnd == hwndCurrent ? SWP_NOZORDER : uFlag);

                            y += h;
                            if(y + h > rctCurrentScreen.Bottom) {
                                y = 0;
                                x += w;
                            }
                        }
                        fNowTiled = true;
                    }
                }
                else {
                    if(dicTiledRectangle != null) {
                        uint uFlag = SWP_NOZORDER | SWP_NOACTIVATE;

                        foreach(IntPtr hwnd in dicTiledRectangle.Keys) {
                            RECT rct = dicTiledRectangle[hwnd];

                            PInvoke_QTWM.SetWindowPos(hwnd, IntPtr.Zero, rct.left, rct.top, rct.Width, rct.Height, hwnd == hwndCurrent ? SWP_NOZORDER : uFlag);
                        }

                        dicTiledRectangle.Clear();
                        fNowTiled = false;
                    }
                }
            }
        }

        private Point CalculateTiling(int windowCount) {
            if(windowCount < 3)
                return new Point(2, 1);
            if(windowCount < 5)
                return new Point(2, 2);
            if(windowCount < 10)
                return new Point(2, 3);

            return new Point(3, 3);
        }

        private List<IntPtr> lstExploererHwnd;
        private List<IntPtr> EnumExplorer(bool fExcludeCurrent) {
            lstExploererHwnd = new List<IntPtr>();

            PInvoke_QTWM.EnumWindows(
                EnumExplorerCallback,
                fExcludeCurrent ? pluginServer.ExplorerHandle : IntPtr.Zero);

            return lstExploererHwnd;
        }

        private bool EnumExplorerCallback(IntPtr hwnd, IntPtr lParam) {
            StringBuilder sb = new StringBuilder(260);
            PInvoke_QTWM.GetClassName(hwnd, sb, 260);

            string className = sb.ToString();
            if(lParam != hwnd && (className == CN_CabinetWClass || className == CN_Explorer)) {
                lstExploererHwnd.Add(hwnd);
            }

            return true;
        }

        /*
         * config	0x80	if on, initial sizing
         *			0x40	enlarge/shrink window at fixed pos.
         *			0x20	if on, initial location
         *			0x10	if on, starting preset
         *			0x08	if on, restore closed rct
         */
    }
}
