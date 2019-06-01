using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl {

        public static readonly DependencyProperty ValueProperty = 
                DependencyProperty.Register("Value", typeof(int), typeof(Spinner),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty MaxProperty =
                DependencyProperty.Register("Max", typeof(int), typeof(Spinner), 
                new FrameworkPropertyMetadata(10000, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MinProperty =
                DependencyProperty.Register("Min", typeof(int), typeof(Spinner), 
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public int Max {
            get { return (int)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public int Min {
            get { return (int)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public int Value {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public Spinner() {
            InitializeComponent();
        }

        //# Make sure the text stays numeric and in range.
        private void txtValue_TextChanged(object sender, RoutedEventArgs e) {
            int i;
            if(!int.TryParse(txtValue.Text, out i)) {
                txtValue.Text = Value.ToString();
            }
            else if(i > Max) {
                i = Max;    
                txtValue.Text = Max.ToString();
            }
            else if(i < Min) {
                i = Min;
                txtValue.Text = Min.ToString();
            }
            // It seems the binding only refreshes when focus is lost,
            // so we need to set Value here as well.
            Value = i;
        }

        //# Allow only digits to be entered.  We still need TextChanged to 
        //# make sure letters don't get in via pasting, drag & drop, etc.
        private void txtValue_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.ToCharArray().All(char.IsDigit);
        }

        //# Increment, or decrement on up, or down arrow key usage.
        private void txtValue_KeyDown(object sender, KeyEventArgs e) {
           if(e.Key == Key.Up) {
               Value++;
           }
           else if(e.Key == Key.Down) {
               Value--;
           }
        }

        [ValueConversion(typeof(int), typeof(string))]
        public class IntToTextConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return value.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                int i;
                return int.TryParse(value.ToString(), out i) ? i : Binding.DoNothing;
            }
        }
    }
}
