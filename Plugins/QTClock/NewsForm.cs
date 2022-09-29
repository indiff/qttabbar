using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Keys = System.Windows.Forms.Keys;
using Timer = System.Windows.Forms.Timer;

namespace QuizoPlugins
{
    public partial class NewsForm : Form
    {
        private Clock.Item[] items;
        private Timer timer;
        private Clock clock;
        private static NewsForm instance;

        private static readonly object m_objLock = new object();


        private NewsForm(Clock clock)
        {
            InitializeComponent();
            // 355 * 162  0x133 0x73  宽度设置为屏幕 1/3 高度为屏幕 1/6
            // int width = Screen.PrimaryScreen.WorkingArea.Size.Width / 3;
            // int height = Screen.PrimaryScreen.WorkingArea.Size.Height - 100;
            // this.ClientSize = new System.Drawing.Size(width, height);
            this.clock = clock;
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 200;
                timer.Tick += timer_Tick;
            }
            timer.Start();
        }


        public static NewsForm GetInstance(Clock clock)
        {
            if (instance == null || instance.IsDisposed)
            {
                lock (m_objLock)
                {
                    if (instance == null || instance.IsDisposed)
                    {
                        instance = new NewsForm(clock);
                    }
                }
 
            }
            return instance; 
        }

       

        private void timer_Tick(object sender, EventArgs e)
        {
            AutoSideHideOrShow();
        }

        void AutoSideHideOrShow()
        {
            int sideThickness = 4;//边缘的厚度，窗体停靠在边缘隐藏后留出来的可见部分的厚度  

            //如果窗体最小化或最大化了则什么也不做  
            if (this.WindowState == FormWindowState.Minimized || this.WindowState == FormWindowState.Maximized)
            {
                return;
            }

            //如果鼠标在窗体内  
            if (Cursor.Position.X >= this.Left && Cursor.Position.X < this.Right && Cursor.Position.Y >= this.Top && Cursor.Position.Y < this.Bottom)
            {
                //如果窗体离屏幕边缘很近，则自动停靠在该边缘  
                if (this.Top <= sideThickness)
                {
                    this.Top = 0;
                }
                if (this.Left <= sideThickness)
                {
                    this.Left = 0;
                }
                if (this.Left >= Screen.PrimaryScreen.WorkingArea.Width - this.Width - sideThickness)
                {
                    this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                }
            }
            //当鼠标离开窗体以后  
            else
            {
                //隐藏到屏幕左边缘  
                if (this.Left == 0)
                {
                    this.Left = sideThickness - this.Width;
                }
                //隐藏到屏幕右边缘  
                else if (this.Left == Screen.PrimaryScreen.WorkingArea.Width - this.Width)
                {
                    this.Left = Screen.PrimaryScreen.WorkingArea.Width - sideThickness;
                }
                //隐藏到屏幕上边缘  
                else if (this.Top == 0 && this.Left > 0 && this.Left < Screen.PrimaryScreen.WorkingArea.Width - this.Width)
                {
                    this.Top = sideThickness - this.Height;
                }
            }
        }

        void AutoSideHide()
        {
            int sideThickness = 4;//边缘的厚度，窗体停靠在边缘隐藏后留出来的可见部分的厚度  
            //隐藏到屏幕左边缘  
            if (this.Left == 0)
            {
                this.Left = sideThickness - this.Width;
            }
            //隐藏到屏幕右边缘  
            else if (this.Left == Screen.PrimaryScreen.WorkingArea.Width - this.Width)
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Width - sideThickness;
            }
            //隐藏到屏幕上边缘  
            else if (this.Top == 0 && this.Left > 0 && this.Left < Screen.PrimaryScreen.WorkingArea.Width - this.Width)
            {
                this.Top = sideThickness - this.Height;
            }
        }


        private void NewsForm_Load(object sender, EventArgs e)
        {
            this.Top = 100;
            this.Left = Screen.PrimaryScreen.Bounds.Width - 800;
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color FColor = Color.Red;
            Color TColor = Color.Yellow;
            Brush b = new LinearGradientBrush(this.ClientRectangle, FColor, TColor, LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(b, this.ClientRectangle);
        }

        public void setItems(Clock.Item[] items )
        {
            // dt.Rows.Clear();
            this.label1.Text = ("数据加载中...");
            this.items = items;
            if (this.items != null && this.items.Length > 0)
            {
                this.listBox1.Items.Clear();
                for (var i = 0; i < this.items.Length; i++)
                {
                     StringBuilder builder = new StringBuilder();
                     builder.AppendFormat("{0} {1} {2}", 
                            // theNewData.items[i].description, 
                            items[i].title,
                            items[i].sitename,
                            items[i].views);
                        // builder.AppendLine();
                        this.listBox1.Items.Add(builder);
                        /*dt.Rows.Add(items[i].title,
                            items[i].sitename,
                            items[i].views);*/
                 }
                 if (items.Length > 0)
                 {
                     /*if (len > 0) {
                         double size = len / 1024d;
                         this.label1.Text = ("加载成功,数据大小" + len + "KB");
                     }
                     else {
                         this.label1.Text = ("加载成功");
                         
                     }*/
                     this.label1.Text = ("加载成功" + items.Length + "条");
                 }
            }
            else
            {
                this.label1.Text = ("加载失败");
            }
        }



        private void NewsForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
               // MessageBox.Show("esc");
                this.Hide();
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //  MessageBox.Show("mouse click");
            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                // MessageBox.Show(index.ToString());
                if (items != null && items.Length > 0)
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        if (index == i )
                        {
                            if (!string.IsNullOrEmpty(items[i].url) && items[i].url.ToLower().StartsWith("http"))
                            {
                                Process.Start(items[i].url);
                                this.AutoSideHide(); // 双击则 side 隐藏
                            }
                            else
                            {
                                MessageBox.Show("未获取到链接");
                            }
                            return;
                        }
                    }
                }
            }
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
               // MessageBox.Show("esc");
                // this.Hide();
                this.AutoSideHide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            // this.Hide();
            // this.AutoSideHide();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndices.Count > 0)
            {
                this.toolTip1.Active = true;
                if (items != null && this.listBox1.SelectedIndex < items.Length)
                {
                    this.toolTip1.SetToolTip(this.listBox1, items[this.listBox1.SelectedIndex].description);
                    var listBox1Item = this.listBox1.Items[this.listBox1.SelectedIndex];
                    
                }
            }
            else
            {
                this.toolTip1.Active = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.label1.Text = ("数据加载中...");
            if (null != clock)
            {
                new Thread(() =>
                {
                   
                    try
                    {
                        clock.LoadNews();
                    }
                    finally
                    {
                    }
                }).Start();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.AutoSideHide();
        }
    }
}
