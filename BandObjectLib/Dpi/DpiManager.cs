//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BandObjectLib 
{
  internal static class DpiManager
  {
      public static bool PerMonitorDpiIsSupported
      {
          get
          {
              return QTUtility.LaterThan8_1;
          }
      }

    public static int GetDpiFromPoint(Point pnt)
    {
      int dpiX;
      int dipY;
      return DpiManager.PerMonitorDpiIsSupported && 
             MCR.SUCCEEDED(
                 PInvoke.GetDpiForMonitor(PInvoke.MonitorFromPoint(pnt, 2), 
                     0, out dpiX, out dipY)) ? dpiX : DpiManager.DefaultDpi;
    }

    public static float GetScalingFromPoint(Point pnt)
    {
        return (float) DpiManager.GetDpiFromPoint(pnt) / 96f;
    } 

    public static int GetDpiForWindow(IntPtr hwnd)
    {
      if (DpiManager.PerMonitorDpiIsSupported)
      {
        if (QTUtility.IsWindows10AndLater)
        {
          try
          {
            return PInvoke.GetDpiForWindow(hwnd);
          }
          catch
          {
          }
        }
        int dpiX;
        int dpiY;
        if (MCR.SUCCEEDED(PInvoke.GetDpiForMonitor(PInvoke.MonitorFromWindow(hwnd, 2), 0, out dpiX, out dpiY)))
          return dpiX;
      }
      return DpiManager.DefaultDpi;
    }

    public static float GetScalingForWindow(IntPtr hwnd)
    {
        return (float) DpiManager.GetDpiForWindow(hwnd) / 96f;
    }

    public static float GetScalingForWindow(IWin32Window wnd)
    {
        return (float)DpiManager.GetDpiForWindow(wnd.Handle) / 96f;
    }

    public static int GetDpiForControl(Control control)
    {
        return DpiManager.PerMonitorDpiIsSupported && control.IsHandleCreated
            ? DpiManager.GetDpiForWindow(control.Handle)
            : DpiManager.DefaultDpi;
    }

    public static IObjectWithDpi GetDPIAwareParent(Control control)
    {
      for (Control parent = control.Parent; parent != null; parent = parent.Parent)
      {
        if (parent is IObjectWithDpi )
          return (IObjectWithDpi) parent;
      }
      return (IObjectWithDpi) null;
    }

    public static int DefaultDpi
    {
      get
      {
        if (!DpiManager.PerMonitorDpiIsSupported)
        {
          IntPtr dc = PInvoke.GetDC(IntPtr.Zero);
          try
          {
            return PInvoke.GetDeviceCaps(dc, 88);
          }
          finally
          {
            PInvoke.ReleaseDC(IntPtr.Zero, dc);
          }
        }
        else
        {
          int dpiX;
          int dpiY;
          return MCR.SUCCEEDED(PInvoke.GetDpiForMonitor(PInvoke.MonitorFromPoint(Screen.PrimaryScreen.Bounds.Location, 1), 0, out dpiX, out dpiY)) ? dpiX : 96;
        }
      }
    }

    public static float SystemScaling
    {
        get
        {
            return (float) DpiManager.DefaultDpi / 96f;
        } 
    }

    public static int MaxDpi
    {
      get
      {
        if (!DpiManager.PerMonitorDpiIsSupported)
          return DpiManager.DefaultDpi;
        int maxDpi = 96;
        foreach (Screen allScreen in Screen.AllScreens)
        {
          int dpiX;
          int dpiY;
          if (MCR.SUCCEEDED(PInvoke.GetDpiForMonitor(PInvoke.MonitorFromPoint(allScreen.Bounds.Location, 1), 0, out dpiX, out dpiY)) && maxDpi < dpiX)
            maxDpi = dpiX;
        }
        return maxDpi;
      }
    }

    public static float MaxScaling
    {
        get
        {
            return (float)DpiManager.MaxDpi / 96f;
        }
    } 
  }

  internal class QTUtility
  {
      private static Version osVersion = Environment.OSVersion.Version;

      public static bool LaterThan8_1
      {
          get
          {
              return IsWindows10AndLater || IsWindows8_1;
          }
      }

      public static bool IsWindows8_1
      {
          get
          {
              return QTUtility.osVersion.Major == 6 && QTUtility.osVersion.Minor == 3;
          }
      }

      public static bool IsWindows10AndLater
      {
          get
          {
              if (QTUtility.osVersion.Major >= 10)
                  return true;
              return QTUtility.osVersion.Major == 6 && QTUtility.osVersion.Minor == 4;
          }
      }
      public static bool LaterThan10Beta17666
      {
          get
          {
              if (QTUtility.IsWindows10AndLater)
                  return true;
              return QTUtility.IsWindows10 && QTUtility.osVersion.Build >= 17666;
          }
      }

      private static bool IsWindows10
      {
          get
          {
              if (QTUtility.osVersion.Major >= 10)
                  return true;
              return QTUtility.osVersion.Major == 6 && QTUtility.osVersion.Minor == 4;
          }
      }
  }

  internal class PInvoke
  {

      [DllImport("Shcore.dll")]
      public static extern int GetDpiForMonitor(
          IntPtr hmonitor,
          int dpiType,
          out int dpiX,
          out int dpiY);


      [DllImport("user32.dll")]
      public static extern IntPtr MonitorFromPoint(Point pt, int dwFlags);


      [DllImport("user32.dll")]
      public static extern int GetDpiForWindow(IntPtr hwnd);


      [DllImport("user32.dll")]
      public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);


      [DllImport("gdi32.dll")]
      public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);


      [DllImport("user32.dll")]
      public static extern IntPtr GetDC(IntPtr hWnd);


      [DllImport("user32.dll")]
      public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
  }
}
