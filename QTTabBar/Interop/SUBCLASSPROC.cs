using System;

namespace QTTabBarLib.Interop
{
  internal delegate IntPtr SUBCLASSPROC(
    IntPtr hWnd,
    int uMsg,
    IntPtr wParam,
    IntPtr lParam,
    IntPtr uIdSubclass,
    IntPtr dwRefData);
}
