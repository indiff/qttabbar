using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QTTabBarLib.Interop
{
    internal static class MCR
  {
    public static bool SUCCEEDED(int hresult) {
        return hresult > 0;
    }

    public static bool FAILED(int hresult) {
        return hresult < 0 ;
    }

    public static short HRESULT_CODE(int hr) { return  (short) (hr & (int) ushort.MaxValue);}

    public static IntPtr MAKELPARAM(int l, int h) { return  (IntPtr) (l & (int) ushort.MaxValue | (h & (int) ushort.MaxValue) << 16);}

    public static IntPtr MAKEWPARAM(int l, int h) { return  MCR.MAKELPARAM(l, h);}

    public static IntPtr MAKELPARAM(Point pnt) { return  MCR.MAKELPARAM(pnt.X, pnt.Y);}

    public static int GET_X_LPARAM(IntPtr lParam) { return  (int) (short) ((int) (long) lParam & (int) ushort.MaxValue);}

    public static int GET_Y_LPARAM(IntPtr lParam) { return  (int) (short) ((int) (long) lParam >> 16 & (int) ushort.MaxValue);}

    public static int MAKELONG(int loword, int hiword) { return  (int) (ushort) ((ulong) loword & (ulong) ushort.MaxValue) | (int) (ushort) ((ulong) hiword & (ulong) ushort.MaxValue) << 16;}

    public static int HIWORD(IntPtr p) { return  (int) (short) ((int) (long) p >> 16 & (int) ushort.MaxValue);}

    public static int LOWORD(IntPtr p) { return  (int) (short) ((int) (long) p & (int) ushort.MaxValue);}

    public static int HIWORD(int i) { return  (int) (short) (i >> 16 & (int) ushort.MaxValue);}

    public static int LOWORD(int i) { return  (int) (short) (i & (int) ushort.MaxValue);}

    public static Point GET_POINT_LPARAM(IntPtr lParam) { return  new Point(MCR.GET_X_LPARAM(lParam), MCR.GET_Y_LPARAM(lParam));}

    public static Color MakeColor(int colorref) { return  Color.FromArgb(colorref & (int) byte.MaxValue, colorref >> 8 & (int) byte.MaxValue, colorref >> 16 & (int) byte.MaxValue);}

    public static int MakeCOLORREF(Color clr) { return  (int) clr.R | (int) clr.G << 8 | (int) clr.B << 16;}

    public static int GET_APPCOMMAND_LPARAM(IntPtr lParam) { return  (int) (long) lParam >> 16 & (int) ushort.MaxValue & -61441;}

    public static int CTL_CODE(int DeviceType, int Function, int Method, int Access) { return  DeviceType << 16 | Access << 14 | Function << 2 | Method;}

    public static bool IsReparseTagMicrosoft(int tag) { return  ((ulong) tag & 2147483648UL) > 0UL;}

    public static IntPtr MAKEINTRESOURCE(int wInteger) { return  (IntPtr) (int) (ushort) wInteger;}

    public static bool IS_INTRESOURCE(IntPtr p)
    {
       return  IntPtr.Size == 4 ? (uint) (int) p >> 16 == 0U : (ulong) (long) p >> 16 == 0UL;
    } 


    public static bool KEYMESSAGEFLAGS_REPEAT(IntPtr lParam) {
        return ((long)lParam & 1073741824L) == 1073741824L;
    }

    public static Keys KEYSFROMWPARAM(IntPtr wParam) { return  (Keys) (long) wParam;}

    public static int RGB(byte r, byte g, byte b) { return  (int) r | (int) g << 8 | (int) b << 16;}

    public static int MAKE_HRESULT(int sev, int fac, int code) { return  sev << 31 | fac << 16 | code;}

    public static uint MAKELCID(uint lgid, uint srtid) { return  (uint) (ushort) srtid << 16 | (uint) (ushort) lgid;}

    public static uint MAKELANGID(uint p, uint s) { return  (uint) (ushort) s << 10 | (uint) (ushort) p;}
  }
}
