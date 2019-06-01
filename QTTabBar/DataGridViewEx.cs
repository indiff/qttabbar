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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace QTTabBarLib {
    internal sealed class DataGridViewEx : DataGridView {
        public DataGridViewEx() {
            DoubleBuffered = true;
        }
    }

    internal sealed class DataGridViewProgressBarColumn : DataGridViewColumn {
        private List<DataGridViewProgressBarCell> lstCells;
        private static string VALUE_WAITING = "           Waiting...           ";

        public DataGridViewProgressBarColumn()
            : base(new DataGridViewProgressBarCell()) {
            lstCells = new List<DataGridViewProgressBarCell>();
            SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public void FinishProgress(DataGridViewProgressBarCell cell) {
            if(cell.CalculatingStatus == HashCalcStatus.Calculating) {
                cell.CalculatingStatus = HashCalcStatus.Finished;
            }
            cell.EndProgress();
            lstCells.Remove(cell);
        }

        public void InitializeProgress(DataGridViewProgressBarCell cell) {
            if(cell.ColumnIndex != Index) {
                throw new ArgumentException("cell is not contained in this column.");
            }
            if(!lstCells.Contains(cell)) {
                cell.OldStyle = cell.Style.Clone();
                cell.Value = VALUE_WAITING;
                cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                cell.Style.ForeColor = cell.Style.SelectionForeColor = SystemColors.GrayText;
                cell.CalculatingStatus = HashCalcStatus.Calculating;
                lstCells.Add(cell);
            }
        }

        public void StopAll() {
            foreach(DataGridViewProgressBarCell cell in lstCells) {
                if(cell.CalculatingStatus != HashCalcStatus.Finished) {
                    cell.CalculatingStatus = HashCalcStatus.Aborted;
                }
                cell.EndProgress();
            }
            lstCells.Clear();
        }
    }

    internal sealed class DataGridViewProgressBarCell : DataGridViewTextBoxCell {
        private HashCalcStatus calcStatus;
        private bool fNowCalculating;
        private bool fStopped = true;
        private long lCallbackInterval = 0x64000L;
        private long lFileSize;
        private DataGridViewCellStyle oldStyle;
        private long progressValue;
        private static string VALUE_ABORTED = "            Canceled.           ";
        private static string VALUE_EMPTY = "                                ";

        public void EndProgress() {
            fStopped = true;
            progressValue = 0L;
            if(calcStatus == HashCalcStatus.Aborted) {
                Value = VALUE_ABORTED;
                Style.ForeColor = Color.Red;
                Style.SelectionForeColor = oldStyle.SelectionForeColor;
            }
            else {
                Style = oldStyle;
            }
            Invalidate();
        }

        private void Invalidate() {
            if(DataGridView != null) {
                DataGridView.InvalidateCell(this);
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
            if(fNowCalculating) {
                PaintBackGround(graphics, advancedBorderStyle, cellStyle, cellBounds);
                paintParts &= ~(DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.Background);
            }
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }

        private void PaintBackGround(Graphics graphics, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewCellStyle cellStyle, Rectangle cellBounds) {
            Rectangle rectangle = BorderWidths(advancedBorderStyle);
            Rectangle rect = cellBounds;
            rect.Offset(rectangle.X, rectangle.Y);
            rect.Width -= rectangle.Right;
            rect.Height -= rectangle.Bottom;
            using(SolidBrush brush = new SolidBrush(cellStyle.BackColor)) {
                graphics.FillRectangle(brush, rect);
            }
            if(lFileSize <= 0L) {
                return;
            }
            Rectangle bounds = rect;
            double num = (progressValue) / ((double)lFileSize);
            bounds.Width = (int)(bounds.Width * num);
            if(VisualStyleRenderer.IsSupported) {
                try {
                    new VisualStyleRenderer(VisualStyleElement.ProgressBar.Chunk.Normal).DrawBackground(graphics, bounds);
                    goto Label_00E1;
                }
                catch {
                    goto Label_00E1;
                }
            }
            using(SolidBrush brush2 = new SolidBrush(SystemColors.Highlight)) {
                graphics.FillRectangle(brush2, bounds);
            }
        Label_00E1:
            using(StringFormat format = new StringFormat()) {
                format.Alignment = format.LineAlignment = StringAlignment.Center;
                using(SolidBrush brush3 = new SolidBrush(cellStyle.ForeColor)) {
                    graphics.DrawString(((int)(num * 100.0)) + "%", cellStyle.Font, brush3, rect, format);
                }
            }
        }

        public void Progress(int value) {
            if(fNowCalculating) {
                if(fStopped) {
                    Value = VALUE_EMPTY;
                    fStopped = false;
                }
                long num = value * lCallbackInterval;
                if((progressValue + num) <= lFileSize) {
                    progressValue += num;
                    Invalidate();
                }
            }
        }

        protected override bool SetValue(int rowIndex, object value) {
            fStopped = true;
            progressValue = 0L;
            return base.SetValue(rowIndex, value);
        }

        public HashCalcStatus CalculatingStatus {
            get {
                return calcStatus;
            }
            set {
                fNowCalculating = value == HashCalcStatus.Calculating;
                calcStatus = value;
            }
        }

        public long FileSize {
            get {
                return lFileSize;
            }
            set {
                lFileSize = value;
            }
        }

        public DataGridViewCellStyle OldStyle {
            get {
                return oldStyle;
            }
            set {
                oldStyle = value;
            }
        }
    }

    internal enum HashCalcStatus {
        None,
        Calculating,
        Finished,
        Aborted
    }
}
