using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QTPlugin
{
    public static class DpiScaling
    {
        private static volatile bool _enabled;

        [DllImport("user32.dll", EntryPoint = "GetDpiForWindow")]
        private static extern uint GetDpiForWindowNative(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "GetDpiForSystem")]
        private static extern uint GetDpiForSystemNative();

        public static event EventHandler EnabledChanged;

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                EnabledChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static int GetDpi(IntPtr hwnd)
        {
            uint dpi = 0;
            try
            {
                if (hwnd != IntPtr.Zero)
                    dpi = GetDpiForWindowNative(hwnd);
            }
            catch (EntryPointNotFoundException)
            {
            }

            if (dpi == 0)
            {
                try
                {
                    dpi = GetDpiForSystemNative();
                }
                catch (EntryPointNotFoundException)
                {
                }
            }

            return dpi == 0 ? 96 : (int)dpi;
        }

        public static float GetScale(IntPtr hwnd)
        {
            return GetDpi(hwnd) / 96f;
        }
    }

    public class DpiAwareForm : Form
    {
        private const int WmDpiChanged = 0x02E0;
        private const int WmDpiChangedAfterParent = 0x02E3;
        private float _dpiScale = 1f;
        private bool _scaleChildren = true;

        public float DpiScale => _dpiScale;

        public bool ScaleChildren
        {
            get => _scaleChildren;
            set => _scaleChildren = value;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            AutoScaleMode = AutoScaleMode.None;
            base.OnHandleCreated(e);
            DpiScaling.EnabledChanged -= DpiScaling_EnabledChanged;
            DpiScaling.EnabledChanged += DpiScaling_EnabledChanged;
            UpdateDpi();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DpiScaling.EnabledChanged -= DpiScaling_EnabledChanged;
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WmDpiChanged || m.Msg == WmDpiChangedAfterParent)
                UpdateDpi(false);
        }

        public void UpdateDpi()
        {
            UpdateDpi(true);
        }

        private void UpdateDpi(bool resizeForm)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            float newScale = DpiScaling.Enabled ? DpiScaling.GetScale(Handle) : 1f;
            if (Math.Abs(newScale - _dpiScale) < 0.01f)
                return;

            float scaleFactor = newScale / _dpiScale;
            _dpiScale = newScale;
            if (resizeForm)
                OnDpiChanged(scaleFactor);
            else
                ApplyDpiChanged(scaleFactor, false);

            OnDpiScaleChanged(scaleFactor);
        }

        public virtual void OnDpiChanged(float scaleFactor)
        {
            ApplyDpiChanged(scaleFactor, true);
        }

        protected virtual void OnDpiScaleChanged(float scaleFactor)
        {
        }

        protected int ScaleDpi(int value)
        {
            return (int)Math.Round(value * _dpiScale);
        }

        private void ApplyDpiChanged(float scaleFactor, bool resizeForm)
        {
            if (IsDisposed || Math.Abs(scaleFactor - 1f) < 0.01f)
                return;

            try
            {
                SuspendLayout();
                if (resizeForm)
                {
                    ClientSize = ScaleSize(ClientSize, scaleFactor);
                    Padding = ScalePadding(Padding, scaleFactor);
                    MinimumSize = ScaleSize(MinimumSize, scaleFactor);
                    MaximumSize = ScaleSize(MaximumSize, scaleFactor);
                }

                if (_scaleChildren)
                    ScaleChildControls(this, scaleFactor);
            }
            catch (Exception ex)
            {
                Trace.TraceError("DpiAwareForm.OnDpiChanged failed: {0}", ex);
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        protected virtual void ScaleChildControls(Control parent, float scaleFactor)
        {
            foreach (Control child in parent.Controls)
                child.Scale(new SizeF(scaleFactor, scaleFactor));
        }

        private void DpiScaling_EnabledChanged(object sender, EventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke((MethodInvoker)UpdateDpi);
                }
                catch (InvalidOperationException ex)
                {
                    Trace.TraceError("DpiAwareForm could not schedule DPI update: {0}", ex);
                }
                return;
            }

            UpdateDpi();
        }

        private static int Scale(int value, float scaleFactor)
        {
            return (int)Math.Round(value * scaleFactor);
        }

        private static Size ScaleSize(Size value, float scaleFactor)
        {
            return new Size(Scale(value.Width, scaleFactor), Scale(value.Height, scaleFactor));
        }

        private static Padding ScalePadding(Padding value, float scaleFactor)
        {
            return new Padding(
                Scale(value.Left, scaleFactor),
                Scale(value.Top, scaleFactor),
                Scale(value.Right, scaleFactor),
                Scale(value.Bottom, scaleFactor));
        }
    }
}
