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
using System.Drawing;
using System.Windows.Forms;
using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "SampleClock", Version = "0.9.0.0", Description = "Sample clock plugin")]
    public class ClockSample : IBarCustomItem {
        private ToolStripLabel labelTime;
        private Timer timer;
        private bool fOn;

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
        }

        public void Close(EndCode code) {
            if(timer != null) {
                timer.Dispose();
            }

            if(labelTime != null) {
                labelTime.Dispose();
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }


        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        public void OnShortcutKeyPressed(int iIndex) {
        }

        #endregion


        #region IBarCustomItem Members

        public ToolStripItem CreateItem(bool fLarge, DisplayStyle displayStyle) {
            if(labelTime == null) {
                labelTime = new ToolStripLabel();
                labelTime.DisplayStyle = ToolStripItemDisplayStyle.Text;
                labelTime.AutoSize = true;
                labelTime.Font = new Font("Courier New", labelTime.Font.SizeInPoints);
                labelTime.Alignment = ToolStripItemAlignment.Right;
                labelTime.Padding = new Padding(0, 0, 24, 0);
            }

            if(timer == null) {
                timer = new Timer();
                timer.Interval = 1000;
                timer.Tick += timer_Tick;
            }

            timer.Start();

            return labelTime;
        }

        #endregion


        private void timer_Tick(object sender, EventArgs e) {
            DateTime dt = DateTime.Now;

            int h = dt.Hour;
            int m = dt.Minute;
            string sep = fOn ? " " : ":";

            labelTime.Text = h + sep + (m < 10 ? "0" : String.Empty) + m;
            labelTime.ToolTipText = dt.ToLongDateString();

            fOn = !fOn;
        }
    }
}
