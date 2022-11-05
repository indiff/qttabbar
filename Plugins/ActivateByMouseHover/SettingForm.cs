using System.Windows.Forms;
using System.Globalization;

namespace QuizoPlugins
{
	public partial class SettingForm : Form
	{
		public SettingForm( int delay )
		{
			InitializeComponent();

			if( this.numericUpDown1.Minimum <= delay && delay <= this.numericUpDown1.Maximum )
			{
				this.numericUpDown1.Value = delay;
			}

            if (CultureInfo.CurrentCulture.Name == "ja-JP")
            {
                this.label1.Text = "待機時間(ミリ秒)";
                this.Text = "Activate By MouseHover";
            }

            if (CultureInfo.CurrentCulture.Name == "zh-CHS")
            {
                this.label1.Text = "延时时间(毫秒)";
                this.Text = "鼠标悬浮激活标签";
            }
       }

		public int Value
		{
			get
			{
				return (int)this.numericUpDown1.Value;
			}
		}

        private void label1_Click(object sender, System.EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {

        }	
	}
}
