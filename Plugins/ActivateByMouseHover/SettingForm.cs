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

			if( CultureInfo.CurrentCulture.Name == "ja-JP" )
			{
				this.label1.Text = "待機時間(ミリ秒)";
			}
		}

		public int Value
		{
			get
			{
				return (int)this.numericUpDown1.Value;
			}
		}	
	}
}
