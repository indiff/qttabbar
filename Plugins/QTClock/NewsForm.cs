using System;
using System.Collections;              // ArrayList
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization; // JavaScriptSerializer
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
        private static readonly int sideThickness = 4;//边缘的厚度，窗体停靠在边缘隐藏后留出来的可见部分的厚度 

        private Rectangle WorkingArea
        {
            get
            {
                Screen[] screens = Screen.AllScreens;
                if (screens == null || screens.Length == 0)
                {
                    return Screen.PrimaryScreen.WorkingArea;
                }

                int screenIndex = ClockSettings.DockSide == NewsDockSide.Left ? 0 : screens.Length - 1;
                return screens[screenIndex].WorkingArea;
            }
        }

        private bool IsCursorNearDockEdge(Rectangle area)
        {
            Point cursor = Cursor.Position;
            if (ClockSettings.DockSide == NewsDockSide.Left)
            {
                return cursor.X >= area.Left && cursor.X <= area.Left + sideThickness &&
                       cursor.Y >= area.Top && cursor.Y < area.Bottom;
            }

            return cursor.X >= area.Right - sideThickness && cursor.X <= area.Right &&
                   cursor.Y >= area.Top && cursor.Y < area.Bottom;
        }

        private int DockedLeft
        {
            get
            {
                Rectangle area = WorkingArea;
                return ClockSettings.DockSide == NewsDockSide.Left ? area.Left : area.Right - Width;
            }
        }

        private int HiddenLeft
        {
            get
            {
                Rectangle area = WorkingArea;
                return ClockSettings.DockSide == NewsDockSide.Left
                    ? area.Left + sideThickness - Width
                    : area.Right - sideThickness;
            }
        }

        // 新增字段
        private readonly List<NewsSourceConfig> _newsSources = new List<NewsSourceConfig>
        {
            //new NewsSourceConfig { DisplayName = "🇮🇳 India Health", Url = "https://saurav.tech/NewsAPI/top-headlines/category/health/in.json", NeedTranslate = true },
            //new NewsSourceConfig { DisplayName = "🇮🇳 India Tech", Url = "https://saurav.tech/NewsAPI/top-headlines/category/technology/in.json", NeedTranslate = true },
            //new NewsSourceConfig { DisplayName = "🇮🇳 India Business", Url = "https://saurav.tech/NewsAPI/top-headlines/category/business/in.json", NeedTranslate = true },
            new NewsSourceConfig { DisplayName = "🇺🇸 CNN", Url = "https://saurav.tech/NewsAPI/everything/cnn.json", NeedTranslate = true },
            new NewsSourceConfig { DisplayName = "🇺🇸 BBC News", Url = "https://saurav.tech/NewsAPI/everything/bbc-news.json", NeedTranslate = true },
            new NewsSourceConfig { DisplayName = "🇺🇸 Google News", Url = "https://saurav.tech/NewsAPI/everything/google-news.json", NeedTranslate = true },
            //new NewsSourceConfig { DisplayName = "🇬🇧 UK General", Url = "https://saurav.tech/NewsAPI/top-headlines/category/general/gb.json", NeedTranslate = true },
            //new NewsSourceConfig { DisplayName = "🇫🇷 France", Url = "https://saurav.tech/NewsAPI/top-headlines/category/general/fr.json", NeedTranslate = true },
        };

        private CancellationTokenSource _loadCts; // 用于取消上一次未完成的加载



        private NewsForm()
        {
            ClockLocalization.ApplyCurrentCulture();
            InitializeComponent();

            // 初始化 CheckedListBox
            checkedListBox1.CheckOnClick = true;
            foreach (var src in _newsSources)
            {
                checkedListBox1.Items.Add(src, false);
            }
            // 默认选中第一个
            if (checkedListBox1.Items.Count > 0)
                checkedListBox1.SetItemChecked(0, false);


            //checkedListBox1.Dock = DockStyle.Top;
            //checkedListBox1.Height = 60;
            //checkedListBox1.ColumnWidth = Width / 2; // 两列显示
            //listBox1.Dock = DockStyle.Fill;

            FormClosed += NewsForm_FormClosed;
            ApplyDockSide();
            // 355 * 162  0x133 0x73  宽度设置为屏幕 1/3 高度为屏幕 1/6
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

        private void NewsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;

            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
                timer.Dispose();
                timer = null;
            }
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

        public void ApplyDockSide()
        {
            if (IsDisposed || Disposing || WindowState != FormWindowState.Normal)
            {
                return;
            }

            SetDockPosition(false);
        }

        private void SetDockPosition(bool visible)
        {
            Rectangle area = WorkingArea;
            Left = visible ? DockedLeft : HiddenLeft;
        }

       

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                AutoSideHideOrShow();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        void AutoSideHideOrShow()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                return;
            }

            var bMulScreens = Screen.AllScreens.Length > 1;
            if (bMulScreens)
            {
                this.label2.Text = ClockLocalization.Get("UI.MultiScreenDetected");
            }
            else
            {
                this.label2.Text = "" ;
            }

            Rectangle area = WorkingArea;

            //如果窗体最小化或最大化了则什么也不做  
            if (this.WindowState == FormWindowState.Minimized || this.WindowState == FormWindowState.Maximized)
            {
                return;
            }

            bool cursorInForm = Cursor.Position.X >= Left &&
                                Cursor.Position.X < Right &&
                                Cursor.Position.Y >= Top &&
                                Cursor.Position.Y < Bottom;
            if (IsCursorNearDockEdge(area) || cursorInForm)
            {
                SetDockPosition(true);
                return;
            }

            SetDockPosition(false);
        }

        void AutoSideHide()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                return;
            }

            var bMulScreens = Screen.AllScreens.Length > 1;
            this.label2.Text = ClockLocalization.Get("UI.CheckScreens");
            if (bMulScreens)
            {
                this.label2.Text = ClockLocalization.Get("UI.MultiScreenDetected");
            }
            else
            {
                this.label2.Text = "" ;
            }
            Rectangle area = WorkingArea;
            SetDockPosition(false);
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
            this.label1.Text = ClockLocalization.Get("UI.DataLoading");
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
                         this.label1.Text = ClockLocalization.Get("UI.LoadSuccess");
                     }
                     else {
                         this.label1.Text = ClockLocalization.Get("UI.LoadSuccess");

                     }*/
                     this.label1.Text = ClockLocalization.Format("UI.LoadSuccessCount", items.Length);
                     this.button1.Enabled = true;
                     this.button2.Enabled = true;
                     this.button3.Enabled = true;
                 }
            }
            else
            {
                this.label1.Text = ClockLocalization.Get("UI.LoadFailed");
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
                                MessageBox.Show(ClockLocalization.Get("UI.NoLink"));
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
                    // 如果描述信息为空，则取标题
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
            this.label1.Text = ClockLocalization.Get("UI.DataLoading");
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;

            // ── 检查 checkedListBox1 是否有勾选项 ──
            try
            {
                bool hasCheckedSource = false;
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemChecked(i))
                    {
                        hasCheckedSource = true;
                        break;
                    }
                }

                if (hasCheckedSource)
                {
                    // ── 新逻辑：多源 + 翻译 ──
                    var selectedSources = new List<NewsSourceConfig>();
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        if (checkedListBox1.GetItemChecked(i))
                            selectedSources.Add((NewsSourceConfig)checkedListBox1.Items[i]);
                    }

                    _loadCts?.Cancel();
                    _loadCts = new CancellationTokenSource();
                    var token = _loadCts.Token;

                    new Thread(() =>
                    {
                        try
                        {
                            var allItems = LoadMultipleSourcesAsync(selectedSources, token);
                            if (!token.IsCancellationRequested && !IsDisposed && IsHandleCreated)
                            {
                                BeginInvoke(new Action(() => setItems(allItems)));
                            }
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            if (!IsDisposed && IsHandleCreated)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    label1.Text = ClockLocalization.Format("UI.LoadError", ex.Message);
                                    enableButtons(true);
                                }));
                            }
                        }
                    }).Start();
                }
                else
                {
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
            }
            finally
            {
                this.button1.Enabled = true;
                this.button2.Enabled = true;
                this.button3.Enabled = true;
            }
        }

        private Clock.Item[] LoadMultipleSourcesAsync(
      List<NewsSourceConfig> sources,
      CancellationToken token)
        {
            var resultList = new List<Clock.Item>();
            var translateIndices = new List<int>(); // 记录需要翻译的 item 索引
            var serializer = new JavaScriptSerializer();

            // ══════════ 阶段1：下载+解析（纯英文） ══════════
            foreach (var source in sources)
            {
                if (token.IsCancellationRequested) break;

                try
                {
                    using (var client = new System.Net.WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        var json = client.DownloadString(source.Url);

                        var root = serializer.Deserialize<Dictionary<string, object>>(json);
                        if (root == null || !root.ContainsKey("articles")) continue;

                        var articles = root["articles"] as ArrayList;
                        if (articles == null) continue;

                        foreach (var articleObj in articles)
                        {
                            if (token.IsCancellationRequested) break;

                            var article = articleObj as Dictionary<string, object>;
                            if (article == null) continue;

                            var title = SafeGetString(article, "title");
                            var description = SafeGetString(article, "description");
                            var url = SafeGetString(article, "url");

                            var sitename = source.DisplayName;
                            if (article.ContainsKey("source"))
                            {
                                var srcObj = article["source"] as Dictionary<string, object>;
                                if (srcObj != null && !string.IsNullOrEmpty(SafeGetString(srcObj, "name")))
                                    sitename = SafeGetString(srcObj, "name");
                            }

                            resultList.Add(new Clock.Item
                            {
                                title = title,
                                description = description,
                                url = url,
                                sitename = sitename,
                                views = ""
                            });

                            // ★ 记录需要翻译的索引
                            if (source.NeedTranslate)
                                translateIndices.Add(resultList.Count - 1);
                        }
                    }
                }
                catch (Exception) { }
            }

            var items = resultList.ToArray();

            // ══════════ 阶段2：并行翻译（不阻塞） ══════════
            if (translateIndices.Count > 0)
            {
                StartParallelTranslation(items, translateIndices, token);
            }

            return items; // 立即返回英文版
        }

        /// <summary>
        /// 并行翻译，每条独立超时(5s)，成功一条刷新一条
        /// </summary>
        private void StartParallelTranslation(
            Clock.Item[] items,
            List<int> translateIndices,
            CancellationToken parentToken)
        {
            const int TRANSLATE_TIMEOUT_MS = 10000;
            const int MAX_PARALLELISM = 4;

            var semaphore = new SemaphoreSlim(MAX_PARALLELISM);

            foreach (int idx in translateIndices)
            {
                if (parentToken.IsCancellationRequested) break;

                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    await semaphore.WaitAsync(parentToken).ConfigureAwait(false);
                    try
                    {
                        if (parentToken.IsCancellationRequested) return;

                        string originalTitle = items[idx].title;
                        string originalDesc = items[idx].description;

                        // ★ 带超时的翻译
                        string translatedTitle = await TranslateWithTimeoutAsync(
                            originalTitle, TRANSLATE_TIMEOUT_MS, parentToken);
                        string translatedDesc = await TranslateWithTimeoutAsync(
                            originalDesc, TRANSLATE_TIMEOUT_MS, parentToken);

                        // 超时或取消 → 保留原文，不刷新
                        if (translatedTitle == null || parentToken.IsCancellationRequested)
                            return;

                        // ★ 翻译成功 → 逐条刷新 UI
                        if (!IsDisposed && IsHandleCreated)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                // 防御性检查：确保索引仍有效且内容未被覆盖
                                if (idx < items.Length)
                                {
                                    items[idx].title = translatedTitle ?? originalTitle;
                                    items[idx].description = translatedDesc ?? originalDesc;
                                    RefreshListItem(idx); // 只刷新这一行
                                }
                            }));
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[翻译失败] idx={idx}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
        }

        /// <summary>
        /// 带超时的单条翻译，超时返回 null
        /// </summary>
        private async Task<string> TranslateWithTimeoutAsync(
            string text, int timeoutMs, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    cts.CancelAfter(timeoutMs);

                    // 将同步的 EnToZh 包装为 Task
                    var translateTask = Task.Run(() => TranslateService.EnToZh2(text), cts.Token);
                    var completedTask = await Task.WhenAny(translateTask, Task.Delay(timeoutMs, cts.Token))
                                                  .ConfigureAwait(false);

                    if (completedTask == translateTask)
                        return translateTask.Result; // 翻译完成
                    else
                        return null; // 超时
                }
            }
            catch (OperationCanceledException)
            {
                return null; // 超时或外部取消
            }
            catch
            {
                return null; // 翻译异常
            }
        }

        /// <summary>
        /// 只刷新 listBox1 中指定行的显示（避免全量 setItems 闪烁）
        /// </summary>
        private void RefreshListItem(int index)
        {
            if (listBox1.Items.Count > index)
            {
                // WinForms ListBox 没有单行刷新 API
                // 通过 Invalidate 触发重绘，配合 DrawItem 事件自定义绘制
                // 或者直接替换该项（会保持选中状态）
                var selectedItem = listBox1.SelectedItem;
                listBox1.BeginUpdate();
                try
                {
                    // 假设 listBox1 绑定的是 Clock.Item[] 或通过 DataSource
                    // 如果是 DataSource 模式：
                    if (listBox1.DataSource is Clock.Item[] arr && index < arr.Length)
                    {
                        // 重置 DataSource 触发刷新（轻量级）
                        listBox1.DataSource = null;
                        listBox1.DataSource = arr;
                        listBox1.DisplayMember = "title";
                    }
                }
                finally
                {
                    listBox1.EndUpdate();
                }
            }
        }

        /// <summary>
        /// 安全地从字典中取字符串，替代 ?.ToString() ?? ""
        /// </summary>
        private static string SafeGetString(Dictionary<string, object> dict, string key)
        {
            if (dict == null) return "";
            if (!dict.ContainsKey(key)) return "";
            var val = dict[key];
            if (val == null) return "";
            return val.ToString();
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
