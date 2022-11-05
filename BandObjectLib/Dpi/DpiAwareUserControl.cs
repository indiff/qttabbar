// Decompiled with JetBrains decompiler
// Type: QTTabBarLib.DpiAwareUserControl
// Assembly: QTTabBar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=78a0cde69b47ca25
// MVID: CF6CE8E6-DE2E-4190-8625-40058473C28C
// Assembly location: D:\java\QTTabBar 2048 Beta2\QTTabBar.dll

using System;
using System.Windows.Forms;

namespace QTTabBarLib
{
  internal class DpiAwareUserControl : UserControl, IDpiAwareObject, IObjectWithDpi
  {
    private bool fNotified;

    public int Dpi { get; set; } = 96;

    public float Scaling => (float) this.Dpi / 96f;

    public void NotifyDpiChanged(int oldDpi, int newDpi)
    {
      this.fNotified = true;
      this.Dpi = newDpi;
      this.OnDpiChanged(oldDpi, newDpi);
    }

    protected virtual void OnDpiChanged(int oldDpi, int newDpi)
    {
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
      base.OnVisibleChanged(e);
      if (this.fNotified || !this.Visible)
        return;
      this.Dpi = DpiManager.GetDpiForWindow(this.Handle);
    }
  }
}
