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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for EditableHeader.xaml
    /// </summary>
    public partial class EditableHeader : UserControl {

        private bool preparing = false;
        private string originalText;
        private bool editOnLoad = false;

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public UIElement Container {
            get { return (UIElement)GetValue(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }

        public bool ReadOnly {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        public bool CancelIfEmpty {
            get { return (bool)GetValue(CancelIfEmptyProperty); }
            set { SetValue(CancelIfEmptyProperty, value); }
        }

        public bool IsEditing {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableHeader),
            new FrameworkPropertyMetadata("New Header", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ContainerProperty =
            DependencyProperty.Register("Container", typeof(UIElement), typeof(EditableHeader),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(EditableHeader),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CancelIfEmptyProperty =
            DependencyProperty.Register("CancelIfEmpty", typeof(bool), typeof(EditableHeader),
            new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableHeader),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsEditingChanged));

        public EditableHeader(string initialName) {
            InitializeComponent();

            if(initialName != null) {
                Text = initialName;
            }

            Loaded += delegate {
                if(editOnLoad) StartEdit();
                editOnLoad = false;
            };
        }


        public EditableHeader()
            : this(null) {
        }

        private static void IsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            EditableHeader header = (EditableHeader)d;
            if((bool)e.NewValue) {
                header.StartEdit();
            }
            else {
                header.EndEdit();
            }
        }

        private void EndEdit(bool cancel = false) {
            if(!IsLoaded) {
                editOnLoad = false;
                return;
            }
            else if(txtHeaderEdit.Visibility == Visibility.Hidden) {
                return;
            }
            txtHeaderEdit.Visibility = Visibility.Hidden;

            BindingExpression expr = txtHeaderEdit.GetBindingExpression(TextBox.TextProperty);
            expr.UpdateSource();

            if(cancel || (CancelIfEmpty && string.IsNullOrEmpty(Text))) {
                Text = originalText;
            }
            IsEditing = false;
        }

        private void StartEdit() {
            if(!IsLoaded) {
                editOnLoad = true;
                return;
            }
            else if(txtHeaderEdit.Visibility == Visibility.Visible) {
                return;
            }
            originalText = Text;
            txtHeaderEdit.SelectAll();
            txtHeaderEdit.Visibility = Visibility.Visible;
            txtHeaderEdit.CaptureMouse();
            txtHeaderEdit.Focus();
            txtHeaderEdit.ReleaseMouseCapture();
            IsEditing = true;
        }

        private void txtHeaderEdit_LostFocus(object sender, RoutedEventArgs e) {
            if(IsEditing) EndEdit();
        }

        private void txtHeaderEdit_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                EndEdit();
            }
            else if(e.Key == Key.Escape) {
                EndEdit(true);
            }
        }

        private void txtHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if(ReadOnly) {
                // We can add a sound if you need to do so.
                return;
            }
            preparing = Container == null || Container.IsFocused;
        }

        private void txtHeader_MouseUp(object sender, MouseButtonEventArgs e) {
            // Does not open the text box if the editable area has got focus by clicking right now.
            if(!preparing) {
                return;
            }

            StartEdit();
        }
    }
}
