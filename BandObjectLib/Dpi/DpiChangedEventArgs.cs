// Decompiled with JetBrains decompiler
// Type: QTTabBarLib.DpiChangedEventArgs
// Assembly: QTTabBar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=78a0cde69b47ca25
// MVID: CF6CE8E6-DE2E-4190-8625-40058473C28C
// Assembly location: D:\java\QTTabBar 2048 Beta2\QTTabBar.dll

using System;
using System.Drawing;

namespace QTTabBarLib
{
  public sealed class DpiChangedEventArgs : EventArgs
  {
    public int OldDpi { get; set; }

    public int NewDpi { get; set; }

    public Rectangle NewBounds { get; set; }
  }
}
