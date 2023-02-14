using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    internal static class Graphic
    {


        public static void FillRectangleRTL(
            IntPtr hdc,
            Color color,
            Rectangle rct,
            bool fRtl,
            bool fWithoutCorner = false)
        {
            if (fRtl)
            {
                if (fWithoutCorner)
                    Graphic.FillRectangleRTLWithoutCorner(hdc, color, rct);
                else
                    Graphic.FillRectangleRTL(hdc, color, rct);
            }
            else
            {
                using (Graphics g = Graphics.FromHdc(hdc))
                {
                    using (SolidBrush sb = new SolidBrush(color))
                    {
                        if (fWithoutCorner)
                            Graphic.FillRectangleWithoutCorners(g, sb, rct);
                        else
                            g.FillRectangle((Brush)sb, rct);
                    }
                }
            }
        }

        public static void FillRectangleRTLWithoutCorner(IntPtr hdc, Color color, Rectangle rct)
        {
            IntPtr pen = PInvoke.CreatePen(0, 1, ColorTranslator.ToWin32(color));
            IntPtr solidBrush = PInvoke.CreateSolidBrush(ColorTranslator.ToWin32(color));
            IntPtr hgdiobj1 = PInvoke.SelectObject(hdc, pen);
            IntPtr hgdiobj2 = PInvoke.SelectObject(hdc, solidBrush);
            PInvoke.Rectangle(hdc, rct.Left + 2, rct.Top, rct.Right - 2, rct.Bottom);
            PInvoke.Rectangle(hdc, rct.Left + 1, rct.Top + 1, rct.Right - 1, rct.Bottom - 1);
            PInvoke.Rectangle(hdc, rct.Left, rct.Top + 2, rct.Right, rct.Bottom - 2);
            PInvoke.SelectObject(hdc, hgdiobj2);
            PInvoke.SelectObject(hdc, hgdiobj1);
            PInvoke.DeleteObject(solidBrush);
            PInvoke.DeleteObject(pen);
        }

        public static void FillRectangleWithoutCorners(Graphics g, SolidBrush sb, Rectangle rct)
        {
            Rectangle rect1 = new Rectangle(rct.X + 2, rct.Y, rct.Width - 4, rct.Height);
            g.FillRectangle((Brush)sb, rect1);
            Rectangle rect2 = new Rectangle(rct.X + 1, rct.Y + 1, rct.Width - 2, rct.Height - 2);
            g.FillRectangle((Brush)sb, rect2);
            Rectangle rect3 = new Rectangle(rct.X, rct.Y + 2, rct.Width, rct.Height - 4);
            g.FillRectangle((Brush)sb, rect3);
        }

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

        public static void DrawLineRTL(IntPtr hdc, Color color, Point pntStart, Point pntEnd)
        {
            IntPtr pen = PInvoke.CreatePen(0, 1, ColorTranslator.ToWin32(color));
            IntPtr hgdiobj = PInvoke.SelectObject(hdc, pen);
            PInvoke.MoveToEx(hdc, pntStart.X, pntStart.Y, IntPtr.Zero);
            PInvoke.LineTo(hdc, pntEnd.X, pntEnd.Y);
            PInvoke.SelectObject(hdc, hgdiobj);
            PInvoke.DeleteObject(pen);
        }

        public static int ScaleBy(float windowScaling, int tabHeight)
        {
            return QTUtility2.Round(windowScaling * (float)tabHeight);
        }

        public static T SelectValueByScaling<T>(float scaling, T value96, T value120, T value144)
        {
            if ((double)scaling <= 1.0)
                return value96;
            return (double)scaling > 1.25 ? value144 : value120;
        }

        public static Padding Translate(Padding pad)
        {
            return QTUtility.RightToLeft ? new Padding(pad.Right, pad.Top, pad.Left, pad.Bottom) : pad;
        }

        public static Font CreateDefaultFont()
        {
            try
            {
                return new Font(QTUtility.DefaultFontName, 
                    9f,
                    FontStyle.Regular, 
                    GraphicsUnit.Point, (byte)0);
            }
            catch (Exception ex)
            {
            }
            return (Font)null;
        }

        public static Font CreateDefaultFont(float point, FontStyle style = FontStyle.Regular)
        {
            try
            {
                return new Font(QTUtility.DefaultFontName, point, style, GraphicsUnit.Point, (byte)0);
            }
            catch (Exception ex)
            {
            }
            return (Font)null;
        }
    }
}
