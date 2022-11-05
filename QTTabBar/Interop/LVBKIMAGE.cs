// Decompiled with JetBrains decompiler
// Type: QTTabBarLib.Interop.LVBKIMAGE
// Assembly: QTTabBar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=78a0cde69b47ca25
// MVID: CF6CE8E6-DE2E-4190-8625-40058473C28C
// Assembly location: D:\java\QTTabBar 2048 Beta2\QTTabBar.dll

using System;
using System.Runtime.InteropServices;

namespace QTTabBarLib.Interop
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct LVBKIMAGE
  {
    public int ulFlags;
    public IntPtr hBmp;
    public IntPtr pszImage;
    public int cchImageMax;
    public int xOffset;
    public int yOffset;
  }
}
