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
using BandObjectLib;
using SHDocVw;

namespace QTTabBarLib {

    // Stands in for the real deskband site object Explorer normally hands a
    // toolband via SetSite (which only happens if Explorer hosts a rebar/toolband
    // UI at all). Wraps a live IWebBrowser2 for an already-open window so
    // QTTabBarClass can attach to it directly, without ever being docked.
    internal class ExplorerSiteAdapter : _IServiceProvider, IInputObjectSite, IOleWindow {
        private readonly IWebBrowser2 webBrowser;

        public ExplorerSiteAdapter(IWebBrowser2 webBrowser) {
            this.webBrowser = webBrowser;
        }

        public void QueryService(Guid guid, Guid riid, out object obj) {
            // Explorer's own automation object answers these service queries itself
            // (IWebBrowserApp, IShellBrowser, ITravelLogStg, ...).
            ((_IServiceProvider)webBrowser).QueryService(guid, riid, out obj);
        }

        public int OnFocusChangeIS(object punkObj, int fSetFocus) {
            return 0;
        }

        public void GetWindow(out IntPtr phwnd) {
            // Deliberately IntPtr.Zero, not the Explorer frame HWND: BandObject.SetSite
            // stores this as ReBarHandle, and a non-zero value makes ShowDW take a
            // rebar-visual-fix path that BeginInvokes on the control before its window
            // handle exists (we have no real rebar band to report here).
            phwnd = IntPtr.Zero;
        }

        public void ContextSensitiveHelp(bool fEnterMode) {
        }
    }
}
