//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class QTabControl : Control {
        private Bitmap bmpCloseBtn_Cold;
        private Bitmap bmpCloseBtn_ColdAlt;
        private Bitmap bmpCloseBtn_Hot;
        private Bitmap bmpCloseBtn_Pressed;
        private Bitmap bmpFolIconBG;
        private Bitmap bmpLocked;
        private SolidBrush brshActive;
        private SolidBrush brshInactv;
        private Color[] colorSet;
        private IContainer components;
        private QTabItem draggingTab;
        private bool fActiveTxtBold;
        private bool fAutoSubText;
        private bool fCloseBtnOnHover;
        private bool fDrawCloseButton;
        private bool fDrawFolderImg;
        private bool fDrawShadow;
        private bool fForceClassic;
        private bool fLimitSize;
        private bool fNeedToDrawUpDown;
        private bool fNowMouseIsOnCloseBtn;
        private bool fNowMouseIsOnIcon;
        private bool fNowShowCloseBtnAlt;
        private bool fNowTabContextMenuStripShowing;
        private Font fnt_Underline;
        private Font fntBold;
        private Font fntBold_Underline;
        private Font fntDriveLetter;
        private Font fntSubText;
        private bool fOncePainted;
        internal const float FONTSIZE_DIFF = 0.75f;
        private bool fRedrawSuspended;
        private bool fShowSubDirTip;
        private bool fSubDirShown;
        private bool fSuppressDoubleClick;
        private bool fSuppressMouseUp;
        private QTabItem hotTab;
        private int iCurrentRow;
        private int iFocusedTabIndex = -1;
        private int iMultipleType;
        private int iPointedChanged_LastRaisedIndex = -2;
        private int iPseudoHotIndex = -1;
        private int iScrollClickedCount;
        private int iScrollWidth;
        private int iSelectedIndex;
        private int iTabIndexOfSubDirShown = -1;
        private int iTabMouseOnButtonsIndex = -1;
        private Size itemSize = new Size(100, 0x18);
        private int iToolTipIndex = -1;
        private int maxAllowedTabWidth = 10;
        private int minAllowedTabWidth = 10;
        private QTabItem selectedTabPage;
        private StringFormat sfTypoGraphic;
        private TabSizeMode sizeMode;
        private Padding sizingMargin;
        private Bitmap[] tabImages;
        private QTabCollection tabPages;
        private StringAlignment tabTextAlignment;
        private Timer timerSuppressDoubleClick;
        private ToolTip toolTip;
        private UpDown upDown;
        private const int UPDOWN_WIDTH = 0x24;
        private VisualStyleRenderer vsr_LHot;
        private VisualStyleRenderer vsr_LNormal;
        private VisualStyleRenderer vsr_LPressed;
        private VisualStyleRenderer vsr_MHot;
        private VisualStyleRenderer vsr_MNormal;
        private VisualStyleRenderer vsr_MPressed;
        private VisualStyleRenderer vsr_RHot;
        private VisualStyleRenderer vsr_RNormal;
        private VisualStyleRenderer vsr_RPressed;

        public event QTabCancelEventHandler CloseButtonClicked;
        public event QTabCancelEventHandler Deselecting;
        public event ItemDragEventHandler ItemDrag;
        public event QTabCancelEventHandler PointedTabChanged;
        public event QEventHandler RowCountChanged;
        public event EventHandler SelectedIndexChanged;
        public event QTabCancelEventHandler Selecting;
        public event QTabCancelEventHandler TabCountChanged;
        public event QTabCancelEventHandler TabIconMouseDown;

        public QTabControl() {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            components = new Container();
            tabPages = new QTabCollection(this);
            BackColor = Color.Transparent;
            sfTypoGraphic = StringFormat.GenericTypographic;
            sfTypoGraphic.FormatFlags |= StringFormatFlags.NoWrap;
            sfTypoGraphic.Trimming = StringTrimming.EllipsisCharacter;
            colorSet = new Color[] {
                Config.Skin.TabTextActiveColor,
                Config.Skin.TabTextInactiveColor,
                Config.Skin.TabTextHotColor,
                Config.Skin.TabShadActiveColor,
                Config.Skin.TabShadInactiveColor,
                Config.Skin.TabShadHotColor
            };
            brshActive = new SolidBrush(colorSet[0]);
            brshInactv = new SolidBrush(colorSet[1]);
            timerSuppressDoubleClick = new Timer(components);
            timerSuppressDoubleClick.Interval = SystemInformation.DoubleClickTime + 100;
            timerSuppressDoubleClick.Tick += timerSuppressDoubleClick_Tick;
            if(VisualStyleRenderer.IsSupported) {
                InitializeRenderer();
            }
        }

        private bool CalculateItemRectangle() {
            int x = 0;
            int count = tabPages.Count;
            if(sizeMode == TabSizeMode.Fixed) {
                for(int i = 0; i < count; i++) {
                    tabPages[i].TabBounds = new Rectangle(x, 0, itemSize.Width, itemSize.Height);
                    tabPages[i].Edge = 0;
                    x += itemSize.Width;
                }
            }
            else {
                int width;
                if(fLimitSize) {
                    for(int j = 0; j < count; j++) {
                        width = tabPages[j].TabBounds.Width;
                        if(width > maxAllowedTabWidth) {
                            width = maxAllowedTabWidth;
                        }
                        if(width < minAllowedTabWidth) {
                            width = minAllowedTabWidth;
                        }
                        tabPages[j].TabBounds = new Rectangle(x, 0, width, itemSize.Height);
                        tabPages[j].Edge = 0;
                        x += width;
                    }
                }
                else {
                    for(int k = 0; k < count; k++) {
                        width = tabPages[k].TabBounds.Width;
                        tabPages[k].TabBounds = new Rectangle(x, 0, width, itemSize.Height);
                        tabPages[k].Edge = 0;
                        x += width;
                    }
                }
            }
            if(tabPages.Count > 1) {
                tabPages[0].Edge = Edges.Left;
                tabPages[tabPages.Count - 1].Edge = Edges.Right;
            }
            return (x > (Width - 0x24));
        }

        private void CalculateItemRectangle_MultiRows() {
            int x = 0;
            int count = tabPages.Count;
            int width = Width;
            int num4 = itemSize.Width;
            int height = itemSize.Height;
            int num6 = height - 3;
            int num7 = 0;
            int num8 = 0;
            if(sizeMode == TabSizeMode.Fixed) {
                for(int i = 0; i < count; i++) {
                    if((x + num4) > width) {
                        num7++;
                        x = 0;
                    }
                    tabPages[i].TabBounds = new Rectangle(x, num6 * num7, num4, height);
                    tabPages[i].Row = num7;
                    if(x == 0) {
                        tabPages[i].Edge = Edges.Left;
                    }
                    else if((i == (count - 1)) || (((x + num4) + num4) > width)) {
                        tabPages[i].Edge = Edges.Right;
                    }
                    else {
                        tabPages[i].Edge = 0;
                    }
                    x += num4;
                    if(i == iSelectedIndex) {
                        num8 = num7;
                    }
                }
            }
            else {
                int maxTabWidth;
                if(fLimitSize) {
                    for(int j = 0; j < count; j++) {
                        maxTabWidth = tabPages[j].TabBounds.Width;
                        if(maxTabWidth > maxAllowedTabWidth) {
                            maxTabWidth = maxAllowedTabWidth;
                        }
                        if(maxTabWidth < minAllowedTabWidth) {
                            maxTabWidth = minAllowedTabWidth;
                        }
                        if((x + maxTabWidth) > width) {
                            num7++;
                            x = 0;
                        }
                        tabPages[j].TabBounds = new Rectangle(x, num6 * num7, maxTabWidth, height);
                        tabPages[j].Row = num7;
                        if(x == 0) {
                            tabPages[j].Edge = Edges.Left;
                        }
                        else if(j == (count - 1)) {
                            tabPages[j].Edge = Edges.Right;
                        }
                        else {
                            int minTabWidth = tabPages[j + 1].TabBounds.Width;
                            if(minTabWidth > maxAllowedTabWidth) {
                                minTabWidth = maxAllowedTabWidth;
                            }
                            if(minTabWidth < minAllowedTabWidth) {
                                minTabWidth = minAllowedTabWidth;
                            }
                            if(((x + maxTabWidth) + minTabWidth) > width) {
                                tabPages[j].Edge = Edges.Right;
                            }
                            else {
                                tabPages[j].Edge = 0;
                            }
                        }
                        x += maxTabWidth;
                        if(j == iSelectedIndex) {
                            num8 = num7;
                        }
                    }
                }
                else {
                    for(int k = 0; k < count; k++) {
                        maxTabWidth = tabPages[k].TabBounds.Width;
                        if((x + maxTabWidth) > width) {
                            num7++;
                            x = 0;
                        }
                        tabPages[k].TabBounds = new Rectangle(x, num6 * num7, maxTabWidth, height);
                        tabPages[k].Row = num7;
                        if(x == 0) {
                            tabPages[k].Edge = Edges.Left;
                        }
                        else if(k == (count - 1)) {
                            tabPages[k].Edge = Edges.Right;
                        }
                        else {
                            int num14 = tabPages[k + 1].TabBounds.Width;
                            if(((x + maxTabWidth) + num14) > width) {
                                tabPages[k].Edge = Edges.Right;
                            }
                            else {
                                tabPages[k].Edge = 0;
                            }
                        }
                        x += maxTabWidth;
                        if(k == iSelectedIndex) {
                            num8 = num7;
                        }
                    }
                }
            }
            if((num7 != 0) && (iMultipleType == 1)) {
                int num15 = num7 - num8;
                if(num15 > 0) {
                    for(int m = 0; m < count; m++) {
                        QTabItem base2 = tabPages[m];
                        Rectangle tabBounds = base2.TabBounds;
                        if(base2.Row > num8) {
                            base2.Row -= num8 + 1;
                            tabBounds.Y = base2.Row * num6;
                            base2.TabBounds = tabBounds;
                        }
                        else {
                            tabBounds.Y += num15 * num6;
                            base2.TabBounds = tabBounds;
                            base2.Row += num15;
                        }
                    }
                }
            }
            if(num7 != iCurrentRow) {
                iCurrentRow = num7;
                if(RowCountChanged != null) {
                    RowCountChanged(this, new QEventArgs(iCurrentRow + 1));
                }
            }
        }

        private bool ChangeSelection(QTabItem tabToSelect, int index) {
            if(((Deselecting != null) && (this.iSelectedIndex > -1)) && (this.iSelectedIndex < tabPages.Count)) {
                QTabCancelEventArgs e = new QTabCancelEventArgs(tabPages[this.iSelectedIndex], this.iSelectedIndex, false, TabControlAction.Deselecting);
                Deselecting(this, e);
            }
            int iSelectedIndex = this.iSelectedIndex;
            QTabItem selectedTabPage = this.selectedTabPage;
            this.iSelectedIndex = index;
            this.selectedTabPage = tabToSelect;
            if(Selecting != null) {
                QTabCancelEventArgs args2 = new QTabCancelEventArgs(tabToSelect, index, false, TabControlAction.Selecting);
                Selecting(this, args2);
                if(args2.Cancel) {
                    this.iSelectedIndex = iSelectedIndex;
                    this.selectedTabPage = selectedTabPage;
                    return false;
                }
            }
            if(fNeedToDrawUpDown) {
                if((tabToSelect.TabBounds.X + iScrollWidth) < 0) {
                    iScrollWidth = -tabToSelect.TabBounds.X;
                    iScrollClickedCount = index;
                }
                else if((tabToSelect.TabBounds.X + iScrollWidth) > (Width - 0x24)) {
                    while((tabToSelect.TabBounds.Right + iScrollWidth) > Width) {
                        OnUpDownClicked(true, true);
                    }
                }
            }
            Refresh();
            if(SelectedIndexChanged != null) {
                SelectedIndexChanged(this, new EventArgs());
            }
            iFocusedTabIndex = -1;
            return true;
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            if(brshActive != null) {
                brshActive.Dispose();
                brshActive = null;
            }
            if(brshInactv != null) {
                brshInactv.Dispose();
                brshInactv = null;
            }
            if(sfTypoGraphic != null) {
                sfTypoGraphic.Dispose();
                sfTypoGraphic = null;
            }
            if(bmpLocked != null) {
                bmpLocked.Dispose();
                bmpLocked = null;
            }
            if(bmpCloseBtn_Cold != null) {
                bmpCloseBtn_Cold.Dispose();
                bmpCloseBtn_Cold = null;
            }
            if(bmpCloseBtn_Hot != null) {
                bmpCloseBtn_Hot.Dispose();
                bmpCloseBtn_Hot = null;
            }
            if(bmpCloseBtn_Pressed != null) {
                bmpCloseBtn_Pressed.Dispose();
                bmpCloseBtn_Pressed = null;
            }
            if(bmpCloseBtn_ColdAlt != null) {
                bmpCloseBtn_ColdAlt.Dispose();
            }
            if(bmpFolIconBG != null) {
                bmpFolIconBG.Dispose();
                bmpFolIconBG = null;
            }
            if(fnt_Underline != null) {
                fnt_Underline.Dispose();
                fnt_Underline = null;
            }
            if(fntBold != null) {
                fntBold.Dispose();
                fntBold = null;
            }
            if(fntBold_Underline != null) {
                fntBold_Underline.Dispose();
                fntBold_Underline = null;
            }
            if(fntSubText != null) {
                fntSubText.Dispose();
                fntSubText = null;
            }
            if(fntDriveLetter != null) {
                fntDriveLetter.Dispose();
                fntDriveLetter = null;
            }
            foreach(QTabItem base2 in tabPages) {
                if(base2 != null) {
                    base2.OnClose();
                }
            }
            base.Dispose(disposing);
        }

        private void DrawBackground(Graphics g, bool bSelected, bool fHot, Rectangle rctItem, Edges edges, bool fVisualStyle, int index) {
            if(!fVisualStyle) {
                int num = bSelected ? 0 : 1;
                if(tabImages == null) {
                    g.FillRectangle(SystemBrushes.Control, rctItem);
                    g.DrawLine(SystemPens.ControlLightLight, new Point(rctItem.X + 2, rctItem.Y), new Point(((rctItem.X + rctItem.Width) - 2) - num, rctItem.Y));
                    g.DrawLine(SystemPens.ControlLightLight, new Point(rctItem.X + 2, rctItem.Y), new Point(rctItem.X, rctItem.Y + 2));
                    g.DrawLine(SystemPens.ControlLightLight, new Point(rctItem.X, rctItem.Y + 2), new Point(rctItem.X, (rctItem.Y + rctItem.Height) - 1));
                    g.DrawLine(SystemPens.ControlDarkDark, new Point((rctItem.X + rctItem.Width) - num, rctItem.Y + 2), new Point((rctItem.X + rctItem.Width) - num, (rctItem.Y + rctItem.Height) - 1));
                    g.DrawLine(SystemPens.ControlDark, new Point(((rctItem.X + rctItem.Width) - num) - 1, rctItem.Y + 1), new Point(((rctItem.X + rctItem.Width) - num) - 1, (rctItem.Y + rctItem.Height) - 1));
                    g.DrawLine(SystemPens.ControlDarkDark, new Point(((rctItem.X + rctItem.Width) - num) - 1, rctItem.Y + 1), new Point((rctItem.X + rctItem.Width) - num, rctItem.Y + 2));
                    if(bSelected) {
                        Pen pen = new Pen(colorSet[2], 2f);
                        g.DrawLine(pen, new Point(rctItem.X, (rctItem.Y + rctItem.Height) - 1), new Point((rctItem.X + rctItem.Width) + 1, (rctItem.Y + rctItem.Height) - 1));
                        pen.Dispose();
                    }
                }
                else {
                    Bitmap bitmap;
                    if(bSelected) {
                        bitmap = tabImages[0];
                    }
                    else if(fHot || (iPseudoHotIndex == index)) {
                        bitmap = tabImages[2];
                    }
                    else {
                        bitmap = tabImages[1];
                    }
                    if(bitmap != null) {
                        int left = sizingMargin.Left;
                        int top = sizingMargin.Top;
                        int right = sizingMargin.Right;
                        int bottom = sizingMargin.Bottom;
                        int vertical = sizingMargin.Vertical;
                        int horizontal = sizingMargin.Horizontal;
                        Rectangle[] rectangleArray = new Rectangle[] { new Rectangle(rctItem.X, rctItem.Y, left, top), new Rectangle(rctItem.X + left, rctItem.Y, rctItem.Width - horizontal, top), new Rectangle(rctItem.Right - right, rctItem.Y, right, top), new Rectangle(rctItem.X, rctItem.Y + top, left, rctItem.Height - vertical), new Rectangle(rctItem.X + left, rctItem.Y + top, rctItem.Width - horizontal, rctItem.Height - vertical), new Rectangle(rctItem.Right - right, rctItem.Y + top, right, rctItem.Height - vertical), new Rectangle(rctItem.X, rctItem.Bottom - bottom, left, bottom), new Rectangle(rctItem.X + left, rctItem.Bottom - bottom, rctItem.Width - horizontal, bottom), new Rectangle(rctItem.Right - right, rctItem.Bottom - bottom, right, bottom) };
                        Rectangle[] rectangleArray2 = new Rectangle[9];
                        int width = bitmap.Width;
                        int height = bitmap.Height;
                        rectangleArray2[0] = new Rectangle(0, 0, left, top);
                        rectangleArray2[1] = new Rectangle(left, 0, width - horizontal, top);
                        rectangleArray2[2] = new Rectangle(width - right, 0, right, top);
                        rectangleArray2[3] = new Rectangle(0, top, left, height - vertical);
                        rectangleArray2[4] = new Rectangle(left, top, width - horizontal, height - vertical);
                        rectangleArray2[5] = new Rectangle(width - right, top, right, height - vertical);
                        rectangleArray2[6] = new Rectangle(0, height - bottom, left, bottom);
                        rectangleArray2[7] = new Rectangle(left, height - bottom, width - horizontal, bottom);
                        rectangleArray2[8] = new Rectangle(width - right, height - bottom, right, bottom);
                        for(int i = 0; i < 9; i++) {
                            g.DrawImage(bitmap, rectangleArray[i], rectangleArray2[i], GraphicsUnit.Pixel);
                        }
                    }
                }
            }
            else {
                VisualStyleRenderer renderer;
                if(!bSelected) {
                    if(!fHot && (iPseudoHotIndex != index)) {
                        Edges edges4 = edges;
                        if(edges4 == Edges.Left) {
                            renderer = vsr_LNormal;
                        }
                        else if(edges4 == Edges.Right) {
                            renderer = vsr_RNormal;
                        }
                        else {
                            renderer = vsr_MNormal;
                        }
                    }
                    else {
                        Edges edges3 = edges;
                        if(edges3 == Edges.Left) {
                            renderer = vsr_LHot;
                        }
                        else if(edges3 == Edges.Right) {
                            renderer = vsr_RHot;
                        }
                        else {
                            renderer = vsr_MHot;
                        }
                    }
                }
                else {
                    Edges edges2 = edges;
                    if(edges2 == Edges.Left) {
                        renderer = vsr_LPressed;
                    }
                    else if(edges2 == Edges.Right) {
                        renderer = vsr_RPressed;
                    }
                    else {
                        renderer = vsr_MPressed;
                    }
                    renderer.DrawBackground(g, rctItem);
                    return;
                }
                renderer.DrawBackground(g, rctItem);
            }
        }

        private static void DrawDriveLetter(Graphics g, string str, Font fnt, Rectangle rctFldImg, bool fSelected) {
            Rectangle layoutRectangle = new Rectangle(rctFldImg.X + 7, rctFldImg.Y + 6, 0x10, 0x10);
            using(SolidBrush brush = new SolidBrush(QTUtility2.MakeModColor(fSelected ? Config.Skin.TabShadActiveColor : Config.Skin.TabShadInactiveColor))) {
                Rectangle rectangle2 = layoutRectangle;
                rectangle2.Offset(1, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(-2, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(1, -1);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(0, 2);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(1, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(0, -2);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(-2, 0);
                g.DrawString(str, fnt, brush, rectangle2);
                rectangle2.Offset(0, 2);
                g.DrawString(str, fnt, brush, rectangle2);
                brush.Color = fSelected ? Config.Skin.TabTextActiveColor : Config.Skin.TabTextInactiveColor;
                g.DrawString(str, fnt, brush, layoutRectangle);
            }
        }

        private void DrawTab(Graphics g, Rectangle itemRct, int index, QTabItem tabHot, bool fVisualStyle) {
            Rectangle rectangle2;
            Rectangle rctItem = rectangle2 = itemRct;
            QTabItem base2 = tabPages[index];
            bool bSelected = iSelectedIndex == index;
            bool fHot = base2 == tabHot;
            rectangle2.X += 2;
            if(bSelected) {
                rctItem.Width += 4;
            }
            else {
                rctItem.X += 2;
                rctItem.Y += 2;
                rctItem.Height -= 2;
                rectangle2.Y += 2;
            }
            DrawBackground(g, bSelected, fHot, rctItem, base2.Edge, fVisualStyle, index);
            int num = (rctItem.Height - 0x10) / 2;
            if(fDrawFolderImg && QTUtility.ImageListGlobal.Images.ContainsKey(base2.ImageKey)) {
                Rectangle rect = new Rectangle(rctItem.X + (bSelected ? 7 : 5), rctItem.Y + num, 0x10, 0x10);
                rectangle2.X += 0x18;
                rectangle2.Width -= 0x18;
                if((fNowMouseIsOnIcon && (iTabMouseOnButtonsIndex == index)) || (iTabIndexOfSubDirShown == index)) {
                    if(fSubDirShown && (iTabIndexOfSubDirShown == index)) {
                        rect.X++;
                        rect.Y++;
                    }
                    if(bmpFolIconBG == null) {
                        bmpFolIconBG = Resources_Image.imgFolIconBG;
                    }
                    g.DrawImage(bmpFolIconBG, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4));
                }
                g.DrawImage(QTUtility.ImageListGlobal.Images[base2.ImageKey], rect);
                if(Config.Tabs.ShowDriveLetters) {
                    string pathInitial = base2.PathInitial;
                    if(pathInitial.Length > 0) {
                        DrawDriveLetter(g, pathInitial, fntDriveLetter, rect, bSelected);
                    }
                }
            }
            else {
                rectangle2.X += 4;
                rectangle2.Width -= 4;
            }
            if(base2.TabLocked) {
                Rectangle rectangle4 = new Rectangle(rctItem.X + (bSelected ? 6 : 4), rctItem.Y + num, 9, 11);
                if(fDrawFolderImg) {
                    rectangle4.X += 9;
                    rectangle4.Y += 5;
                }
                else {
                    rectangle4.Y += 2;
                    rectangle2.X += 10;
                    rectangle2.Width -= 10;
                }
                if(bmpLocked == null) {
                    bmpLocked = Resources_Image.imgLocked;
                }
                g.DrawImage(bmpLocked, rectangle4);
            }
            bool flag3 = base2.Comment.Length > 0;
            if((fDrawCloseButton && !fCloseBtnOnHover) && !fNowShowCloseBtnAlt) {
                rectangle2.Width -= 15;
            }
            float num2 = flag3 ? ((base2.TitleTextSize.Width + base2.SubTitleTextSize.Width) + 4f) : (base2.TitleTextSize.Width + 2f);
            float num3 = Math.Max(((rectangle2.Height - base2.TitleTextSize.Height) / 2f), 0f);
            float num4 = (tabTextAlignment == StringAlignment.Center) ? Math.Max(((rectangle2.Width - num2) / 2f), 0f) : 0f;
            RectangleF rct = new RectangleF(rectangle2.X + num4, rectangle2.Y + num3, Math.Min((base2.TitleTextSize.Width + 2f), (rectangle2.Width - num4)), rectangle2.Height);
            if(fDrawShadow) {
                DrawTextWithShadow(g, base2.Text, bSelected ? colorSet[0] : colorSet[1], bSelected ? colorSet[3] : colorSet[4], (bSelected && fActiveTxtBold) ? (base2.Underline ? fntBold_Underline : fntBold) : (base2.Underline ? fnt_Underline : Font), rct, sfTypoGraphic);
            }
            else {
                g.DrawString(base2.Text, (bSelected && fActiveTxtBold) ? (base2.Underline ? fntBold_Underline : fntBold) : (base2.Underline ? fnt_Underline : Font), bSelected ? brshActive : brshInactv, rct, sfTypoGraphic);
            }
            if(iFocusedTabIndex == index) {
                Rectangle rectangle = rctItem;
                rectangle.Inflate(-2, -1);
                rectangle.Y++;
                rectangle.Width--;
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
            if(flag3 && (rectangle2.Width > base2.TitleTextSize.Width)) {
                float num5 = Math.Max(((rectangle2.Height - base2.SubTitleTextSize.Height) / 2f), 0f);
                RectangleF ef2 = new RectangleF(rct.Right, rectangle2.Y + num5, Math.Min((base2.SubTitleTextSize.Width + 2f), (rectangle2.Width - ((base2.TitleTextSize.Width + num4) + 4f))), rectangle2.Height);
                if(fDrawShadow) {
                    DrawTextWithShadow(g, (fAutoSubText ? "@ " : ": ") + base2.Comment, colorSet[1], colorSet[4], fntSubText, ef2, sfTypoGraphic);
                }
                else {
                    g.DrawString((fAutoSubText ? "@ " : ": ") + base2.Comment, fntSubText, brshInactv, ef2, sfTypoGraphic);
                }
            }
            if(fDrawCloseButton && (!fCloseBtnOnHover || fHot)) {
                Rectangle closeButtonRectangle = GetCloseButtonRectangle(base2.TabBounds, bSelected);
                if(fNowMouseIsOnCloseBtn && (iTabMouseOnButtonsIndex == index)) {
                    if(MouseButtons == MouseButtons.Left) {
                        if(bmpCloseBtn_Pressed == null) {
                            bmpCloseBtn_Pressed = Resources_Image.imgCloseButton_Press;
                        }
                        g.DrawImage(bmpCloseBtn_Pressed, closeButtonRectangle);
                    }
                    else {
                        if(bmpCloseBtn_Hot == null) {
                            bmpCloseBtn_Hot = Resources_Image.imgCloseButton_Hot;
                        }
                        g.DrawImage(bmpCloseBtn_Hot, closeButtonRectangle);
                    }
                }
                else if(fNowShowCloseBtnAlt || fCloseBtnOnHover) {
                    if(bmpCloseBtn_ColdAlt == null) {
                        bmpCloseBtn_ColdAlt = Resources_Image.imgCloseButton_ColdAlt;
                    }
                    g.DrawImage(bmpCloseBtn_ColdAlt, closeButtonRectangle);
                }
                else {
                    if(bmpCloseBtn_Cold == null) {
                        bmpCloseBtn_Cold = Resources_Image.imgCloseButton_Cold;
                    }
                    g.DrawImage(bmpCloseBtn_Cold, closeButtonRectangle);
                }
            }
        }

        private static void DrawTextWithShadow(Graphics g, string txt, Color clrTxt, Color clrShdw, Font fnt, RectangleF rct, StringFormat sf) {
            RectangleF layoutRectangle = rct;
            RectangleF ef2 = rct;
            RectangleF ef3 = rct;
            layoutRectangle.Offset(1f, 1f);
            ef2.Offset(2f, 0f);
            ef3.Offset(1f, 2f);
            Color color = Color.FromArgb(0xc0, clrShdw);
            Color color2 = Color.FromArgb(0x80, clrShdw);
            using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x40, clrShdw))) {
                g.DrawString(txt, fnt, brush, ef3, sf);
                brush.Color = color2;
                g.DrawString(txt, fnt, brush, ef2, sf);
                brush.Color = color;
                g.DrawString(txt, fnt, brush, layoutRectangle, sf);
                brush.Color = clrTxt;
                g.DrawString(txt, fnt, brush, rct, sf);
            }
        }

        public bool FocusNextTab(bool fBack, bool fEntered, bool fEnd) {
            if(tabPages.Count <= 0) {
                return false;
            }
            if(fEntered) {
                iFocusedTabIndex = fBack ? (tabPages.Count - 1) : 0;
                SetPseudoHotIndex(iFocusedTabIndex);
                return true;
            }
            if((fBack && (iFocusedTabIndex == 0)) || (!fBack && (iFocusedTabIndex == (tabPages.Count - 1)))) {
                iFocusedTabIndex = -1;
                return false;
            }
            if(fEnd) {
                iFocusedTabIndex = fBack ? 0 : (tabPages.Count - 1);
            }
            else {
                iFocusedTabIndex += fBack ? -1 : 1;
                if(iFocusedTabIndex < 0) {
                    iFocusedTabIndex = tabPages.Count - 1;
                }
            }
            SetPseudoHotIndex(iFocusedTabIndex);
            return true;
        }

        private Rectangle GetCloseButtonRectangle(Rectangle rctTab, bool fSelected) {
            int num = ((itemSize.Height - 15) / 2) + 1;
            if(!fSelected) {
                num += 2;
            }
            if((iMultipleType == 0) && fNeedToDrawUpDown) {
                rctTab.X += iScrollWidth;
            }
            return new Rectangle(rctTab.Right - 0x11, rctTab.Top + num, 15, 15);
        }

        public int GetFocusedTabIndex() {
            return iFocusedTabIndex;
        }

        private Rectangle GetFolderIconRectangle(Rectangle rctTab, bool fSelected) {
            int num = (rctTab.Height - 0x10) / 2;
            if(!fSelected) {
                num += 2;
            }
            if((iMultipleType == 0) && fNeedToDrawUpDown) {
                rctTab.X += iScrollWidth;
            }
            return new Rectangle(rctTab.X + (fSelected ? 5 : 3), (rctTab.Y + num) - 2, 20, 20);
        }

        private Rectangle GetItemRectangle(int index) {
            Rectangle tabBounds = tabPages[index].TabBounds;
            if(fNeedToDrawUpDown) {
                tabBounds.X += iScrollWidth;
            }
            return tabBounds;
        }

        private Rectangle GetItemRectWithInflation(int index) {
            Rectangle tabBounds = tabPages[index].TabBounds;
            if(index == iSelectedIndex) {
                tabBounds.Inflate(4, 0);
            }
            if(fNeedToDrawUpDown) {
                tabBounds.X += iScrollWidth;
            }
            return tabBounds;
        }

        public QTabItem GetTabMouseOn() {
            Point pt = PointToClient(MousePosition);
            if(((upDown != null) && upDown.Visible) && upDown.Bounds.Contains(pt)) {
                return null;
            }
            QTabItem base2 = null;
            QTabItem base3 = null;
            for(int i = 0; i < tabPages.Count; i++) {
                if(GetItemRectWithInflation(i).Contains(pt)) {
                    if(base2 == null) {
                        base2 = tabPages[i];
                        if(iMultipleType == 0) {
                            return base2;
                        }
                    }
                    else {
                        base3 = tabPages[i];
                        break;
                    }
                }
            }
            if((base3 != null) && (base2.Row <= base3.Row)) {
                return base3;
            }
            return base2;
        }

        public QTabItem GetTabMouseOn(out int index) {
            Point pt = PointToClient(MousePosition);
            QTabItem base2 = null;
            QTabItem base3 = null;
            int num = -1;
            int num2 = -1;
            for(int i = 0; i < tabPages.Count; i++) {
                if(GetItemRectWithInflation(i).Contains(pt)) {
                    if(base2 == null) {
                        base2 = tabPages[i];
                        num = i;
                        if(iMultipleType == 0) {
                            index = i;
                            return base2;
                        }
                    }
                    else {
                        base3 = tabPages[i];
                        num2 = i;
                        break;
                    }
                }
            }
            if(base3 != null) {
                if(base2.Row > base3.Row) {
                    index = num;
                    return base2;
                }
                index = num2;
                return base3;
            }
            index = num;
            return base2;
        }

        public Rectangle GetTabRect(QTabItem tab) {
            Rectangle tabBounds = tab.TabBounds;
            if(fNeedToDrawUpDown) {
                tabBounds.X += iScrollWidth;
            }
            return tabBounds;
        }

        public Rectangle GetTabRect(int index, bool fInflation) {
            if((index <= -1) || (index >= tabPages.Count)) {
                throw new ArgumentOutOfRangeException("index," + index, "index is out of range.");
            }
            if(fInflation) {
                return GetItemRectWithInflation(index);
            }
            return GetItemRectangle(index);
        }

        private bool HitTestOnButtons(Rectangle rctTab, Point pntClient, bool fCloseButton, bool fSelected) {
            if(fCloseButton) {
                return GetCloseButtonRectangle(rctTab, fSelected).Contains(pntClient);
            }
            return GetFolderIconRectangle(rctTab, fSelected).Contains(pntClient);
        }

        private void InitializeRenderer() {
            vsr_LPressed = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemLeftEdge.Pressed);
            vsr_RPressed = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemRightEdge.Pressed);
            vsr_MPressed = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Pressed);
            vsr_LNormal = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemLeftEdge.Normal);
            vsr_RNormal = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemRightEdge.Normal);
            vsr_MNormal = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Normal);
            vsr_LHot = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Hot);
            vsr_RHot = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItemRightEdge.Hot);
            vsr_MHot = new VisualStyleRenderer(VisualStyleElement.Tab.TopTabItem.Hot);
        }

        private void InvalidateTabsOnMouseMove(QTabItem tabPage, int index, Point pnt) {
            iTabMouseOnButtonsIndex = index;
            if(tabPage != hotTab) {
                hotTab = tabPage;
                if((tabPage != null) && !tabPage.TabLocked) {
                    bool fSelected = index == iSelectedIndex;
                    if(fDrawCloseButton) {
                        fNowMouseIsOnCloseBtn = HitTestOnButtons(tabPage.TabBounds, pnt, true, fSelected);
                    }
                    if(fDrawFolderImg && fShowSubDirTip) {
                        fNowMouseIsOnIcon = HitTestOnButtons(tabPage.TabBounds, pnt, false, fSelected);
                    }
                }
                else {
                    fNowMouseIsOnCloseBtn = false;
                    fNowMouseIsOnIcon = false;
                }
                PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
            }
            else if(tabPage != null) {
                bool flag2 = index == iSelectedIndex;
                bool flag3 = false;
                if(fDrawCloseButton) {
                    bool flag4 = HitTestOnButtons(tabPage.TabBounds, pnt, true, flag2);
                    if(fNowMouseIsOnCloseBtn ^ flag4) {
                        fNowMouseIsOnCloseBtn = flag4 && !tabPage.TabLocked;
                        flag3 = true;
                    }
                }
                if(fDrawFolderImg && fShowSubDirTip) {
                    bool flag5 = HitTestOnButtons(tabPage.TabBounds, pnt, false, flag2);
                    if(fNowMouseIsOnIcon ^ flag5) {
                        fNowMouseIsOnIcon = flag5;
                        flag3 = true;
                    }
                }
                if(flag3) {
                    PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
                }
            }
        }

        protected override void OnLostFocus(EventArgs e) {
            iFocusedTabIndex = -1;
            if(iPseudoHotIndex != -1) {
                SetPseudoHotIndex(-1);
            }
            base.OnLostFocus(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            if(!fSuppressDoubleClick) {
                int num;
                QTabItem tabMouseOn = GetTabMouseOn(out num);
                if(((!fDrawCloseButton || (tabMouseOn == null)) || !HitTestOnButtons(tabMouseOn.TabBounds, e.Location, true, num == iSelectedIndex)) && ((!fDrawFolderImg || !fShowSubDirTip) || ((tabMouseOn == null) || !HitTestOnButtons(tabMouseOn.TabBounds, e.Location, false, num == iSelectedIndex)))) {
                    base.OnMouseDoubleClick(e);
                    fSuppressMouseUp = true;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            int num;
            QTabItem tabMouseOn = GetTabMouseOn(out num);
            if(tabMouseOn != null) {
                bool cancel = e.Button == MouseButtons.Right;
                if((!cancel && fDrawCloseButton) && HitTestOnButtons(tabMouseOn.TabBounds, e.Location, true, num == iSelectedIndex)) {
                    PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
                    return;
                }
                if((fNowMouseIsOnIcon && HitTestOnButtons(tabMouseOn.TabBounds, e.Location, false, num == iSelectedIndex)) && (TabIconMouseDown != null)) {
                    if((e.Button == MouseButtons.Left) || cancel) {
                        iTabIndexOfSubDirShown = num;
                        int tabPageIndex = 0;
                        if((iMultipleType == 0) && fNeedToDrawUpDown) {
                            tabPageIndex = iScrollWidth;
                        }
                        TabIconMouseDown(this, new QTabCancelEventArgs(tabMouseOn, tabPageIndex, cancel, TabControlAction.Selecting));
                        PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
                    }
                    return;
                }
                if(e.Button == MouseButtons.Left) {
                    MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Left, ModifierKeys);
                    if(!Config.Mouse.TabActions.ContainsKey(chord) && SelectTab(tabMouseOn)) {
                        fSuppressDoubleClick = true;
                        timerSuppressDoubleClick.Enabled = true;
                    }
                }
            }
            draggingTab = tabMouseOn;
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            iToolTipIndex = -1;
            if(toolTip != null) {
                toolTip.Active = false;
            }
            iPointedChanged_LastRaisedIndex = -2;
            if((PointedTabChanged != null) && (hotTab != null)) {
                PointedTabChanged(null, new QTabCancelEventArgs(null, -1, false, TabControlAction.Deselecting));
            }
            hotTab = null;
            fNowMouseIsOnCloseBtn = fNowMouseIsOnIcon = false;
            PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            int num;
            if(((e.Button == MouseButtons.Right) && !Parent.RectangleToScreen(Bounds).Contains(MousePosition)) && ((ItemDrag != null) && (draggingTab != null))) {
                ItemDrag(this, new ItemDragEventArgs(e.Button, draggingTab));
            }
            QTabItem tabMouseOn = GetTabMouseOn(out num);
            InvalidateTabsOnMouseMove(tabMouseOn, num, e.Location);
            if((PointedTabChanged != null) && (num != iPointedChanged_LastRaisedIndex)) {
                if(tabMouseOn != null) {
                    iPointedChanged_LastRaisedIndex = num;
                    PointedTabChanged(this, new QTabCancelEventArgs(tabMouseOn, num, false, TabControlAction.Selecting));
                }
                else if(iPointedChanged_LastRaisedIndex != -2) {
                    iPointedChanged_LastRaisedIndex = -1;
                    PointedTabChanged(this, new QTabCancelEventArgs(null, -1, false, TabControlAction.Deselecting));
                }
            }
            if(tabMouseOn != null) {
                if(((iToolTipIndex != num) && IsHandleCreated) && !string.IsNullOrEmpty(tabMouseOn.ToolTipText)) {
                    if(toolTip == null) {
                        toolTip = new ToolTip(components) { ShowAlways = true };
                    }
                    else {
                        toolTip.Active = false;
                    }
                    string toolTipText = tabMouseOn.ToolTipText;
                    string str2 = tabMouseOn.ShellToolTip;
                    if(!string.IsNullOrEmpty(str2)) {
                        toolTipText = toolTipText + "\r\n" + str2;
                    }
                    iToolTipIndex = num;
                    toolTip.SetToolTip(this, toolTipText);
                    toolTip.Active = true;
                }
            }
            else {
                iToolTipIndex = -1;
                if(toolTip != null) {
                    toolTip.Active = false;
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            draggingTab = null;
            if(fSuppressMouseUp) {
                fSuppressMouseUp = false;
                base.OnMouseUp(e);
            }
            else {
                int num;
                QTabItem tabMouseOn = GetTabMouseOn(out num);
                if(((fDrawCloseButton && (e.Button != MouseButtons.Right)) && ((CloseButtonClicked != null) && (tabMouseOn != null))) && (!tabMouseOn.TabLocked && HitTestOnButtons(tabMouseOn.TabBounds, e.Location, true, num == iSelectedIndex))) {
                    if(e.Button == MouseButtons.Left) {
                        iTabMouseOnButtonsIndex = -1;
                        QTabCancelEventArgs args = new QTabCancelEventArgs(tabMouseOn, num, false, TabControlAction.Deselected);
                        CloseButtonClicked(this, args);
                        if(args.Cancel) {
                            PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
                        }
                    }
                }
                else {
                    base.OnMouseUp(e);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            fOncePainted = true;
            if(iMultipleType != 0) {
                OnPaint_MultipleRow(e);
            }
            else {
                fNeedToDrawUpDown = CalculateItemRectangle();
                try {
                    QTabItem tabMouseOn = GetTabMouseOn();
                    bool fVisualStyle = !fForceClassic && VisualStyleRenderer.IsSupported;
                    if(fVisualStyle && (vsr_LPressed == null)) {
                        InitializeRenderer();
                    }
                    for(int i = 0; i < tabPages.Count; i++) {
                        if(i != iSelectedIndex) {
                            DrawTab(e.Graphics, GetItemRectangle(i), i, tabMouseOn, fVisualStyle);
                        }
                    }
                    if((tabPages.Count > 0) && (iSelectedIndex > -1)) {
                        DrawTab(e.Graphics, GetItemRectangle(iSelectedIndex), iSelectedIndex, tabMouseOn, fVisualStyle);
                    }
                    if((fNeedToDrawUpDown && (iSelectedIndex < tabPages.Count)) && ((iSelectedIndex > -1) && (GetItemRectangle(iSelectedIndex).X != 0))) {
                        e.Graphics.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, 2, e.ClipRectangle.Height));
                    }
                    ShowUpDown(fNeedToDrawUpDown);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
            }
        }

        private void OnPaint_MultipleRow(PaintEventArgs e) {
            CalculateItemRectangle_MultiRows();
            try {
                QTabItem tabMouseOn = GetTabMouseOn();
                bool fVisualStyle = !fForceClassic && VisualStyleRenderer.IsSupported;
                if(fVisualStyle && (vsr_LPressed == null)) {
                    InitializeRenderer();
                }
                bool flag2 = false;
                for(int i = 0; i < (iCurrentRow + 1); i++) {
                    for(int j = 0; j < tabPages.Count; j++) {
                        QTabItem base3 = tabPages[j];
                        if(base3.Row == i) {
                            if(j != iSelectedIndex) {
                                DrawTab(e.Graphics, base3.TabBounds, j, tabMouseOn, fVisualStyle);
                            }
                            else {
                                flag2 = true;
                            }
                        }
                    }
                    if(flag2) {
                        DrawTab(e.Graphics, tabPages[iSelectedIndex].TabBounds, iSelectedIndex, tabMouseOn, fVisualStyle);
                        flag2 = false;
                    }
                }
                ShowUpDown(false);
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        private void OnTabPageAdded(QTabItem tabPage, int index) {
            if(index == 0) {
                selectedTabPage = tabPage;
            }
            if(TabCountChanged != null) {
                TabCountChanged(this, new QTabCancelEventArgs(tabPage, index, false, TabControlAction.Selected));
            }
        }

        private void OnTabPageInserted(QTabItem tabPage, int index) {
            if(index <= iSelectedIndex) {
                iSelectedIndex++;
            }
            if(TabCountChanged != null) {
                TabCountChanged(this, new QTabCancelEventArgs(tabPage, index, false, TabControlAction.Selected));
            }
        }

        private void OnTabPageRemoved(QTabItem tabPage, int index) {
            if(!Disposing && (index != -1)) {
                if(index == iSelectedIndex) {
                    iSelectedIndex = -1;
                }
                else if(index < iSelectedIndex) {
                    iSelectedIndex--;
                }
                if(TabCountChanged != null) {
                    TabCountChanged(this, new QTabCancelEventArgs(tabPage, index, false, TabControlAction.Deselected));
                }
            }
        }

        private void OnUpDownClicked(bool dir, bool lockPaint) {
            int num = Width - 0x24;
            if((!dir || ((tabPages[tabPages.Count - 1].TabBounds.Right + iScrollWidth) >= num)) && (dir || ((tabPages[0].TabBounds.Left + iScrollWidth) != 0))) {
                iScrollClickedCount += dir ? 1 : -1;
                if(iScrollClickedCount > (tabPages.Count - 1)) {
                    iScrollClickedCount = tabPages.Count - 1;
                }
                else if(iScrollClickedCount < 0) {
                    iScrollClickedCount = 0;
                }
                else {
                    iScrollWidth = -tabPages[iScrollClickedCount].TabBounds.X;
                    if(!lockPaint) {
                        Invalidate();
                    }
                }
            }
        }

        public bool PerformFocusedFolderIconClick(bool fParent) {
            if(((TabIconMouseDown == null) || !Focused) || ((-1 >= iFocusedTabIndex) || (iFocusedTabIndex >= tabPages.Count))) {
                return false;
            }
            iTabIndexOfSubDirShown = iFocusedTabIndex;
            QTabItem tabPage = tabPages[iFocusedTabIndex];
            int tabPageIndex = 0;
            if((iMultipleType == 0) && fNeedToDrawUpDown) {
                tabPageIndex = iScrollWidth;
            }
            TabIconMouseDown(this, new QTabCancelEventArgs(tabPage, tabPageIndex, fParent, TabControlAction.Selecting));
            PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
            return true;
        }

        public override void Refresh() {
            if(!fRedrawSuspended) {
                base.Refresh();
            }
        }

        public void RefreshFolderImage() {
            iTabMouseOnButtonsIndex = -1;
            fNowMouseIsOnIcon = false;
            PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
        }

        public void RefreshOptions(bool fInit) {
            if(fInit) {
                if(Config.Tabs.MultipleTabRows) {
                    iMultipleType = Config.Tabs.ActiveTabOnBottomRow ? 1 : 2;
                }
                fDrawFolderImg = Config.Tabs.ShowFolderIcon;
            }
            else {
                colorSet = new Color[] {
                    Config.Skin.TabTextActiveColor,
                    Config.Skin.TabTextInactiveColor,
                    Config.Skin.TabTextHotColor,
                    Config.Skin.TabShadActiveColor,
                    Config.Skin.TabShadInactiveColor,
                    Config.Skin.TabShadHotColor
                };
                brshActive.Color = colorSet[0];
                brshInactv.Color = colorSet[1];
            }
            if(Config.Skin.FixedWidthTabs) {
                sizeMode = TabSizeMode.Fixed;
                fLimitSize = false;
            }
            else {
                sizeMode = TabSizeMode.Normal;
                fLimitSize = true; // Config.LimitedWidthTabs;
            }
            if((Config.Skin.TabMaxWidth >= Config.Skin.TabMinWidth) && (Config.Skin.TabMinWidth > 9)) {
                maxAllowedTabWidth = Config.Skin.TabMaxWidth;
                minAllowedTabWidth = Config.Skin.TabMinWidth;
            }
            itemSize = new Size(maxAllowedTabWidth, Config.Skin.TabHeight);
            fActiveTxtBold = Config.Skin.ActiveTabInBold;
            fForceClassic = Config.Skin.UseTabSkin;
            SetFont(Config.Skin.TabTextFont);
            sizingMargin = Config.Skin.TabSizeMargin + new Padding(0, 0, 1, 1);
            if(Config.Skin.UseTabSkin && Config.Skin.TabImageFile.Length > 0) {
                SetTabImages(QTTabBarClass.CreateTabImage());
            }
            else {
                SetTabImages(null);
            }
            tabTextAlignment = Config.Skin.TabTextCentered ? StringAlignment.Center : StringAlignment.Near;
            fDrawShadow = Config.Skin.TabTitleShadows;
            fDrawCloseButton = Config.Tabs.ShowCloseButtons && !Config.Tabs.CloseBtnsWithAlt;
            fCloseBtnOnHover = Config.Tabs.CloseBtnsOnHover;
            fShowSubDirTip = Config.Tabs.ShowSubDirTipOnTab;
            if(!fInit) {
                if(fDrawFolderImg != Config.Tabs.ShowFolderIcon) {
                    fDrawFolderImg = Config.Tabs.ShowFolderIcon;
                    if(fDrawFolderImg) {
                        foreach(QTabItem base2 in TabPages) {
                            base2.ImageKey = base2.ImageKey;
                        }
                    }
                    else {
                        fNowMouseIsOnIcon = false;
                    }
                }
                if(fAutoSubText && !Config.Tabs.RenameAmbTabs) {
                    foreach(QTabItem item in TabPages) {
                        item.Comment = string.Empty;
                        item.RefreshRectangle();
                    }
                    Refresh();
                }
                else if(!fAutoSubText && Config.Tabs.RenameAmbTabs) {
                    QTabItem.CheckSubTexts(this);
                }   
            }
            fAutoSubText = Config.Tabs.RenameAmbTabs;
        }

        public bool SelectFocusedTab() {
            if((Focused && (-1 < iFocusedTabIndex)) && (iFocusedTabIndex < tabPages.Count)) {
                SelectedIndex = iFocusedTabIndex;
                return true;
            }
            return false;
        }

        public bool SelectTab(QTabItem tabPage) {
            int index = tabPages.IndexOf(tabPage);
            if(index == -1) {
                throw new ArgumentException("arg was not found.");
            }
            return (((index != -1) && (selectedTabPage != tabPage)) && ChangeSelection(tabPage, index));
        }

        public void SelectTab(int index) {
            if((index <= -1) || (index >= tabPages.Count)) {
                throw new ArgumentOutOfRangeException("index," + index, "index is out of range.");
            }
            QTabItem tabToSelect = tabPages[index];
            if(selectedTabPage != tabToSelect) {
                ChangeSelection(tabToSelect, index);
            }
            else {
                iSelectedIndex = index;
            }
        }

        public void SelectTabDirectly(QTabItem tabPage) {
            int index = tabPages.IndexOf(tabPage);
            selectedTabPage = tabPage;
            SelectedIndex = index;
        }

        public void SetContextMenuState(bool fShow) {
            fNowTabContextMenuStripShowing = fShow;
        }

        private void SetFont(Font fnt) {
            Font = fnt;
            if(fntBold != null) {
                fntBold.Dispose();
            }
            fntBold = Font;
            try {
                fntBold = new Font(Font, FontStyle.Bold);
            }
            catch {}
            if(fnt_Underline != null) {
                fnt_Underline.Dispose();
            }
            fnt_Underline = Font;
            try {
                fnt_Underline = new Font(Font, FontStyle.Underline);
            }
            catch {}
            if(fntBold_Underline != null) {
                fntBold_Underline.Dispose();
            }
            fntBold_Underline = fntBold;
            try {
                fntBold_Underline = new Font(fntBold, FontStyle.Underline);    
            }
            catch {}
            if(fntSubText != null) {
                fntSubText.Dispose();
            }
            float sizeInPoints = Font.SizeInPoints;
            fntSubText = Font;
            try {
                fntSubText = new Font(Font.FontFamily, (sizeInPoints > 8.25f) ? (sizeInPoints - 0.75f) : sizeInPoints);   
            }
            catch {}
            if(fntDriveLetter != null) {
                fntDriveLetter.Dispose();
            }
            fntDriveLetter = Font;
            try {
                fntDriveLetter = new Font(Font.FontFamily, 8.25f);
            }
            catch {}
            QTabItem.TabFont = Font;
        }

        public void SetPseudoHotIndex(int index) {
            int iPseudoHotIndex = this.iPseudoHotIndex;
            this.iPseudoHotIndex = index;
            if((iPseudoHotIndex > -1) && (iPseudoHotIndex < TabCount)) {
                Invalidate(GetTabRect(iPseudoHotIndex, true));
            }
            if((this.iPseudoHotIndex > -1) && (this.iPseudoHotIndex < TabCount)) {
                Invalidate(GetTabRect(this.iPseudoHotIndex, true));
            }
            Update();
        }

        public void SetRedraw(bool bRedraw) {
            if(bRedraw && fRedrawSuspended) {
                base.Refresh();
            }
            fRedrawSuspended = !bRedraw;
        }

        public void SetSubDirTipShown(bool fShown) {
            if(!fShown) {
                iTabIndexOfSubDirShown = -1;
            }
            fSubDirShown = fShown;
        }

        private void SetTabImages(Bitmap[] bmps) {
            if((bmps != null) && (bmps.Length == 3)) {
                if(tabImages == null) {
                    tabImages = bmps;
                }
                else if((tabImages[0] != null) && (tabImages[1] != null)) {
                    Bitmap bitmap = tabImages[0];
                    Bitmap bitmap2 = tabImages[1];
                    Bitmap bitmap3 = tabImages[2];
                    tabImages[0] = bmps[0];
                    tabImages[1] = bmps[1];
                    tabImages[2] = bmps[2];
                    bitmap.Dispose();
                    bitmap2.Dispose();
                    bitmap3.Dispose();
                }
                else {
                    tabImages = bmps;
                }
            }
            else if(((tabImages != null) && (tabImages[0] != null)) && ((tabImages[1] != null) && (tabImages[2] != null))) {
                Bitmap bitmap4 = tabImages[0];
                Bitmap bitmap5 = tabImages[1];
                Bitmap bitmap6 = tabImages[2];
                tabImages = null;
                bitmap4.Dispose();
                bitmap5.Dispose();
                bitmap6.Dispose();
            }
        }

        public int SetTabRowType(int iType) {
            iMultipleType = iType;
            if(iType != 0) {
                fNeedToDrawUpDown = false;
                return (iCurrentRow + 1);
            }
            return 1;
        }

        public void ShowCloseButton(bool fShow) {
            fDrawCloseButton = fNowShowCloseBtnAlt = fShow;
            Invalidate();
        }

        private void ShowUpDown(bool fShow) {
            if(fShow) {
                if(upDown == null) {
                    upDown = new UpDown();
                    upDown.Anchor = AnchorStyles.Right;
                    upDown.ValueChanged += upDown_ValueChanged;
                    Controls.Add(upDown);
                }
                upDown.Location = new Point(Width - 0x24, 0);
                upDown.Visible = true;
                upDown.BringToFront();
            }
            else if((upDown != null) && upDown.Visible) {
                upDown.Visible = false;
            }
        }

        private void timerSuppressDoubleClick_Tick(object sender, EventArgs e) {
            timerSuppressDoubleClick.Enabled = false;
            fSuppressDoubleClick = false;
        }

        private void upDown_ValueChanged(object sender, QEventArgs e) {
            OnUpDownClicked(e.Direction == ArrowDirection.Right, false);
        }

        protected override void WndProc(ref Message m) {
            QTabItem tabMouseOn;
            int num;
            int msg = m.Msg;
            switch(msg) {
                case WM.SETCURSOR:
                    if(fSubDirShown || fNowTabContextMenuStripShowing) {
                        uint num4 = ((uint)((long)m.LParam)) & 0xffff;
                        uint num5 = (((uint)((long)m.LParam)) >> 0x10) & 0xffff;
                        if((num4 == 1) && (num5 == 0x200)) {
                            tabMouseOn = GetTabMouseOn(out num);
                            InvalidateTabsOnMouseMove(tabMouseOn, num, PointToClient(MousePosition));
                            m.Result = (IntPtr)1;
                            return;
                        }
                    }
                    break;

                case WM.MOUSEACTIVATE: {
                        if(!fSubDirShown || (TabIconMouseDown == null)) {
                            break;
                        }
                        int num2 = (((int)((long)m.LParam)) >> 0x10) & 0xffff;
                        if(num2 == 0x207) {
                            break;
                        }
                        bool cancel = num2 == 0x204;
                        m.Result = (IntPtr)4;
                        tabMouseOn = GetTabMouseOn(out num);
                        if(((tabMouseOn == null) || (num == iTabIndexOfSubDirShown)) || !HitTestOnButtons(tabMouseOn.TabBounds, PointToClient(MousePosition), false, num == iSelectedIndex)) {
                            TabIconMouseDown(this, new QTabCancelEventArgs(null, -1, false, TabControlAction.Deselected));
                            return;
                        }
                        int tabPageIndex = 0;
                        if((iMultipleType == 0) && fNeedToDrawUpDown) {
                            tabPageIndex = iScrollWidth;
                        }
                        TabIconMouseDown(this, new QTabCancelEventArgs(tabMouseOn, tabPageIndex, cancel, TabControlAction.Selecting));
                        if(fSubDirShown) {
                            iTabIndexOfSubDirShown = num;
                        }
                        else {
                            iTabIndexOfSubDirShown = -1;
                        }
                        fNowMouseIsOnIcon = true;
                        iTabMouseOnButtonsIndex = num;
                        PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
                        return;
                    }

                case WM.ERASEBKGND:
                    if(!fRedrawSuspended) {
                        break;
                    }
                    m.Result = (IntPtr)1;
                    return;

                default:
                    if(msg != WM.CONTEXTMENU) {
                        break;
                    }
                    if((QTUtility2.GET_X_LPARAM(m.LParam) != -1) || (QTUtility2.GET_Y_LPARAM(m.LParam) != -1)) {
                        tabMouseOn = GetTabMouseOn(out num);
                        if(tabMouseOn == null) {
                            PInvoke.SendMessage(Parent.Handle, 0x7b, m.WParam, m.LParam);
                            return;
                        }
                        if(!fShowSubDirTip || !HitTestOnButtons(tabMouseOn.TabBounds, PointToClient(MousePosition), false, num == iSelectedIndex)) {
                            break;
                        }
                    }
                    return;
            }
            base.WndProc(ref m);
        }

        public bool AutoSubText {
            get {
                return fAutoSubText;
            }
        }

        protected override bool CanEnableIme {
            get {
                return false;
            }
        }

        public bool DrawFolderImage {
            get {
                return fDrawFolderImg;
            }
        }

        public bool EnableCloseButton {
            get {
                return fDrawCloseButton;
            }
            set {
                fDrawCloseButton = value;
            }
        }

        public bool OncePainted {
            get {
                return fOncePainted;
            }
        }

        public int SelectedIndex {
            get {
                return iSelectedIndex;
            }
            set {
                SelectTab(value);
            }
        }

        public QTabItem SelectedTab {
            get {
                return tabPages[iSelectedIndex];
            }
        }

        public bool TabCloseButtonOnAlt {
            get {
                return fNowShowCloseBtnAlt;
            }
        }

        public bool TabCloseButtonOnHover {
            get {
                return fCloseBtnOnHover;
            }
        }

        public int TabCount {
            get {
                return tabPages.Count;
            }
        }

        public int TabOffset {
            get {
                if((iMultipleType == 0) && fNeedToDrawUpDown) {
                    return iScrollWidth;
                }
                return 0;
            }
        }

        public QTabCollection TabPages {
            get {
                return tabPages;
            }
        }

        public sealed class QTabCollection : List<QTabItem> {
            private QTabControl Owner;

            public QTabCollection(QTabControl owner) {
                Owner = owner;
            }

            new public void Add(QTabItem tabPage) {
                base.Add(tabPage);
                Owner.OnTabPageAdded(tabPage, Count - 1);
                Owner.Refresh();
            }

            new public void Insert(int index, QTabItem tabPage) {
                base.Insert(index, tabPage);
                Owner.OnTabPageInserted(tabPage, index);
                Owner.Refresh();
            }

            new public bool Remove(QTabItem tabPage) {
                int index = IndexOf(tabPage);
                Owner.OnTabPageRemoved(tabPage, index);
                bool flag = base.Remove(tabPage);
                Owner.Refresh();
                return flag;
            }

            public void Relocate(int indexSource, int indexDestination) {
                int selectedIndex = Owner.SelectedIndex;
                int num2 = (indexSource > indexDestination) ? indexSource : indexDestination;
                int num3 = (indexSource > indexDestination) ? indexDestination : indexSource;
                QTabItem item = base[indexSource];
                base.Remove(item);
                base.Insert(indexDestination, item);
                if((num2 >= selectedIndex) && (selectedIndex >= num3)) {
                    if(num2 == selectedIndex) {
                        if(num2 == indexSource) {
                            Owner.SelectedIndex = indexDestination;
                        }
                        else {
                            Owner.SelectedIndex--;
                        }
                    }
                    else if((num3 < selectedIndex) && (selectedIndex < num2)) {
                        if(num2 == indexSource) {
                            Owner.SelectedIndex++;
                        }
                        else {
                            Owner.SelectedIndex--;
                        }
                    }
                    else if(num3 == selectedIndex) {
                        if(num2 == indexSource) {
                            Owner.SelectedIndex++;
                        }
                        else {
                            Owner.SelectedIndex = indexDestination;
                        }
                    }
                }
                Owner.Refresh();
            }
        }
    }
}
