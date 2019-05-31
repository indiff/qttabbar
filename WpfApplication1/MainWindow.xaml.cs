using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

   /*     private void lstButtonBarPool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = lstButtonBarPool.SelectedIndex;

            if (idx == -1)
            {
                return;
            }

           
        }*/

        private void lstCategories_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MessageBox.Show("" + sender.ToString());
        }

        private void tabbedPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        
    }
}
