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
using System.Drawing;

namespace QTTabBarLib {
    public class AbstractListView : IDisposable {
        public event EventHandler ListViewDestroyed;
        
        protected bool fDisposed;

        public void Dispose() {
            Dispose(true);
        }
        
        public virtual IntPtr Handle {
            get { return IntPtr.Zero; }
        }

        public virtual void Dispose(bool fDisposing) {
            fDisposed = true;
        }


        public virtual int GetHotItem() {
            return -1;
        }

        public virtual void HandleF2() {
        }
        
        public virtual void HandleShiftKey() {
        }

        public virtual bool HasFocus() {
            return false;
        }

        public virtual void HideSubDirTip(int iReason = -1) {
        }

        public virtual void HideSubDirTipMenu() {
        }

        public virtual void HideSubDirTip_ExplorerInactivated() {
        }

        public virtual void HideThumbnailTooltip(int iReason = -1) {
        }

        public virtual int HitTest(IntPtr LParam) {
            return -1;
        }

        public virtual int HitTest(Point pt, bool ScreenCoords) {
            return -1;
        }

        public virtual bool HotItemIsSelected() {
            return false;
        }

        public virtual bool IsTrackingItemName() {
            return false;
        }

        public virtual bool MouseIsOverListView() {
            return false;
        }

        public virtual bool PointIsBackground(Point pt, bool screenCoords) {
            return false;
        }

        protected void OnListViewDestroyed() {
            if(ListViewDestroyed != null) {
                ListViewDestroyed(this, null);
            }
        }

        public virtual void RefreshSubDirTip(bool force = false) {
        }

        public virtual void ScrollHorizontal(int amount) {
        }

        public virtual void SetFocus() {
        }

        public virtual void SetRedraw(bool redraw) {
        }

        public virtual void ShowAndClickSubDirTip() {
        }

        public virtual bool SubDirTipMenuIsShowing() {
            return false;
        }
    }
}
