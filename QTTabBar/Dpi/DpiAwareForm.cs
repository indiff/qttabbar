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
    /// DPI 感知窗体基类。
    /// 继承此类的窗体自动响应 DPI 变化。
    /// </summary>
    public class DpiAwareForm : Form, IDpiAwareObject
    {
        private float _currentDpiScale = 1f;
        private bool _scaleChildren = true;
        private bool _dpiInitialized;
        private bool _isDisposed;

        /// <summary>当前 DPI 缩放因子</summary>
        public float DpiScale => _currentDpiScale;

        /// <summary>是否自动缩放子控件</summary>
        public bool ScaleChildren
        {
            get => _scaleChildren;
            set => _scaleChildren = value;
        }

        public DpiAwareForm()
        {
            // 构造时用系统 DPI 预缩放
            try
            {
                _currentDpiScale = DpiManager.SystemDpiScale;
                // 窗体不预缩放尺寸（由设计时尺寸决定），只预缩放字体
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareForm.ctor");
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
                QTUtility2.MakeErrorLog(ex, "DpiAwareForm.OnHandleCreated");
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DpiManager.UnregisterListener(this);
            base.OnHandleDestroyed(e);
        }

        /// <summary>DPI 变化时调用</summary>
        public virtual void OnDpiChanged(float scaleFactor)
        {
            if (_isDisposed || Math.Abs(scaleFactor - 1f) < 0.01f) return;

            try
            {
                this.SuspendLayout();

                // 缩放窗体尺寸
                this.ClientSize = new Size(
                    (int)Math.Round(this.ClientSize.Width * scaleFactor),
                    (int)Math.Round(this.ClientSize.Height * scaleFactor));

                // 缩放字体
                if (this.Font != null)
                {
                    float textScale = QTUtility.TextScale;
                    float newFontSize = DpiManager.UNSCALED_FONTSIZE * textScale;
                    if (Math.Abs(this.Font.SizeInPoints - newFontSize) > 0.01f)
                    {
                        this.Font = new Font(this.Font.FontFamily, newFontSize, this.Font.Style);
                    }
                }

                // 递归缩放子控件
                if (_scaleChildren)
                {
                    ScaleChildControls(this, scaleFactor);
                }

                this.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareForm.OnDpiChanged");
            }
        }

        /// <summary>递归缩放子控件</summary>
        protected virtual void ScaleChildControls(Control parent, float scaleFactor)
        {
            if (parent == null || parent.Controls == null) return;

            foreach (Control child in parent.Controls)
            {
                if (child is DpiAwareControl dac)
                {
                    dac.OnDpiChanged(scaleFactor);
                }
                else if (child is DpiAwareForm daf)
                {
                    daf.OnDpiChanged(scaleFactor);
                }
                else
                {
                    child.Width = (int)Math.Round(child.Width * scaleFactor);
                    child.Height = (int)Math.Round(child.Height * scaleFactor);
                    child.Padding = DpiManager.ScalePadding(child.Padding, scaleFactor);
                    child.Margin = DpiManager.ScalePadding(child.Margin, scaleFactor);
                    ScaleChildControls(child, scaleFactor);
                }
            }
        }

        /// <summary>手动触发 DPI 更新</summary>
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
            if (m.Msg == 0x02E0 && _dpiInitialized) // WM_DPICHANGED
            {
                try
                {
                    float newScale = DpiManager.GetWindowDpiScale(this.Handle);
                    if (Math.Abs(newScale - _currentDpiScale) > 0.01f)
                    {
                        float factor = newScale / _currentDpiScale;
                        _currentDpiScale = newScale;
                        OnDpiChanged(factor);
                    }
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex, "DpiAwareForm.WndProc");
                }
            }
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            _isDisposed = true;
            if (disposing)
            {
                DpiManager.UnregisterListener(this);
            }
            base.Dispose(disposing);
        }
    }
}
