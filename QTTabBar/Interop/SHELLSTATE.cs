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

using System.Runtime.InteropServices;

namespace QTTabBarLib.Interop {
    [StructLayout(LayoutKind.Sequential)]
    public struct SHELLSTATE {
        // fShowAllObjects : 1
        // fShowExtensions : 1
        // fNoConfirmRecycle : 1
        // fShowSysFiles : 1
        // fShowCompColor : 1
        // fDoubleClickInWebView : 1
        // fDesktopHTML : 1
        // fWin95Classic : 1
        // fDontPrettyPath : 1
        // fShowAttribCol : 1
        // fMapNetDrvBtn : 1
        // fShowInfoTip : 1
        // fHideIcons : 1
        // fWebView : 1
        // fFilter : 1
        // fShowSuperHidden : 1
        // fNoNetCrawling : 1
        private uint bitvector1;
        public uint dwWin95Unused;
        public uint uWin95Unused;
        public int lParamSort;
        public int iSortDirection;
        public uint version;
        public uint uNotUsed;
        // fSepProcess : 1
        // fStartPanelOn : 1
        // fShowStartPage : 1
        // fSpareFlags : 13
        private uint bitvector2;

        public uint fShowAllObjects {
            get { return (bitvector1 & 1u) / 1; }
            set { bitvector1 = (value * 1) | bitvector1; }
        }

        public uint fShowExtensions {
            get { return (bitvector1 & 2u) / 2; }
            set { bitvector1 = (value * 2) | bitvector1; }
        }

        public uint fNoConfirmRecycle {
            get { return (bitvector1 & 4u) / 4; }
            set { bitvector1 = (value * 4) | bitvector1; }
        }

        public uint fShowSysFiles {
            get { return (bitvector1 & 8u) / 8; }
            set { bitvector1 = (value * 8) | bitvector1; }
        }

        public uint fShowCompColor {
            get { return (bitvector1 & 16u) / 16; }
            set { bitvector1 = (value * 16) | bitvector1; }
        }

        public uint fDoubleClickInWebView {
            get { return (bitvector1 & 32u) / 32; }
            set { bitvector1 = (value * 32) | bitvector1; }
        }

        public uint fDesktopHTML {
            get { return (bitvector1 & 64u) / 64; }
            set { bitvector1 = (value * 64) | bitvector1; }
        }

        public uint fWin95Classic {
            get { return (bitvector1 & 128u) / 128; }
            set { bitvector1 = (value * 128) | bitvector1; }
        }

        public uint fDontPrettyPath {
            get { return (bitvector1 & 256u) / 256; }
            set { bitvector1 = (value * 256) | bitvector1; }
        }

        public uint fShowAttribCol {
            get { return (bitvector1 & 512u) / 512; }
            set { bitvector1 = (value * 512) | bitvector1; }
        }

        public uint fMapNetDrvBtn {
            get { return (bitvector1 & 1024u) / 1024; }
            set { bitvector1 = (value * 1024) | bitvector1; }
        }

        public uint fShowInfoTip {
            get { return (bitvector1 & 2048u) / 2048; }
            set { bitvector1 = (value * 2048) | bitvector1; }
        }

        public uint fHideIcons {
            get { return (bitvector1 & 4096u) / 4096; }
            set { bitvector1 = (value * 4096) | bitvector1; }
        }

        public uint fWebView {
            get { return (bitvector1 & 8192u) / 8192; }
            set { bitvector1 = (value * 8192) | bitvector1; }
        }

        public uint fFilter {
            get { return (bitvector1 & 16384u) / 16384; }
            set { bitvector1 = (value * 16384) | bitvector1; }
        }

        public uint fShowSuperHidden {
            get { return (bitvector1 & 32768u) / 32768; }
            set { bitvector1 = (value * 32768) | bitvector1; }
        }

        public uint fNoNetCrawling {
            get { return (bitvector1 & 65536u) / 65536; }
            set { bitvector1 = (value * 65536) | bitvector1; }
        }

        public uint fSepProcess {
            get { return bitvector2 & 1u; }
            set { bitvector2 = value | bitvector2; }
        }

        public uint fStartPanelOn {
            get { return (bitvector2 & 2u) / 2; }
            set { bitvector2 = (value * 2) | bitvector2; }
        }

        public uint fShowStartPage {
            get { return (bitvector2 & 4u) / 4; }
            set { bitvector2 = (value * 4) | bitvector2; }
        }

        public uint fSpareFlags {
            get { return (bitvector2 & 65528u) / 8; }
            set { bitvector2 = (value * 8) | bitvector2; }
        }
    }
}
