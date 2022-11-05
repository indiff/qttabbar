using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    internal static class Graphic
    {
        internal static void FillRectangleRTL(
            Graphics g,
            Color color,
            Rectangle rct,
            bool fRtl)
        {
            if (fRtl)
            {
                IntPtr hdc = g.GetHdc();
                try
                {
                    Graphic.FillRectangleRTL(hdc, color, rct);
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }
            else
            {
                using (SolidBrush solidBrush = new SolidBrush(color))
                    g.FillRectangle((Brush)solidBrush, rct);
            }
        }


        public static void FillRectangleRTL(IntPtr hdc, Color color, Rectangle rct)
        {
            IntPtr pen = PInvoke.CreatePen(0, 1, ColorTranslator.ToWin32(color));
            IntPtr solidBrush = PInvoke.CreateSolidBrush(ColorTranslator.ToWin32(color));
            IntPtr hgdiobj1 = PInvoke.SelectObject(hdc, pen);
            IntPtr hgdiobj2 = PInvoke.SelectObject(hdc, solidBrush);
            PInvoke.Rectangle(hdc, rct.Left, rct.Top, rct.Right, rct.Bottom);
            PInvoke.SelectObject(hdc, hgdiobj2);
            PInvoke.SelectObject(hdc, hgdiobj1);
            PInvoke.DeleteObject(solidBrush);
            PInvoke.DeleteObject(pen);
        }
    }
}
