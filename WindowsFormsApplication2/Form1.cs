using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
        //    this.Top = 0;
           // this.Left = 0;

            Rectangle rect = Screen.PrimaryScreen.Bounds;

            int left = ( (rect.Width - this.Width) / 2 ) - 10 ;


            this.Top = ( int ) ( rect.Height * 0.15 );
            this.Left = left;
        }
    }
}
