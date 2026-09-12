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
    /// DPI 感知控件基类。借鉴 2048 Beta2 的 DpiAwareControl 设计。
    /// 继承此类的控件自动：
    /// 1. 构造时用系统 DPI 预缩放
    /// 2. 句柄创建后用窗口实际 DPI 重新缩放
    /// 3. WM_DPICHANGED 时自动重新缩放
    /// 4. 递归缩放子控件
    /// </summary>
    public class DpiAwareControl : Control, IDpiAwareObject
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

        /// <summary>DPI 感知的实际高度（已缩放）</summary>
        public int ActualHeightDpiAware => Height;

        /// <summary>DPI 感知的实际宽度（已缩放）</summary>
        public int ActualWidthDpiAware => Width;

        public DpiAwareControl()
        {
            // 构造时句柄未创建，用系统 DPI 预缩放
            try
            {
                _currentDpiScale = DpiManager.SystemDpiScale;
                DpiManager.ScaleBeforeHandleIsCreated(this);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareControl.ctor");
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 句柄创建后，用窗口实际 DPI 重新缩放
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

                // 注册 DPI 变化监听
                DpiManager.RegisterListener(this);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiAwareControl.OnHandleCreated");
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DpiManager.UnregisterListener(this);
            base.OnHandleDestroyed(e);
        }

        /// <summary>DPI 变化时调用，可被子类重写</summary>
        public virtual void OnDpiChanged(float scaleFactor)
        {
            if (_isDisposed || Math.Abs(scaleFactor - 1f) < 0.01f) return;

            try
            {
                this.SuspendLayout();

                // 缩放自身尺寸
                this.Width = (int)Math.Round(this.Width * scaleFactor);
                this.Height = (int)Math.Round(this.Height * scaleFactor);
                this.Padding = DpiManager.ScalePadding(this.Padding, scaleFactor);
                this.Margin = DpiManager.ScalePadding(this.Margin, scaleFactor);

                // 缩放字体（从基准计算，避免累积）
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
                QTUtility2.MakeErrorLog(ex, "DpiAwareControl.OnDpiChanged");
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
                    // 子控件也是 DpiAware，让它自己处理
                    dac.OnDpiChanged(scaleFactor);
                }
                else
                {
                    // 普通控件，手动缩放
                    child.Width = (int)Math.Round(child.Width * scaleFactor);
                    child.Height = (int)Math.Round(child.Height * scaleFactor);
                    child.Padding = DpiManager.ScalePadding(child.Padding, scaleFactor);
                    child.Margin = DpiManager.ScalePadding(child.Margin, scaleFactor);

                    // 递归
                    ScaleChildControls(child, scaleFactor);
                }
            }
        }

        /// <summary>手动触发 DPI 更新（用于外部通知）</summary>
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
            // WM_DPICHANGED = 0x02E0
            if (m.Msg == 0x02E0 && _dpiInitialized)
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
                    QTUtility2.MakeErrorLog(ex, "DpiAwareControl.WndProc WM_DPICHANGED");
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
