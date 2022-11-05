// Decompiled with JetBrains decompiler
// Type: QTTabBarLib.FreeBitmap
// Assembly: QTTabBar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=78a0cde69b47ca25
// MVID: CF6CE8E6-DE2E-4190-8625-40058473C28C
// Assembly location: D:\java\QTTabBar 2048 Beta2\QTTabBar.dll

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace QTTabBarLib
{
  internal sealed class FreeBitmap : IDisposable
  {
    private FileStream fs;
    private Bitmap bmp;

    public FreeBitmap(string path)
    {
      try
      {
        this.fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        this.bmp = (Bitmap) Image.FromStream((Stream) this.fs, false, false);
      }
      catch
      {
      }
    }

    public void Dispose()
    {
      if (this.bmp != null)
      {
        this.bmp.Dispose();
        this.bmp = (Bitmap) null;
      }
      if (this.fs == null)
        return;
      this.fs.Dispose();
      this.fs = (FileStream) null;
    }

    public Size Size()
    {
        return this.bmp != null ? this.bmp.Size : new Size(0,0);
    } 

    public Bitmap Clone(Rectangle rct, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
    {
      try
      {
        if (this.bmp != null)
          return this.bmp.Clone(rct, pixelFormat);
      }
      catch
      {
      }
      return (Bitmap) null;
    }

    public Bitmap Clone()
    {
      try
      {
        if (this.bmp != null)
          return this.bmp.Clone(new Rectangle(Point.Empty, this.bmp.Size), PixelFormat.Format32bppArgb);
      }
      catch
      {
      }
      return (Bitmap) null;
    }
  }
}
