//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2025  Quizo, Paul Accisano, indiff
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

namespace QTTabBarLib.Dpi
{
    /// <summary>
    /// DPI 感知用户控件基类。 DpiAwareUserControl 设计。
    /// </summary>
    public class DpiAwareUserControl : UserControl, IDpiAwareObject
    {
        private float _currentDpiScale = 1f;
        private bool _scaleChildren = true;
        private bool _dpiInitialized;
        private bool _isDisposed;

        public float DpiScale => _currentDpiScale;

        public bool ScaleChildren
        {
            get => _scaleChildren;
            set => _scaleChildren = value;
        }

        public DpiAwareUserControl()
        {
            try
            {
                _currentDpiScale = DpiManager.SystemDpiScale;
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareUserControl.ctor");
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                float newScale = DpiManager.GetWindowDpiScale(this.Handle);
                if (Math.Abs(newScale - _currentDpiScale) > 0.01f)
                {
                    float factor = newScale / _currentDpiScale;
                    _currentDpiScale = newScale;
                    OnDpiChanged(factor);
                }
                _dpiInitialized = true;
                DpiManager.RegisterListener(this);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareUserControl.OnHandleCreated");
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DpiManager.UnregisterListener(this);
            base.OnHandleDestroyed(e);
        }

        public virtual void OnDpiChanged(float scaleFactor)
        {
            if (_isDisposed || Math.Abs(scaleFactor - 1f) < 0.01f) return;

            try
            {
                this.SuspendLayout();
                this.Width = (int)Math.Round(this.Width * scaleFactor);
                this.Height = (int)Math.Round(this.Height * scaleFactor);
                this.Padding = DpiManager.ScalePadding(this.Padding, scaleFactor);
                this.Margin = DpiManager.ScalePadding(this.Margin, scaleFactor);

                if (this.Font != null)
                {
                    float textScale = QTUtility.TextScale;
                    float newFontSize = DpiManager.UNSCALED_FONTSIZE * textScale;
                    if (Math.Abs(this.Font.SizeInPoints - newFontSize) > 0.01f)
                    {
                        this.Font = new Font(this.Font.FontFamily, newFontSize, this.Font.Style);
                    }
                }

                if (_scaleChildren)
                {
                    foreach (Control child in this.Controls)
                    {
                        if (child is IDpiAwareObject dac)
                            dac.OnDpiChanged(scaleFactor);
                        else
                        {
                            child.Width = (int)Math.Round(child.Width * scaleFactor);
                            child.Height = (int)Math.Round(child.Height * scaleFactor);
                        }
                    }
                }
                this.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareUserControl.OnDpiChanged");
            }
        }

        public void UpdateDpi()
        {
            if (!IsHandleCreated) return;
            float newScale = DpiManager.GetWindowDpiScale(this.Handle);
            if (Math.Abs(newScale - _currentDpiScale) > 0.01f)
            {
                float factor = newScale / _currentDpiScale;
                _currentDpiScale = newScale;
                OnDpiChanged(factor);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x02E0 && _dpiInitialized)
            {
                try { UpdateDpi(); }
                catch (Exception ex) { QTUtility2.MakeErrorLog(ex, "DpiAwareUserControl.WndProc"); }
            }
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            _isDisposed = true;
            if (disposing) DpiManager.UnregisterListener(this);
            base.Dispose(disposing);
        }
    }
}
