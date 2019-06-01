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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Padding = System.Windows.Forms.Padding;

namespace QTTabBarLib {

    /// <summary>
    /// Interaction logic for MarginCombo.xaml
    /// </summary>
    public partial class MarginCombo : ComboBox {
        private const int VAL_MAX = 99;
        private TextBox txtMargin;
        private MarginEntry[] entries;
        public MarginCombo() {
            InitializeComponent();
            ItemsSource = entries = new MarginEntry[] {
                new MarginEntry(this, 0),
                new MarginEntry(this, 1),
                new MarginEntry(this, 2),
                new MarginEntry(this, 3),
                new MarginEntry(this, 4)
            };
        }

        // It *really* pisses me off that I can't use the Thickness class instead.
        // The WPF devs forgot to mark it as Serializable!
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
                typeof(Padding), typeof(MarginCombo), new FrameworkPropertyMetadata(System.Windows.Forms.Padding.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, MarginCombo_ValueChanged));

        private static void MarginCombo_ValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            MarginCombo combo = (MarginCombo)dependencyObject;
            Padding p = (Padding)dependencyPropertyChangedEventArgs.NewValue;
            combo.entries[1].Value = p.Left;
            combo.entries[2].Value = p.Top;
            combo.entries[3].Value = p.Right;
            combo.entries[4].Value = p.Bottom;
            combo.entries[0].Value = combo.entries.Skip(1).All(e => e.Value == p.Left) ? p.Left : -1;
        }

        public Padding Value {
            get { return (Padding)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            txtMargin = GetTemplateChild("PART_EditableTextBox") as TextBox;
            if(txtMargin != null) {
                txtMargin.PreviewTextInput += txtMargin_PreviewTextInput;
                txtMargin.TextChanged += txtMargin_TextChanged;
                txtMargin.SetBinding(TextBox.TextProperty, new Binding("Value") {
                    Source = this,
                    Converter = new JoinedTextConverter(),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }
        }

        #region Event Handlers

        //# The converter will do the validation for us.
        private void txtMargin_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs) {
            TextBox box = ((TextBox)sender);
            int pos = box.CaretIndex;
            BindingExpression bind = box.GetBindingExpression(TextBox.TextProperty);
            if(bind != null) {
                bind.UpdateTarget();
                box.CaretIndex = pos;
            }
        }

        //# Make sure the text stays numeric and in range.
        private void txtLTRB_TextChanged(object sender, RoutedEventArgs e) {
            int i;
            TextBox box = ((TextBox)sender);
            int pos = box.CaretIndex;
            BindingExpression bind = box.GetBindingExpression(TextBox.TextProperty);
            if(box.Text.Length == 0) return;
            if(!int.TryParse(box.Text, out i)) {
                if(bind != null) bind.UpdateTarget();
            }
            else if(i > VAL_MAX) {
                box.Text = VAL_MAX.ToString();
                box.CaretIndex = pos;
            }
            else if(i < 0) {
                box.Text = "0";
                box.CaretIndex = pos;
            }
        }

        //# Allow only digits to be entered.  We still need TextChanged to 
        //# make sure letters don't get in via pasting, drag & drop, etc.
        private void txtMargin_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.ToCharArray().All(c => char.IsDigit(c) || c == ' ' || c == ',');
        }

        private void txtLTRB_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.ToCharArray().All(char.IsDigit);
        }

        private void MouseEater(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
        }

        #endregion

        [ValueConversion(typeof(Padding), typeof(string))]
        private class JoinedTextConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                Padding t = (Padding)value;
                return t.Left + ", " + t.Top + ", " + t.Right + ", " + t.Bottom;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                int i;
                int[] v = value.ToString().Split(',').Select(
                        s => int.TryParse(s.Trim(), out i) ? Math.Min(Math.Max(i, 0), VAL_MAX) : 0).ToArray();
                Array.Resize(ref v, 4);
                return new Padding(v[0], v[1], v[2], v[3]);
            }
        }


        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class MarginEntry : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            private MarginCombo Parent;

            public Thickness PanelMargin { get {
                Thickness t = new Thickness(5);
                if(Index > 0) t.Top = 1;
                if(Index < 4) t.Bottom = 0;
                return t;
            }}
            public Thickness LabelMargin { get {
                return new Thickness(Index == 0 ? 5 : 15, 0, 10, 0);
            }}

            public string TextBoxValue {
                get {
                    return Value >= 0 ? Value.ToString() : "";
                }
                set {
                    int i;
                    if(!int.TryParse(value.Trim(), out i) || i < 0 || i > VAL_MAX) return;
                    Padding current = Parent.Value;
                    switch(Index) {
                        case 1: current.Left   = i; break;
                        case 2: current.Top    = i; break;
                        case 3: current.Right  = i; break;
                        case 4: current.Bottom = i; break;
                        default: current = new Padding(i); break;
                    }
                    Parent.Value = current;
                }
            }
            public int Value { get; set; }
            public int Index { get; set; }

            public MarginEntry(MarginCombo parent, int idx) {
                Index = idx;
                Parent = parent;
            }

            public override string ToString() {
                Padding t = Parent.Value;
                return t.Left + ", " + t.Top + ", " + t.Right + ", " + t.Bottom;
            }
        }

        #endregion

    }
}
