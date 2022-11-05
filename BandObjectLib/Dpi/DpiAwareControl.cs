//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano, indiff
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
using System.Windows.Forms;

namespace BandObjectLib 
{
  public class DpiAwareControl : Control, IDpiAwareObject, IObjectWithDpi
  {
    private bool fNotified;

    // public int Dpi = 96  ;

    public int Dpi { get; set; }

    public float Scaling
    {
        get
        {
            return (float) this.Dpi / 96f;
        }
    }

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
      this.fNotified = true;
      int dpi = this.Dpi;
      this.Dpi = DpiManager.GetDpiForWindow(this.Handle);
      if (dpi == this.Dpi)
        return;
      this.OnDpiChanged(dpi, this.Dpi);
    }
  }
}
