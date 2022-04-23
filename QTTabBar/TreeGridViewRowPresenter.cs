using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Reflection;

namespace Ricciolo.Controls
{
    public class TreeGridViewRowPresenter : GridViewRowPresenter
    {

        public static DependencyProperty FirstColumnIndentProperty = DependencyProperty.Register("FirstColumnIndent", typeof(Double), typeof(TreeGridViewRowPresenter), new PropertyMetadata(0d));
        public static DependencyProperty ExpanderProperty = DependencyProperty.Register("Expander", typeof(UIElement), typeof(TreeGridViewRowPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnExpanderChanged)));

        private UIElementCollection childs;

        private static PropertyInfo ActualIndexProperty = typeof(GridViewColumn).GetProperty("ActualIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo DesiredWidthProperty = typeof(GridViewColumn).GetProperty("DesiredWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        public TreeGridViewRowPresenter()
        {
            childs = new UIElementCollection(this, this);
        }

        public Double FirstColumnIndent
        {
            get { return (Double)this.GetValue(FirstColumnIndentProperty); }
            set { this.SetValue(FirstColumnIndentProperty, value); }
        }

        public UIElement Expander
        {
            get { return (UIElement)this.GetValue(ExpanderProperty); }
            set { this.SetValue(ExpanderProperty, value); }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size s = base.ArrangeOverride(arrangeSize);

            if (this.Columns == null || this.Columns.Count == 0) return s;
            UIElement expander = this.Expander;

            double current = 0;
            double max = arrangeSize.Width;
            for (int x = 0; x < this.Columns.Count; x++)
            {
                GridViewColumn column = this.Columns[x];
                // Actual index needed for column reorder
                UIElement uiColumn = (UIElement)base.GetVisualChild((int)ActualIndexProperty.GetValue(column, null));

                // Compute column width
                double w = Math.Min(max, (Double.IsNaN(column.Width)) ? (double)DesiredWidthProperty.GetValue(column, null) : column.Width);

                // First column indent
                if (x == 0 && expander != null)
                {
                    double indent = FirstColumnIndent + expander.DesiredSize.Width;
                    uiColumn.Arrange(new Rect(current + indent, 0, w - indent, arrangeSize.Height));
                }
                else
                    uiColumn.Arrange(new Rect(current, 0, w, arrangeSize.Height));
                max -= w;
                current += w;
            }

            // Show expander
            if (expander != null)
            {
                expander.Arrange(new Rect(this.FirstColumnIndent, 0, expander.DesiredSize.Width, expander.DesiredSize.Height));
            }

            return s;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size s = base.MeasureOverride(constraint);

            // Measure expander
            UIElement expander = this.Expander;
            if (expander != null)
            {
                // Compute max measure
                expander.Measure(constraint);
                s.Width = Math.Max(s.Width, expander.DesiredSize.Width);
                s.Height = Math.Max(s.Height, expander.DesiredSize.Height);
            }

            return s;
        }

        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            // Last element is always the expander
            // called by render engine
            if (index < base.VisualChildrenCount)
                return base.GetVisualChild(index);
            else
                return this.Expander;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                // Last element is always the expander
                if (this.Expander != null)
                    return base.VisualChildrenCount + 1;
                else
                    return base.VisualChildrenCount;
            }
        }

        private static void OnExpanderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Use a second UIElementCollection so base methods work as original
            TreeGridViewRowPresenter p = (TreeGridViewRowPresenter)d;

            p.childs.Remove(e.OldValue as UIElement);
            p.childs.Add((UIElement)e.NewValue);
        }

    }
}
