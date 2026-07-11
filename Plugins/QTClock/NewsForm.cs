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

        public Clock clock { get; set; }

        private static NewsForm instance;

        private static readonly object m_objLock = new object();
        private static readonly int sideThickness = 4;//Edge thickness - the visible portion left showing when the form docks and hides at the screen edge


        private NewsForm()
        {
            InitializeComponent();
            // 355 * 162  0x133 0x73  width set to 1/3 of the screen, height to 1/6 of the screen
            // int width = Screen.PrimaryScreen.WorkingArea.Size.Width / 3;
            // int height = Screen.PrimaryScreen.WorkingArea.Size.Height - 100;
            // this.ClientSize = new System.Drawing.Size(width, height);
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 200;
                timer.Tick += timer_Tick;
            }
            timer.Start();
        }


        public static NewsForm GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                lock (m_objLock)
                {
                    if (instance == null || instance.IsDisposed)
                    {
                        instance = null;
                        instance = new NewsForm();
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
            var bMulScreens = Screen.AllScreens.Length > 1;
            var screenWidth = 0;
            if (bMulScreens)
            {
                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screenWidth += Screen.AllScreens[i].WorkingArea.Width;
                }
                this.label2.Text = "Multi-screen setup detected!";
            }
            else
            {
                screenWidth += Screen.PrimaryScreen.WorkingArea.Width;
                this.label2.Text = "" ;
            }

            //If the form is minimized or maximized, do nothing
            if (this.WindowState == FormWindowState.Minimized || this.WindowState == FormWindowState.Maximized)
            {
                return;
            }

            //If the mouse is within the form
            if (Cursor.Position.X >= this.Left &&
                Cursor.Position.X < this.Right &&
                Cursor.Position.Y >= this.Top &&
                Cursor.Position.Y < this.Bottom)
            {
                //If the form is very close to a screen edge, dock it automatically at that edge
                if (this.Top <= sideThickness)
                {
                    this.Top = 0;
                }
                if (this.Left <= sideThickness)
                {
                    this.Left = 0;
                }
                if (this.Left >= screenWidth - this.Width - sideThickness)
                {
                    this.Left = screenWidth - this.Width;
                }
            }
            //After the mouse leaves the form
            else
            {
                //Hide to the left screen edge
                if (this.Left == 0)
                {
                    this.Left = sideThickness - this.Width;
                }
                //Hide to the right screen edge
                else if (this.Left == screenWidth - this.Width)
                {
                    this.Left = screenWidth - sideThickness;
                }
                //Hide to the right screen edge
                else if (this.Left < screenWidth - this.Width)
                {
                    // MessageBox.Show("Screen.PrimaryScreen.WorkingArea.Width - this.Width " + this.Left);
                    this.Left = screenWidth - sideThickness;
                }
                //Hide to the top screen edge
                else if (this.Top == 0 && this.Left > 0 && this.Left < screenWidth - this.Width)
                {
                    this.Top = sideThickness - this.Height;
                }
            }
        }

        void AutoSideHide()
        {
            var bMulScreens = Screen.AllScreens.Length > 1;
            var screenWidth = 0;
            this.label2.Text = "check screens...";
            if (bMulScreens)
            {
                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screenWidth += Screen.AllScreens[i].WorkingArea.Width;
                }

                // this.label2.Text = "Multi-screen setup detected:" + Screen.AllScreens.Length + ",width:" + screenWidth;
                this.label2.Text = "Multi-screen setup detected!" ;
            }
            else
            {
                screenWidth += Screen.PrimaryScreen.WorkingArea.Width;
                this.label2.Text = "" ;
            }
            //Hide to the left screen edge
            if (this.Left == 0)
            {
                // MessageBox.Show("" + this.Left);
                this.Left = sideThickness - this.Width;
            }
            //Hide to the right screen edge
            else if (this.Left == screenWidth - this.Width)
            {
               // MessageBox.Show("Screen.PrimaryScreen.WorkingArea.Width - this.Width " + this.Left);
                this.Left = screenWidth - sideThickness;
            }
            //Hide to the right screen edge
            else if (this.Left < screenWidth - this.Width)
            {
               // MessageBox.Show("Screen.PrimaryScreen.WorkingArea.Width - this.Width " + this.Left);
                this.Left = screenWidth - sideThickness;
            }
            //Hide to the top screen edge
            else if (this.Top == 0 && this.Left > 0 && this.Left < screenWidth - this.Width)
            {
                //MessageBox.Show("Screen.PrimaryScreen.WorkingArea.Width - this.Width " + this.Left);
                this.Top = sideThickness - this.Height;
            }
        }


        private void NewsForm_Load(object sender, EventArgs e)
        {
            var bMulScreens = Screen.AllScreens.Length > 1;
            var screenWidth = 0;
            if (bMulScreens)
            {
                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screenWidth += Screen.AllScreens[i].WorkingArea.Width;
                }

            }
            else
            {
                screenWidth += Screen.PrimaryScreen.WorkingArea.Width;
            }
            this.Top = 150;
            this.Left = screenWidth - this.Width;
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
            this.label1.Text = ("Loading data...");
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
                         this.label1.Text = ("Loaded successfully, data size " + len + "KB");
                     }
                     else {
                         this.label1.Text = ("Loaded successfully");
                         
                     }*/
                     this.label1.Text = ("Loaded successfully, " + items.Length + " items");
                     this.button1.Enabled = true;
                     this.button2.Enabled = true;
                     this.button3.Enabled = true;
                 }
            }
            else
            {
                this.label1.Text = ("Failed to load");
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
                                this.AutoSideHide(); // double-click hides to the side
                            }
                            else
                            {
                                MessageBox.Show("No link was found");
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
                    var description = items[this.listBox1.SelectedIndex].description;
                    // If the description is empty, use the title instead
                    if (isEmpty(description))
                    {
                        description = items[this.listBox1.SelectedIndex].title;
                    }
                    this.toolTip1.SetToolTip(this.listBox1, description);
                    var listBox1Item = this.listBox1.Items[this.listBox1.SelectedIndex];
                    
                }
            }
            else
            {
                this.toolTip1.Active = false;
            }
        }

        private bool isEmpty(string description)
        {
            return description == null || description.Trim().Length == 0; 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.label1.Text = ("Loading data...");
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
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

        public void enableButtons( bool flag )
        {
            this.button1.Enabled = flag;
            this.button2.Enabled = flag;
            this.button3.Enabled = flag;
        }
    }
}
