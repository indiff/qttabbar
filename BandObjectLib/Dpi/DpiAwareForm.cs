// Decompiled with JetBrains decompiler
// Type: QTTabBarLib.DpiAwareForm
// Assembly: QTTabBar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=78a0cde69b47ca25
// MVID: CF6CE8E6-DE2E-4190-8625-40058473C28C
// Assembly location: D:\java\QTTabBar 2048 Beta2\QTTabBar.dll

using QTTabBarLib.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QTTabBarLib
{
  public class DpiAwareForm : Form, IObjectWithDpi
  {
    private bool fFirstDpiChangeMessage = true;

    protected void ScaleBeforeHandleIsCreated(Point pntStartUp)
    {
      if (DpiManager.DefaultDpi == 96)
        return;
      this.ScaleBeforeHandleIsCreatedCore();
    }

    protected void ScaleBeforeHandleIsCreatedCore()
    {
      if (this.ScaledByFont || DpiManager.PerMonitorDpiIsSupported || this.DesignMode)
        return;
      float num = (float) DpiManager.DefaultDpi / 96f;
      this.MaximumSize = Graphic.ScaleBy(num, this.MaximumSize);
      this.Scale(new SizeF(num, num));
      this.MinimumSize = Graphic.ScaleBy(num, this.MinimumSize);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
      base.OnHandleCreated(e);
      if (this.DesignMode || !DpiManager.PerMonitorDpiIsSupported)
        return;
      this.Dpi = DpiManager.GetDpiForWindow(this.Handle);
      if (this.Dpi == 96)
        return;
      if (this.ScaledByFont)
      {
        this.UpdateFont();
      }
      else
      {
        float scaling = this.Scaling;
        this.MaximumSize = Graphic.ScaleBy(scaling, this.MaximumSize);
        this.Scale(new SizeF(scaling, scaling));
        this.MinimumSize = Graphic.ScaleBy(scaling, this.MinimumSize);
      }
    }

    protected virtual void OnDpiChanged(DpiChangedEventArgs e)
    {
      if (this.DesignMode)
        return;
      this.SuspendLayout();
      if (this.ScaledByFont)
      {
        this.UpdateFont();
      }
      else
      {
        float num = (float) e.NewDpi / (float) e.OldDpi;
        SizeF s = new SizeF(num, num);
        Dictionary<Control, AnchorStyles> dic = new Dictionary<Control, AnchorStyles>();
        this.PreprocessOnDpiChange(dic, e, (Control) this);
        this.UpdateFont();
        Size minimumSize = this.MinimumSize;
        Size maximumSize = this.MaximumSize;
        this.MinimumSize = Size.Empty;
        this.MaximumSize = Size.Empty;
        this.Bounds = e.NewBounds;
        this.Controls.Cast<Control>().ForEach<Control>((Action<Control>) (c => c.Scale(s)));
        this.ReanchorControlsInTabPage(dic);
        ReflectionUtil.ScaleDockPadding((ScrollableControl) this, num);
        this.MaximumSize = Graphic.ScaleBy(num, maximumSize);
        this.MinimumSize = Graphic.ScaleBy(num, minimumSize);
        this.ResumeLayout();
      }
    }

    private void UpdateFont() => this.Font = new Font(this.Font.Name, Graphic.GetFontPixelSize(this.AbsoluteFontSize, this.Scaling), this.Font.Style, GraphicsUnit.Pixel);

    protected override unsafe void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        case 131:
          if (m.WParam != IntPtr.Zero && !DpiManager.PerMonitorDpiIsSupported)
          {
            NCCALCSIZE_PARAMS* lparam = (NCCALCSIZE_PARAMS*) (void*) m.LParam;
            base.WndProc(ref m);
            if (DpiManager.DefaultDpi <= 96 || DpiManager.DefaultDpi <= this.Dpi || this.FormBorderStyle == FormBorderStyle.None)
              return;
            int systemMetrics;
            int num1 = QMath.Round((float) (systemMetrics = PInvoke.GetSystemMetrics(4)) / ((float) DpiManager.DefaultDpi / (float) this.Dpi));
            int num2 = systemMetrics - num1;
            RECT rctWindowNew = lparam->rctWindowNew;
            rctWindowNew.top -= num2;
            lparam->rctWindowNew = rctWindowNew;
            return;
          }
          break;
        case 736:
          int dpi = this.Dpi;
          this.Dpi = (int) (long) m.WParam & (int) ushort.MaxValue;
          Rectangle rectangle = ((RECT*) (void*) m.LParam)->ToRectangle();
          if (this.fFirstDpiChangeMessage && !DpiManager.PerMonitorDpiIsSupported && dpi != DpiManager.DefaultDpi && (this.FormBorderStyle == FormBorderStyle.Sizable || this.FormBorderStyle == FormBorderStyle.SizableToolWindow))
          {
            this.Dpi = DpiManager.GetDpiFromPoint(this.Location);
            rectangle = new Rectangle(this.Location, Graphic.ScaleBy((float) dpi / (float) this.Dpi, this.Size));
          }
          this.fFirstDpiChangeMessage = false;
          bool visible = this.Visible;
          if (visible)
            WindowUtil.SetRedraw(this.Handle, false);
          try
          {
            this.OnDpiChanged(new DpiChangedEventArgs()
            {
              OldDpi = dpi,
              NewDpi = this.Dpi,
              NewBounds = rectangle
            });
            return;
          }
          finally
          {
            if (visible)
            {
              WindowUtil.SetRedraw(this.Handle, true);
              this.Invalidate();
            }
          }
      }
      base.WndProc(ref m);
    }

    private void PreprocessOnDpiChange(
      Dictionary<Control, AnchorStyles> dic,
      DpiChangedEventArgs e,
      Control c)
    {
      if (c is IDpiAwareObject dpiAwareObject)
        dpiAwareObject.NotifyDpiChanged(e.OldDpi, e.NewDpi);
      this.OnPreprocessControlOnDpiChange(e, c);
      if (c.Parent is TabPage && c.Dock == DockStyle.None && (c.Anchor & (AnchorStyles.Bottom | AnchorStyles.Right)) != AnchorStyles.None)
      {
        dic[c] = c.Anchor;
        c.Anchor &= ~(AnchorStyles.Bottom | AnchorStyles.Right);
      }
      c.Controls.Cast<Control>().ForEach<Control>((Action<Control>) (cc => this.PreprocessOnDpiChange(dic, e, cc)));
    }

    protected virtual void OnPreprocessControlOnDpiChange(DpiChangedEventArgs e, Control c)
    {
    }

    private void ReanchorControlsInTabPage(Dictionary<Control, AnchorStyles> dic)
    {
      foreach (KeyValuePair<Control, AnchorStyles> keyValuePair in dic)
        keyValuePair.Key.Anchor = keyValuePair.Value;
    }

    public int Dpi { get; set; } = DpiManager.DefaultDpi;

    public float Scaling => (float) this.Dpi / 96f;

    public bool ScaledByFont => this.AutoScaleMode == AutoScaleMode.Font;

    protected virtual float AbsoluteFontSize { get; } = 9f;
  }
}
