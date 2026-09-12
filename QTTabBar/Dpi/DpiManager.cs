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
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QTTabBarLib.Dpi
{
    /// <summary>
    /// 中心 DPI 管理器。借鉴 2048 Beta2 的 DpiManager 架构设计。
    /// 统一管理 DPI 状态、缩放计算、DPI 变化通知。
    /// </summary>
    public static class DpiManager
    {
        #region Win32 API

        // ✅ 重命名 P/Invoke 方法，避免与公开方法 GetDpiForWindow 冲突
        [DllImport("user32.dll", EntryPoint = "GetDpiForWindow")]
        private static extern int NativeGetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int GetDpiForSystem();

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        private const int MDT_EFFECTIVE_DPI = 0;
        private const uint MONITOR_DEFAULTTONEAREST = 2;

        #endregion

        #region 字段和属性

        private static float _systemDpiScale = 1f;
        private static int _systemDpi = 96;
        private static bool _perMonitorSupported;
        private static readonly List<WeakReference<IDpiAwareObject>> _listeners = new List<WeakReference<IDpiAwareObject>>();

        /// <summary>系统 DPI（主显示器）</summary>
        public static int SystemDpi => _systemDpi;

        /// <summary>系统 DPI 缩放因子</summary>
        public static float SystemDpiScale => _systemDpiScale;

        /// <summary>是否支持 Per-Monitor DPI（Win8.1+）</summary>
        public static bool PerMonitorDpiIsSupported => _perMonitorSupported;

        /// <summary>未缩放的基准字体大小（point）</summary>
        public const float UNSCALED_FONTSIZE = 9.0f;

        #endregion

        #region 初始化

        static DpiManager()
        {
            try
            {
                _systemDpi = GetDpiForSystem();
                if (_systemDpi <= 0) _systemDpi = 96;
                _systemDpiScale = _systemDpi / 96f;

                // 检测 Per-Monitor 支持（Win8.1+）
                _perMonitorSupported = Environment.OSVersion.Version >= new Version(6, 3);
            }
            catch
            {
                _systemDpi = 96;
                _systemDpiScale = 1f;
                _perMonitorSupported = false;
            }
        }

        #endregion

        #region DPI 获取

        /// <summary>获取窗口所在显示器的 DPI</summary>
        public static int GetDpiForWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return _systemDpi;
            try
            {
                int dpi = NativeGetDpiForWindow(hwnd);
                return dpi > 0 ? dpi : _systemDpi;
            }
            catch { return _systemDpi; }
        }

        /// <summary>获取控件所在显示器的 DPI</summary>
        public static int GetDpiForControl(Control control)
        {
            if (control == null || !control.IsHandleCreated) return _systemDpi;
            return GetDpiForWindow(control.Handle);
        }

        /// <summary>从屏幕坐标获取 DPI（多显示器场景）</summary>
        public static int GetDpiFromPoint(int x, int y)
        {
            if (!_perMonitorSupported) return _systemDpi;
            try
            {
                POINT pt = new POINT { X = x, Y = y };
                IntPtr hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
                if (hMonitor == IntPtr.Zero) return _systemDpi;
                GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out _);
                return (int)dpiX;
            }
            catch { return _systemDpi; }
        }

        /// <summary>获取窗口的 DPI 缩放因子</summary>
        public static float GetWindowDpiScale(IntPtr hwnd)
        {
            return GetDpiForWindow(hwnd) / 96f;
        }

        /// <summary>获取控件的 DPI 缩放因子</summary>
        public static float GetControlDpiScale(Control control)
        {
            return GetDpiForControl(control) / 96f;
        }

        #endregion

        #region 缩放计算

        /// <summary>按 DPI 缩放像素值</summary>
        public static int Scale(int px, float dpiScale)
        {
            return (int)Math.Round(px * dpiScale);
        }

        /// <summary>按系统 DPI 缩放像素值</summary>
        public static int ScaleBySystem(int px)
        {
            return (int)Math.Round(px * _systemDpiScale);
        }

        /// <summary>按窗口 DPI 缩放像素值</summary>
        public static int ScaleByWindow(int px, IntPtr hwnd)
        {
            return (int)Math.Round(px * GetWindowDpiScale(hwnd));
        }

        /// <summary>缩放 Padding</summary>
        public static Padding ScalePadding(Padding p, float dpiScale)
        {
            return new Padding(
                (int)Math.Round(p.Left * dpiScale),
                (int)Math.Round(p.Top * dpiScale),
                (int)Math.Round(p.Right * dpiScale),
                (int)Math.Round(p.Bottom * dpiScale));
        }

        /// <summary>缩放 Size</summary>
        public static Size ScaleSize(Size s, float dpiScale)
        {
            return new Size((int)Math.Round(s.Width * dpiScale), (int)Math.Round(s.Height * dpiScale));
        }

        /// <summary>缩放字体（只乘文本缩放，不乘 DPI，因为 Font 是 point 单位）</summary>
        public static float ScaleFont(float pt, float textScale)
        {
            return pt * textScale;
        }

        /// <summary>从基准字体大小计算缩放后的字体（避免累积缩放）</summary>
        public static Font ScaleFontFromBase(Font baseFont, float textScale)
        {
            if (baseFont == null) return null;
            float scaledSize = UNSCALED_FONTSIZE * textScale;
            return new Font(baseFont.FontFamily, scaledSize, baseFont.Style);
        }

        #endregion

        #region 句柄创建前的预缩放

        /// <summary>
        /// 控件句柄创建前的预缩放。
        /// 解决构造函数中 Handle 尚未创建、GetDpiForWindow 返回默认 96 DPI 的问题。
        /// 借鉴 2048 Beta2 的 ScaleBeforeHandleIsCreated。
        /// </summary>
        public static void ScaleBeforeHandleIsCreated(Control control)
        {
            if (control == null) return;
            try
            {
                // 句柄未创建时，用系统 DPI 预缩放
                float scale = _systemDpiScale;
                ScaleControlCore(control, scale);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "DpiManager.ScaleBeforeHandleIsCreated");
            }
        }

        private static void ScaleControlCore(Control control, float scaleFactor)
        {
            if (control == null || Math.Abs(scaleFactor - 1f) < 0.01f) return;

            control.Width = (int)Math.Round(control.Width * scaleFactor);
            control.Height = (int)Math.Round(control.Height * scaleFactor);
            control.Padding = ScalePadding(control.Padding, scaleFactor);
            control.Margin = ScalePadding(control.Margin, scaleFactor);

            // 递归缩放子控件
            foreach (Control child in control.Controls)
            {
                ScaleControlCore(child, scaleFactor);
            }
        }

        #endregion

        #region DPI 变化通知

        /// <summary>注册 DPI 变化监听器</summary>
        public static void RegisterListener(IDpiAwareObject obj)
        {
            if (obj == null) return;
            lock (_listeners)
            {
                _listeners.RemoveAll(wr => !wr.TryGetTarget(out _));
                _listeners.Add(new WeakReference<IDpiAwareObject>(obj));
            }
        }

        /// <summary>注销 DPI 变化监听器</summary>
        public static void UnregisterListener(IDpiAwareObject obj)
        {
            if (obj == null) return;
            lock (_listeners)
            {
                _listeners.RemoveAll(wr =>
                    wr.TryGetTarget(out var target) && ReferenceEquals(target, obj));
            }
        }

        /// <summary>通知所有监听器 DPI 变化</summary>
        public static void NotifyDpiChanged(float oldScale, float newScale)
        {
            if (Math.Abs(oldScale - newScale) < 0.01f) return;
            float scaleFactor = newScale / oldScale;

            IDpiAwareObject[] listeners;
            lock (_listeners)
            {
                var list = new List<IDpiAwareObject>();
                foreach (var wr in _listeners)
                {
                    if (wr.TryGetTarget(out var obj))
                        list.Add(obj);
                }
                listeners = list.ToArray();
            }

            foreach (var listener in listeners)
            {
                try { listener.OnDpiChanged(scaleFactor); }
                catch (Exception ex) { QTUtility2.MakeErrorLog(ex, "DpiManager.NotifyDpiChanged"); }
            }
        }

        #endregion

        #region 预计算缓存

        private static float _cachedDpiScale = -1;
        private static int _scaledTabHeight;
        private static Padding _scaledPadding;
        private static int _scaledButtonHeight;

        /// <summary>预计算的标签栏高度（带缓存）</summary>
        public static int ScaledTabHeight(int baseHeight, float dpiScale)
        {
            if (Math.Abs(_cachedDpiScale - dpiScale) > 0.01f)
            {
                _cachedDpiScale = dpiScale;
                _scaledTabHeight = Scale(baseHeight, dpiScale);
                _scaledPadding = new Padding(Scale(4, dpiScale), Scale(2, dpiScale), Scale(4, dpiScale), Scale(2, dpiScale));
                _scaledButtonHeight = Scale(36, dpiScale);
            }
            return _scaledTabHeight;
        }

        /// <summary>预计算的 Padding（带缓存）</summary>
        public static Padding ScaledPadding => _scaledPadding;

        /// <summary>预计算的按钮高度（带缓存）</summary>
        public static int ScaledButtonHeight => _scaledButtonHeight;

        #endregion
    }
}
