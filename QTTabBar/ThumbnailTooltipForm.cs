//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using MultiLanguage;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    /**
     * 预览窗口
     */
    internal sealed class ThumbnailTooltipForm : Form {
        private const string EMPTYFILE = "  *empty file";
        private bool fFontAsigned;
        private bool fIsShownByKey;
        private ImageCacheStore imageCacheStore;
        private const string IOERROR_CANNOTACCESS = "  *Access Error!";
        private static IOException ioException;
        private bool isShowing;
        private Label lblInfo;
        private Label lblText;
        private List<string> lstPathFailedThumbnail;
        private const int MAX_CACHE_LENGTH = 0x80;
        private const int MAX_TEXT_LENGTH = 0x400;
        public const int MAX_THUMBNAIL_HEIGHT = 0x4b0;
        public const int MAX_THUMBNAIL_WIDTH = 0x780;
        private int maxHeight = Config.Tips.PreviewMaxHeight;
        private int maxWidth = Config.Tips.PreviewMaxWidth;
        private PictureBox pictureBox1;
        public event QEventHandler ThumbnailVisibleChanged;
       //  static readonly string BomMarkUtf8String = Encoding.UTF8.GetString(BomMarkUtf8);


        private  static byte[] _utf16BeBom =
        {
            0xFE,
            0xFF
        };

        private  static byte[] _utf16LeBom =
        {
            0xFF,
            0xFE
        };

        private  static byte[] _utf8Bom =
        {
            0xEF,
            0xBB,
            0xBF
        };

        /// <summary>
        /// static fields.
        /// </summary>
        private static string supportedImages;
        // 支持的视频格式
        private static string supportedMovies = ".asx;.dvr-ms;.mp2;.flv;..mkv;.ts;.3g2;.3gp;.3gp2;.3gpp;.amr;.amv;.asf;.avi;.bdmv;.bik;.d2v;.divx;.drc;.dsa;.dsm;.dss;.dsv;.evo;.f4v;.flc;.fli;.flic;.flv;.hdmov;.ifo;.ivf;.m1v;.m2p;.m2t;.m2ts;.m2v;.m4b;.m4p;.m4v;.mkv;.mp2v;.mp4;.mp4v;.mpe;.mpeg;.mpg;.mpls;.mpv2;.mpv4;.mov;.mts;.ogm;.ogv;.pss;.pva;.qt;.ram;.ratdvd;.rm;.rmm;.rmvb;.roq;.rpm;.smil;.smk;.swf;.tp;.tpr;.ts;.vob;.vp6;.webm;.wm;.wmp;.wmv";


        private static Encoding Encoding950 = Encoding.GetEncoding(950);
        private static Encoding Encoding936 = Encoding.GetEncoding(936);
        private static Encoding EncodingUTF16LeBom = (Encoding)new UnicodeEncoding(false, true);
        private static Encoding EncodingUTF16BeBom = (Encoding)new UnicodeEncoding(true, true);
        private static Encoding EncodingUTF16LeNoBom = (Encoding)new UnicodeEncoding(false, false);
        private static Encoding EncodingUTF16BeNoBom = (Encoding)new UnicodeEncoding(true, false);
        private static Encoding EncodingUTF8Bom = (Encoding)new UTF8Encoding(true);
        private static Encoding EncodingUTF8NoBom = (Encoding)new UTF8Encoding(false);
        static readonly byte[] BomMarkUtf8 = Encoding.UTF8.GetPreamble();
        private static bool _nullSuggestsBinary = true;
        private static double _utf16ExpectedNullPercent = 70;
        private static double _utf16UnexpectedNullPercent = 10;



        public ThumbnailTooltipForm() {
            InitializeComponent();
            lstPathFailedThumbnail = new List<string>();
            imageCacheStore = new ImageCacheStore(0x80);
        }

        public void ClearCache() {
            imageCacheStore.Clear();
        }

        private bool CreateThumbnail(string path, ref Size formSize) {
            string ext = Path.GetExtension(path).ToLower();
            if(ExtIsImage(ext)) {
                FileInfo info = new FileInfo(path);
                if(!info.Exists || (info.Length <= 0L)) {
                    return false;
                }
                bool flag = false;
                bool thumbnail = false;
                bool fCached = false;
                Bitmap bitmap = null;
                ImageData item = null;
                Size empty = Size.Empty;
                Size sizeActual = Size.Empty;
                lblInfo.Text = string.Empty;
                string toolTipText = null;
                if((maxWidth != Config.Tips.PreviewMaxWidth) || (maxHeight != Config.Tips.PreviewMaxHeight)) {
                    maxWidth = Config.Tips.PreviewMaxWidth;
                    maxHeight = Config.Tips.PreviewMaxHeight;
                    pictureBox1.Image = null;
                    imageCacheStore.Clear();
                    lstPathFailedThumbnail.Clear();
                }
                foreach(ImageData data2 in imageCacheStore) {
                    if(data2.Path.PathEquals(path)) {
                        if(data2.ModifiedDate == info.LastWriteTime) {
                            bitmap = data2.Bitmap;
                            thumbnail = data2.Thumbnail;
                            empty = data2.RawSize;
                            sizeActual = data2.ZoomedSize;
                            toolTipText = data2.TooltipText;
                            flag = true;
                        }
                        else {
                            item = data2;
                        }
                        break;
                    }
                }
                if(item != null) {
                    imageCacheStore.Remove(item);
                }
                if(!flag) {
                    try {
                        ImageData data3;
                        if(!ExtIsDefaultImage(ext)) {
                            if(lstPathFailedThumbnail.Contains(path)) {
                                return false;
                            }
                            thumbnail = true;
                            if(!QTUtility.IsXP) {
                                data3 = LoadThumbnail(path, info.LastWriteTime, out empty, out sizeActual, out toolTipText, out fCached);
                            }
                            else {
                                data3 = LoadThumbnail2(path, info.LastWriteTime, out empty, out sizeActual, out toolTipText, out fCached);
                            }
                        }
                        else {
                            data3 = LoadImageFile(path, info.LastWriteTime, out empty, out sizeActual);
                        }
                        if(data3 == null) {
                            lstPathFailedThumbnail.Add(path);
                            return false;
                        }
                        bitmap = data3.Bitmap;
                        imageCacheStore.Add(data3);
                    }
                    catch (Exception e)
                    {
                        QTUtility2.MakeErrorLog(e, "CreateThumbnail");
                        return false;
                    }
                }
                int width = 0x9e;
                if(width < sizeActual.Width) {
                    width = sizeActual.Width;
                }
                bool flag4 = false;
                if(Config.Tips.ShowPreviewInfo) {
                    SizeF ef;
                    string text = Path.GetFileName(path) + "\r\n";
                    if(thumbnail && (toolTipText != null)) {
                        text = text + toolTipText;
                    }
                    else {
                        bool flag5 = sizeActual == empty;
                        text = text + FormatSize(info.Length);
                        if(!thumbnail) {
                            object obj2 = text;
                            text = string.Concat(new object[] { obj2, "    ( ", empty.Width, " x ", empty.Height, " )", flag5 ? string.Empty : "*" });
                        }
                        text = text + "\r\n" + info.LastWriteTime;
                    }
                    using(Graphics graphics = lblInfo.CreateGraphics()) {
                        ef = graphics.MeasureString(text, lblInfo.Font, (width - 8));
                    }
                    lblInfo.SuspendLayout();
                    lblInfo.Text = text;
                    lblInfo.Width = width;
                    lblInfo.Height = (int)(ef.Height + 8f);
                    lblInfo.ResumeLayout();
                    formSize = new Size(width + 8, (sizeActual.Height + lblInfo.Height) + 8);
                }
                else {
                    flag4 = true;
                    formSize = new Size(width + 8, sizeActual.Height + 8);
                }
                try {
                    SuspendLayout();
                    if(flag4) {
                        lblInfo.Dock = DockStyle.None;
                    }
                    else {
                        lblInfo.Dock = DockStyle.Bottom;
                        lblInfo.BringToFront();
                    }
                    pictureBox1.SuspendLayout();
                    pictureBox1.SizeMode = (sizeActual != bitmap.Size) ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
                    pictureBox1.Image = bitmap;
                    pictureBox1.ResumeLayout();
                    pictureBox1.BringToFront();
                    ResumeLayout();
                    return true;
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                    return false;
                }
            }
            if(ExtIsText(ext)) { // 如果预览的是文本文件
                FileInfo textFileInfo = new FileInfo(path);
                if(textFileInfo.Exists) {
                    try {
                        SizeF sizeF;
                        bool fLoadedAll = false;
                        bool isEmptyText = false;
                        string content;
                        ioException = null;
                        // 加载预览的逻辑

                        /*if (textFileInfo.Length > 0L && textFileInfo.Length <= MAX_TEXT_LENGTH)
                        {
                            content = LoadTextFile(path, out fLoadedAll);
                        }
                        else if (textFileInfo.Length > 0L && textFileInfo.Length > MAX_TEXT_LENGTH)
                        {
                            content = LoadTextFile(path, MAX_TEXT_LENGTH, out fLoadedAll);
                        }*/

                        if (textFileInfo.Length > 0L)
                        {
                            // content = LoadTextFile2(path, out fLoadedAll);
                            content = LoadTextFile3(path, out fLoadedAll);
                        }
                        else {
                            isEmptyText = true;
                            // str4 = "  *empty file";
                            content = EMPTYFILE;
                        }
                        lblText.ForeColor = (ioException != null) ? Color.Red : (isEmptyText ? SystemColors.GrayText : SystemColors.InfoText);
                        try {
                            lblText.Font = Config.Tips.PreviewFont;
                            fFontAsigned = true;
                        }
                        catch (Exception e)
                        {
                            QTUtility2.MakeErrorLog(e, "ExtIsText");
                            fFontAsigned = false;
                        }
                        int num2 = 0x100;
                        if(fFontAsigned) {
                            num2 = Math.Max((int)(num2 * (Config.Tips.PreviewFont.SizeInPoints / DefaultFont.Size)), 0x80);
                            formSize.Width = num2;
                        }
                        using(Graphics graphics2 = lblText.CreateGraphics()) {
                            sizeF = graphics2.MeasureString(content, lblText.Font, num2);
                        }
                        if((sizeF.Height < 512f) || fLoadedAll) {
                            formSize.Height = (int)(sizeF.Height + 8f);
                        }
                        else {
                            formSize.Height = 0x200;
                        }
                        SuspendLayout();
                        lblInfo.Dock = DockStyle.None;
                        lblText.Text = content;
                        lblText.BringToFront();
                        ResumeLayout();
                        return true;
                    }
                    catch(Exception exception2) {
                        QTUtility2.MakeErrorLog(exception2, null);
                        return false;
                    }
                }
            }
            return false;
        }

        protected override void Dispose(bool disposing) {
            imageCacheStore.Clear();
            base.Dispose(disposing);
        }

        private static bool ExtIsDefaultImage(string ext) {
            return GetGDIPSupportedImages().Contains(ext.ToLower());
        }

        private static bool ExtIsImage(string ext) {
            return (ext.Length != 0 && Config.Tips.ImageExt.Contains(ext.ToLower()) && ext != ".ico");
        }

        public static bool ExtIsSupported(string ext) {
            if(!ExtIsImage(ext)) {
                return ExtIsText(ext);
            }
            return true;
        }

        private static bool ExtIsText(string ext) {
            return (ext.Length != 0 && Config.Tips.TextExt.Contains(ext.ToLower()));
        }

        private static string FormatSize(long size) {
            string str = size + " bytes";
            if(size >= 0x400L) {
                str = Math.Round(((size) / 1024.0), 1) + " KB";
            }
            if(size >= 0x100000L) {
                str = Math.Round(((size) / 1048576.0), 1) + " MB";
            }
            return str;
        }

        private static string GetGDIPSupportedImages() {
            if(supportedImages == null) {
                ImageCodecInfo[] imageDecoders = ImageCodecInfo.GetImageDecoders();
                StringBuilder builder = new StringBuilder();
                foreach(ImageCodecInfo info in imageDecoders) {
                    builder.Append(info.FilenameExtension + ";");
                }
                supportedImages = builder.ToString()
                    .ToLower()
                    .Replace("*", string.Empty)
                    .Replace(".ico;", string.Empty);
            }
            return supportedImages;
        }

        public bool HideToolTip() {
            if(fIsShownByKey) {
                fIsShownByKey = false;
                return false;
            }
            isShowing = false;
            PInvoke.ShowWindow(Handle, 0);
            pictureBox1.Image = null;
            if(ThumbnailVisibleChanged != null) {
                ThumbnailVisibleChanged(this, new QEventArgs(ArrowDirection.Down));
            }
            return true;
        }

        private void InitializeComponent() {
            pictureBox1 = new PictureBox();
            lblText = new Label();
            lblInfo = new Label();
            ((ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            lblInfo.ForeColor = SystemColors.InfoText;
            lblInfo.BackColor = Color.Transparent;
            lblInfo.Dock = DockStyle.Bottom;
            lblInfo.Padding = new Padding(4);
            lblInfo.Size = new Size(0x10, 50);
            lblInfo.UseMnemonic = false;
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Padding = new Padding(4);
            pictureBox1.Size = new Size(0x100, 0x80);
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.TabStop = false;
            lblText.AutoEllipsis = true;
            lblText.ForeColor = SystemColors.InfoText;
            lblText.BackColor = Color.Transparent;
            lblText.Dock = DockStyle.Fill;
            lblText.Location = new Point(0, 0);
            lblText.Padding = new Padding(4);
            lblText.Size = new Size(0x100, 0x80);
            lblText.UseMnemonic = false;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Info;
            ClientSize = new Size(0x100, 0x80);
            Controls.Add(lblText);
            Controls.Add(pictureBox1);
            Controls.Add(lblInfo);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            ((ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        private static ImageData LoadImageFile(string path, DateTime dtLastWriteTime, out Size sizeRaw, out Size sizeActual) {
            sizeRaw = sizeActual = Size.Empty;
            using(Bitmap bitmap = new Bitmap(path)) {
                if(bitmap != null) {
                    sizeRaw = bitmap.Size;
                    int width = sizeRaw.Width;
                    int height = sizeRaw.Height;
                    int maxWidth = Config.Tips.PreviewMaxWidth;
                    int maxHeight = Config.Tips.PreviewMaxHeight;
                    if((height > maxHeight) || (width > maxWidth)) {
                        if(height > maxHeight) {
                            width = (int)((maxHeight / ((double)height)) * width);
                            height = maxHeight;
                            if(width > maxWidth) {
                                height = (int)((maxWidth / ((double)width)) * height);
                                width = maxWidth;
                            }
                        }
                        else {
                            height = (int)((maxWidth / ((double)width)) * height);
                            width = maxWidth;
                        }
                        sizeActual = new Size(width, height);
                        if(ImageAnimator.CanAnimate(bitmap)) {
                            MemoryStream stream = new MemoryStream();
                            bitmap.Save(stream, bitmap.RawFormat);
                            var imgObj = new ImageData(new Bitmap(stream), stream, path, dtLastWriteTime, sizeRaw, sizeActual);
                            QTUtility2.Close(stream);
                            return imgObj;
                        }
                        return new ImageData(new Bitmap(bitmap, width, height), null, path, dtLastWriteTime, sizeRaw, sizeActual);
                    }
                    sizeActual = sizeRaw;
                    MemoryStream stream2 = new MemoryStream();
                    bitmap.Save(stream2, bitmap.RawFormat);
                    var imgObj2 =  new ImageData(new Bitmap(stream2), stream2, path, dtLastWriteTime, sizeRaw, sizeRaw);
                    QTUtility2.Close(stream2);
                    return imgObj2;
                }
            }
            return null;
        }

        /// <summary> 
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型 
        /// </summary> 
        /// <param name=“FILE_NAME“>文件路径</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// <summary> 
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name=“fs“>文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            if (r != null) {
                r.Close();
               // r.Dispose();
            }
            
           // QTUtility2.Close(streamReader);
            return reVal;

        } 

        /// <summary> 
        /// 判断是否带BOM的UTF8格式（估算方法）
        /// BOM：Byte Order Mark，定义字节顺序。
        /// UTF-8不需要BOM表明字节顺序，但用BOM来表示编码方式。
        /// Windows就是采用BOM来标记文本文件的编码方式的，
        /// 可以把UTF-8和ASCII等编码区分开来，
        /// 但在Windows之外（如，Linux ），会带来问题。
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        private static bool IsUTF8Bytes(byte[] data)
        {
            // 字节数
            int charByteCounter = 1;
            // 当前字节
            byte curByte;
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        // 判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        // 标记位首位若为非0 则至少以2个1开始
                        // 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // 若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("FileLoader.IsUTF8Bytes ERROR: UNEXPECTED byte FORMAT!");
            }
            return true;
        }
       

        private static string LoadTextFile(string path, int count, out bool fLoadedAll)
        {
            using (StreamReader sr = new StreamReader(path, GetType(path)))
            {
                char[] chars = new char[count];
                int readCnt = sr.Read(chars, 0, count);
                string text = new string(chars, 0 , readCnt );
                fLoadedAll = false;
                // QTUtility2.Close(sr);
                return text;
            }
        }

        private static string LoadTextFile(string path, out bool fLoadedAll)
        {
            using (StreamReader sr = new StreamReader(path, GetType(path)))
            {
                string textall = sr.ReadToEnd();
                fLoadedAll = true;
                // QTUtility2.Close(sr);
                return textall;
            }
        }

        private static string LoadTextFile3(string path, out bool fLoadedAll)
        {
            byte[] buffer = null;
            int count = MAX_TEXT_LENGTH;
            string str = string.Empty;
            fLoadedAll = false;
            Encoding detechted = null;
            try
            {
                // using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                /*using (var reader = new StreamReader(path, Encoding.Default, true))
                {
                    if (reader.Peek() >= 0) // you need this!
                        reader.Read();

                    detechted = reader.CurrentEncoding;
                }*/
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read , count, FileOptions.Asynchronous))
                {
                    if (stream.Length < count)
                    {
                        fLoadedAll = true;
                        count = (int)stream.Length;
                    }
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    // detechted = detechBytes(buffer, true);
                    // detechted = detechBytes2(buffer, true);
                }
            }
            catch (IOException exception)
            {
                ioException = exception;
                return "  *Access Error!";
            }
            if (buffer.Length <= 0)
            {
                return str;
            }

            detechted = TryGetEncoding(buffer);
            if (detechted != null)
            {
                QTUtility2.log(" try get encoding " + detechted.EncodingName + " " + detechted.CodePage);
                return detechted.GetString(buffer);
            }

            // detechted = DetectInputCodepage(buffer);
            detechted = DetectEncoding(buffer);
            if (detechted != null)
            {
                // QTUtility2.log(" try get DetectInputCodepage " + detechted.EncodingName + " " + detechted.CodePage);
                QTUtility2.log(" try get DetectEncoding " + detechted.EncodingName + " " + detechted.CodePage);
                return detechted.GetString(buffer);
            }
            return Encoding.Default.GetString(buffer);
        }

        public static Encoding DetectEncoding(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length == 0))
            {
                return Encoding.Default;
            }
            MultiLanguage.IMultiLanguage2 lang = new MultiLanguage.CMultiLanguageClass();
            // IMultiLanguage2 lang = (IMultiLanguage2)new MultiLanguage();
            int len = bytes.Length;
            DetectEncodingInfo info = new DetectEncodingInfo();
            int scores = 1;

            // bytes to IntPtr
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr pbytes = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);

            try
            {
                // setup options (none)   
                MultiLanguage.MLDETECTCP options = MultiLanguage.MLDETECTCP.MLDETECTCP_NONE;
                lang.DetectInputCodepage(0, 0,  pbytes, ref len, out info, ref scores);
            }
            catch
            {
                info.nCodePage = (uint)Encoding.Default.CodePage;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(lang);
            }
            if (info.nCodePage == Encoding.ASCII.CodePage)
            {
                //ASCIIのときはUTF-8にする
                return Encoding.UTF8;
            }
            return Encoding.GetEncoding((int)info.nCodePage);
        }


        /// <summary>
        /// Detect the most probable codepage from an byte array
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <returns>the detected encoding or the default encoding if the detection failed</returns>
        public static Encoding DetectInputCodepage(byte[] input)
        {
            try
            {
                Encoding[] detected = DetectInputCodepages(input, 1);
                if (detected.Length > 0)
                    return detected[0];
                return Encoding.Default;
            }
            catch (COMException)
            {
                // return default codepage on error
                return Encoding.Default;
            }
        }

        /// <summary>
        /// Rerurns up to maxEncodings codpages that are assumed to be apropriate
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <param name="maxEncodings">maxiumum number of encodings to detect</param>
        /// <returns>an array of Encoding with assumed encodings</returns>
        public static Encoding[] DetectInputCodepages(byte[] input, int maxEncodings)
        {
            // StopWatch.Start("DetectInputCodepages_" + Thread.CurrentThread.ManagedThreadId);

            if (maxEncodings < 1)
                throw new ArgumentOutOfRangeException("at least one encoding must be returend", "maxEncodings");

            if (input == null)
                throw new ArgumentNullException("input");

            // empty strings can always be encoded as Default ASCII
            if (input.Length == 0)
                return new Encoding[] { Encoding.ASCII };

            // expand the string to be at least 256 bytes
            if (input.Length < 256)
            {
                byte[] newInput = new byte[256];
                int steps = 256 / input.Length;
                for (int i = 0; i < steps; i++)
                    Array.Copy(input, 0, newInput, input.Length * i, input.Length);

                int rest = 256 % input.Length;
                if (rest > 0)
                    Array.Copy(input, 0, newInput, steps * input.Length, rest);
                input = newInput;
            }

            List<Encoding> result = new List<Encoding>();

            // get the IMultiLanguage" interface
            MultiLanguage.IMultiLanguage2 multilang2 = new MultiLanguage.CMultiLanguageClass();
            if (multilang2 == null)
                throw new System.Runtime.InteropServices.COMException("Failed to get IMultilang2");
            try
            {
                MultiLanguage.DetectEncodingInfo[] detectedEncdings = new MultiLanguage.DetectEncodingInfo[maxEncodings];

                int scores = detectedEncdings.Length;
                int srcLen = input.Length;

                // setup options (none)   
                MultiLanguage.MLDETECTCP options = MultiLanguage.MLDETECTCP.MLDETECTCP_NONE;

                // finally... call to DetectInputCodepage
                multilang2.DetectInputCodepage(options, 0,
                    ref input[0], ref srcLen, ref detectedEncdings[0], ref scores);

                // get result
                if (scores > 0)
                {
                    for (int i = 0; i < scores; i++)
                    {
                        // add the result
                        result.Add(Encoding.GetEncoding((int)detectedEncdings[i].nCodePage));
                    }
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang2);
            }
            // nothing found
            return result.ToArray();
        }

        /**
         * codepage=936 简体中文GBK
            codepage=950 繁体中文BIG5
            codepage=437 美国/加拿大英语
            codepage=932 日文
            codepage=949 韩文
            codepage=866 俄文
         */
        public static Encoding TryGetEncoding(byte[] bytes)
        {
            Encoding encoding = CheckBom(bytes);
            if (encoding != null)
            {
                return encoding;
            }

            // Now check for valid UTF8
            encoding = CheckUtf8(bytes);
            if (encoding != null)
            {
                return encoding;
            }

            // Now try UTF16 
            encoding = CheckUtf16NewlineChars(bytes);
            if (encoding != null)
            {
                return encoding;
            }

            encoding = CheckUtf16Ascii(bytes);
            if (encoding != null)
            {
                return encoding;
            }

            // ANSI or None (binary) then
            if (!DoesContainNulls(bytes))
            {
                return Encoding.Default;
            }
           
            if (bytes.Length > 4)
            {
                if (bytes[0] == (byte)0 && bytes[1] == (byte)0 && bytes[2] == (byte)254 && bytes[3] == byte.MaxValue)
                {
                    return (Encoding)new UTF32Encoding(true, true);
                }
                if (bytes[0] == byte.MaxValue && bytes[1] == (byte)254 && bytes[2] == (byte)0 && bytes[3] == (byte)0)
                {
                    return (Encoding)new UTF32Encoding(false, true);
                }
            }
            return null;
        }


        /// <summary>
        /// Checks if a buffer contains any nulls. Used to check for binary vs text data.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        private static bool DoesContainNulls(byte[] buffer)
        {
            int size = buffer.Length;
            uint pos = 0;
            while (pos < size)
            {
                if (buffer[pos++] == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a buffer contains text that looks like utf16. This is done based
        ///     on the use of nulls which in ASCII/script like text can be useful to identify.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <returns>Encoding.none, Encoding.Utf16LeNoBom or Encoding.Utf16BeNoBom.</returns>
        private static Encoding CheckUtf16Ascii(byte[] buffer)
        {
            int size = buffer.Length;
            var numOddNulls = 0;
            var numEvenNulls = 0;

            // Get even nulls
            uint pos = 0;
            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    numEvenNulls++;
                }

                pos += 2;
            }

            // Get odd nulls
            pos = 1;
            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    numOddNulls++;
                }

                pos += 2;
            }

            double evenNullThreshold = numEvenNulls * 2.0 / size;
            double oddNullThreshold = numOddNulls * 2.0 / size;
            double expectedNullThreshold = _utf16ExpectedNullPercent / 100.0;
            double unexpectedNullThreshold = _utf16UnexpectedNullPercent / 100.0;

            // Lots of odd nulls, low number of even nulls
            if (evenNullThreshold < unexpectedNullThreshold && oddNullThreshold > expectedNullThreshold)
            {
                return EncodingUTF16LeNoBom;
            }

            // Lots of even nulls, low number of odd nulls
            if (oddNullThreshold < unexpectedNullThreshold && evenNullThreshold > expectedNullThreshold)
            {
                return EncodingUTF16BeNoBom;
            }

            // Don't know
            return null;
        }


        /// <summary>
        ///     Checks if a buffer contains text that looks like utf16 by scanning for
        ///     newline chars that would be present even in non-english text.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <returns>Encoding.none, Encoding.Utf16LeNoBom or Encoding.Utf16BeNoBom.</returns>
        private static Encoding CheckUtf16NewlineChars(byte[] buffer)
        {
            int size = buffer.Length;
            if (size < 2)
            {
                return null;
            }

            // Reduce size by 1 so we don't need to worry about bounds checking for pairs of bytes
            size--;

            var leControlChars = 0;
            var beControlChars = 0;

            uint pos = 0;
            while (pos < size)
            {
                byte ch1 = buffer[pos++];
                byte ch2 = buffer[pos++];

                if (ch1 == 0)
                {
                    if (ch2 == 0x0a || ch2 == 0x0d)
                    {
                        ++beControlChars;
                    }
                }
                else if (ch2 == 0)
                {
                    if (ch1 == 0x0a || ch1 == 0x0d)
                    {
                        ++leControlChars;
                    }
                }

                // If we are getting both LE and BE control chars then this file is not utf16
                if (leControlChars > 0 && beControlChars > 0)
                {
                    return null;
                }
            }

            if (leControlChars > 0)
            {
                return EncodingUTF16LeNoBom;
            }

            return beControlChars > 0 ? EncodingUTF16BeNoBom : null;
        }

        /// <summary>
        ///     Checks if a buffer contains valid utf8.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>
        ///     Encoding type of Encoding.None (invalid UTF8), Encoding.Utf8NoBom (valid utf8 multibyte strings) or
        ///     Encoding.ASCII (data in 0.127 range).
        /// </returns>
        /// <returns>2</returns>
        private static  Encoding CheckUtf8(byte[] buffer)
        {
            // UTF8 Valid sequences
            // 0xxxxxxx  ASCII
            // 110xxxxx 10xxxxxx  2-byte
            // 1110xxxx 10xxxxxx 10xxxxxx  3-byte
            // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx  4-byte
            //
            // Width in UTF8
            // Decimal      Width
            // 0-127        1 byte
            // 194-223      2 bytes
            // 224-239      3 bytes
            // 240-244      4 bytes
            //
            // Subsequent chars are in the range 128-191
            var onlySawAsciiRange = true;
            uint pos = 0;
            int size = buffer.Length;
            while (pos < size)
            {
                byte ch = buffer[pos++];

                if (ch == 0 && _nullSuggestsBinary)
                {
                    return null;
                }

                int moreChars;
                if (ch <= 127)
                {
                    // 1 byte
                    moreChars = 0;
                }
                else if (ch >= 194 && ch <= 223)
                {
                    // 2 Byte
                    moreChars = 1;
                }
                else if (ch >= 224 && ch <= 239)
                {
                    // 3 Byte
                    moreChars = 2;
                }
                else if (ch >= 240 && ch <= 244)
                {
                    // 4 Byte
                    moreChars = 3;
                }
                else
                {
                    return null; // Not utf8
                }

                // Check secondary chars are in range if we are expecting any
                while (moreChars > 0 && pos < size)
                {
                    onlySawAsciiRange = false; // Seen non-ascii chars now

                    ch = buffer[pos++];
                    if (ch < 128 || ch > 191)
                    {
                        return null; // Not utf8
                    }

                    --moreChars;
                }
            }

            // If we get to here then only valid UTF-8 sequences have been processed

            // If we only saw chars in the range 0-127 then we can't assume UTF8 (the caller will need to decide)
            return onlySawAsciiRange ? null : EncodingUTF8NoBom;
        }


        public static Encoding CheckBom(byte[] buffer)
        {
            int size = buffer.Length;
            // Check for BOM
            if (size >= 2 && buffer[0] == _utf16LeBom[0] && buffer[1] == _utf16LeBom[1])
            {
                return EncodingUTF16LeBom; ;
            }

            if (size >= 2 && buffer[0] == _utf16BeBom[0] && buffer[1] == _utf16BeBom[1])
            {
                return EncodingUTF16BeBom;
            }

            if (size >= 3 && buffer[0] == _utf8Bom[0] && buffer[1] == _utf8Bom[1] && buffer[2] == _utf8Bom[2])
            {
                return EncodingUTF8Bom;
            }

            return null;
        }


        public static Encoding GetEncoding2(byte[] bytes)
        {
            if ((bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF))
            {
                return Encoding.UTF8;
            }
            else if (bytes[0] == 0xFE && bytes[1] == 0xFF && bytes[2] == 0x00)
            {
                return Encoding.BigEndianUnicode;
            }
            else if (bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x41)
            {
                return Encoding.Unicode;
            }
            else if (IsTragetEncoding(bytes, Encoding.UTF8))
            {
                return Encoding.UTF8;
            }
            else if (IsTragetEncoding(bytes, Encoding.Unicode))
            {
                return Encoding.Unicode;
            }
            else if (IsTragetEncoding(bytes, Encoding936))
            {
                return Encoding936;
            }
            else if (IsTragetEncoding(bytes, Encoding950))
            {
                return Encoding950;
            }
            else
            {
                return Encoding.Default;
            }
        }

        private static byte[] UTF8Preamble = Encoding.UTF8.GetPreamble();

        private static bool IsUtf8Bom(byte[] buffer)
        {
            var isUtf8Bom = false;
            if (buffer != null && buffer.Length > 2)
            {
                if (buffer[0] == UTF8Preamble[0]
                    && buffer[1] == UTF8Preamble[1]
                    && buffer[2] == UTF8Preamble[2])
                {
                    isUtf8Bom = true;
                }
            }
            return isUtf8Bom;
        }

        private static byte[] RemoveBom(byte[] buffer)
        {
            if (buffer != null && buffer.Length > 2)
            {
                byte[] bomBuffer = Encoding.UTF8.GetPreamble();
                while (IsUtf8Bom(buffer))
                {
                    buffer = buffer.Skip(3).ToArray();
                }
            }
            return buffer;
        }

        public static bool IsTragetEncoding(byte[] bytes, Encoding targetEncoding)
        {
            //將byte[]轉為string再轉回byte[]看位元數是否有變
            var stringWithTragetEncoding = targetEncoding.GetString(bytes);
            var bytesWithTragetEncodingCount = targetEncoding.GetByteCount(stringWithTragetEncoding);
            return bytes.Length == bytesWithTragetEncodingCount;
        }

        /// <summary> 
        /// 判断文件流的编码类型 
        /// </summary> 
        /// <param name="filestream">文件流</param> 
        /// <returns>流的编码类型</returns> 
        private static Encoding GetStreamEncoding(byte[] ss)
        {
            try
            {
                byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
                byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
                //带BOM 
                byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF };
                Encoding reVal = Encoding.Default;
                if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
                {
                    reVal = Encoding.UTF8;
                }
                else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                {
                    reVal = Encoding.BigEndianUnicode;
                }
                else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                {
                    reVal = Encoding.Unicode;
                }
                return reVal;
            }
            catch (Exception ex)
            {
                throw new Exception("FileLoader.GetStreamEncoding ERROR:" + ex.Message);
            }
        }

        public static System.Text.Encoding detechBytes2(byte[] bytes, bool thorough)
        {
            if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF) // UTF32-BE 
                return System.Text.Encoding.GetEncoding("utf-32BE"); // UTF-32, big-endian 
            else if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00) // UTF32-LE
                return System.Text.Encoding.UTF32; // UTF-32, little-endian
            // https://en.wikipedia.org/wiki/Byte_order_mark#cite_note-14    
            else if (bytes.Length >= 4 && bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76 && (bytes[3] == 0x38 || bytes[3] == 0x39 || bytes[3] == 0x2B || bytes[3] == 0x2F)) // UTF7
                return System.Text.Encoding.UTF7;  // UTF-7
            else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) // UTF-8
                return System.Text.Encoding.UTF8;  // UTF-8
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) // UTF16-BE
                return System.Text.Encoding.BigEndianUnicode; // UTF-16, big-endian
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) // UTF16-LE
                return System.Text.Encoding.Unicode; // UTF-16, little-endian

            Encoding encoding = null;
            String text = null;
            // Test UTF8 with BOM. This check can easily be copied and adapted
            // to detect many other encodings that use BOMs.
            UTF8Encoding encUtf8Bom = new UTF8Encoding(true, true);
            Boolean couldBeUtf8 = true;
            Byte[] preamble = encUtf8Bom.GetPreamble();
            Int32 prLen = preamble.Length;
            if (bytes.Length >= prLen && preamble.SequenceEqual(bytes.Take(prLen)))
            {
                // UTF8 BOM found; use encUtf8Bom to decode.
                try
                {
                    // Seems that despite being an encoding with preamble,
                    // it doesn't actually skip said preamble when decoding...
                    return encUtf8Bom;
                }
                catch (ArgumentException)
                {
                    // Confirmed as not UTF-8!
                    couldBeUtf8 = false;
                }
            }
            // use boolean to skip this if it's already confirmed as incorrect UTF-8 decoding.
            if (couldBeUtf8 && encoding == null)
            {
                // test UTF-8 on strict encoding rules. Note that on pure ASCII this will
                // succeed as well, since valid ASCII is automatically valid UTF-8.
                UTF8Encoding encUtf8NoBom = new UTF8Encoding(false, true);
                try
                {
                    return encUtf8NoBom;
                }
                catch (ArgumentException)
                {
                    // Confirmed as not UTF-8!
                }
            }
            // fall back to default ANSI encoding.
            return Encoding.Default;
        }

        public static System.Text.Encoding detechBytes(byte[] b, bool thorough)
        {
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) // UTF32-BE 
                return System.Text.Encoding.GetEncoding("utf-32BE"); // UTF-32, big-endian 
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) // UTF32-LE
                return System.Text.Encoding.UTF32; // UTF-32, little-endian
            // https://en.wikipedia.org/wiki/Byte_order_mark#cite_note-14    
            else if (b.Length >= 4 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76 && (b[3] == 0x38 || b[3] == 0x39 || b[3] == 0x2B || b[3] == 0x2F)) // UTF7
                return System.Text.Encoding.UTF7;  // UTF-7
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) // UTF-8
                return System.Text.Encoding.UTF8;  // UTF-8
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) // UTF16-BE
                return System.Text.Encoding.BigEndianUnicode; // UTF-16, big-endian
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) // UTF16-LE
                return System.Text.Encoding.Unicode; // UTF-16, little-endian

            // Maybe there is a future encoding ...
            // PS: The above yields more than this - this doesn't find UTF7 ...
            if (thorough)
            {
                System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.Text.Encoding, byte[]>> lsPreambles =
                    new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.Text.Encoding, byte[]>>();

                foreach (System.Text.EncodingInfo ei in System.Text.Encoding.GetEncodings())
                {
                    System.Text.Encoding enc = ei.GetEncoding();

                    byte[] preamble = enc.GetPreamble();

                    if (preamble == null)
                        continue;

                    if (preamble.Length == 0)
                        continue;

                    if (preamble.Length > b.Length)
                        continue;

                    System.Collections.Generic.KeyValuePair<System.Text.Encoding, byte[]> kvp =
                        new System.Collections.Generic.KeyValuePair<System.Text.Encoding, byte[]>(enc, preamble);

                    lsPreambles.Add(kvp);
                } // Next ei

                // li.Sort((a, b) => a.CompareTo(b)); // ascending sort
                // li.Sort((a, b) => b.CompareTo(a)); // descending sort
                lsPreambles.Sort(
                    delegate(
                        System.Collections.Generic.KeyValuePair<System.Text.Encoding, byte[]> kvp1,
                        System.Collections.Generic.KeyValuePair<System.Text.Encoding, byte[]> kvp2)
                    {
                        return kvp2.Value.Length.CompareTo(kvp1.Value.Length);
                    }
                );


                for (int j = 0; j < lsPreambles.Count; ++j)
                {
                    for (int i = 0; i < lsPreambles[j].Value.Length; ++i)
                    {
                        if (b[i] != lsPreambles[j].Value[i])
                        {
                            goto NEXT_J_AND_NOT_NEXT_I;
                        }
                    } // Next i 

                    return lsPreambles[j].Key;
                NEXT_J_AND_NOT_NEXT_I: continue;
                } // Next j 

            } // End if (thorough)

            return Encoding.Default;
        } // End Function BomInfo 


        private static string LoadTextFile2(string path, out bool fLoadedAll) {
            byte[] buffer;
            int count = MAX_TEXT_LENGTH;
            string str = string.Empty;
            fLoadedAll = false;
            try {
                using(FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    if(stream.Length < count) {
                        fLoadedAll = true;
                        count = (int)stream.Length;
                    }
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    // QTUtility2.Close(stream);
                }
            }
            catch(IOException exception) {
                ioException = exception;
                return "  *Access Error!";
            }
            if(buffer.Length <= 0) {
                return str;
            }
            Encoding encoding = null;
            if(PluginManager.IEncodingDetector != null) {
                try {
                    encoding = PluginManager.IEncodingDetector.GetEncoding(ref buffer);
                }
                catch(Exception exception2) {
                    PluginManager.HandlePluginException(exception2, IntPtr.Zero, "Unknown IEncodingDetector", "Getting Encoding object.");
                    QTUtility2.MakeErrorLog(exception2);
                }
            }
            if(encoding == null) {
                encoding = TxtEnc.GetEncoding(ref buffer);

                QTUtility2.log("TxtEnc :" + encoding.EncodingName + " " + encoding.CodePage);

                if((encoding == null) ||
                   (((
                         (Encoding.Default.CodePage != 0x3a4) &&
                         (encoding.CodePage != 0xfde8)) && 
                     ((encoding.CodePage != 0xfde9) && 
                      (encoding.CodePage != 0x4b0))) 
                    && (encoding.CodePage != 0x2ee0))) {
                    encoding = Encoding.Default;
                }
            }
            QTUtility2.log("Final :" + encoding.EncodingName + " " + encoding.CodePage);
            return encoding.GetString(buffer);
        }

        private static ImageData LoadThumbnail(string path, DateTime dtLastWriteTime, out Size sizeRaw, out Size sizeActual, out string toolTipText, out bool fCached) {
            sizeRaw = sizeActual = Size.Empty;
            toolTipText = null;
            fCached = false;
            IntPtr zero = IntPtr.Zero;
            IShellItem ppsi = null;
            ISharedBitmap ppvThumb = null;
            LocalThumbnailCache o = null;
            try {
                zero = PInvoke.ILCreateFromPath(path);
                if((zero != IntPtr.Zero) && (PInvoke.SHCreateShellItem(IntPtr.Zero, null, zero, out ppsi) == 0)) {
                    o = new LocalThumbnailCache();
                    IThumbnailCache cache2 = (IThumbnailCache)o;
                    uint flags = 0;
                    uint pOutFlags = 0;
                    WTS_THUMBNAILID pThumbnailID = new WTS_THUMBNAILID();
                    uint cxyRequestedThumbSize = (uint)Math.Min(0x400, Math.Min(Config.Tips.PreviewMaxWidth, Config.Tips.PreviewMaxHeight));
                    if(cache2.GetThumbnail(ppsi, cxyRequestedThumbSize, flags, out ppvThumb, ref pOutFlags, ref pThumbnailID) == 0) {
                        IntPtr ptr2;
                        if((pOutFlags & 2) == 2) {
                            fCached = true;
                        }
                        if(ppvThumb.Detach(out ptr2) == 0) {
                            Bitmap bmp = Image.FromHbitmap(ptr2);
                            Size size = bmp.Size;
                            sizeRaw = sizeActual = size;
                            ImageData data = new ImageData(bmp, null, path, dtLastWriteTime, size, size);
                            data.Thumbnail = true;
                            try {
                                toolTipText = data.TooltipText = ShellMethods.GetShellInfoTipText(zero, false);
                            }
                            catch (Exception e)
                            {
                                QTUtility2.MakeErrorLog(e, "LoadThumbnail GetShellInfoTipText");
                            }
                            return data;
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
            finally {
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ppsi != null) {
                    QTUtility2.log("ReleaseComObject ppsi");
                    Marshal.ReleaseComObject(ppsi);
                }
                if(ppvThumb != null) {
                    QTUtility2.log("ReleaseComObject ppvThumb");
                    Marshal.ReleaseComObject(ppvThumb);
                }
                if(o != null) {
                    QTUtility2.log("ReleaseComObject o");
                    Marshal.ReleaseComObject(o);
                }
            }
            return null;
        }

        private static ImageData LoadThumbnail2(string path, DateTime dtLastWriteTime, out Size sizeRaw, out Size sizeActual, out string toolTipText, out bool fCached) {
            sizeRaw = sizeActual = Size.Empty;
            toolTipText = null;
            fCached = false;
            IntPtr zero = IntPtr.Zero;
            IShellFolder ppv = null;
            object obj2 = null;
            try {
                IntPtr ptr3;
                zero = PInvoke.ILCreateFromPath(path);
                if((zero != IntPtr.Zero) && (PInvoke.SHBindToParent(zero, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr3) == 0)) {
                    uint rgfReserved = 0;
                    Guid riid = ExplorerGUIDs.IID_IExtractImage;
                    IntPtr[] apidl = new IntPtr[] { ptr3 };
                    if(ppv.GetUIObjectOf(IntPtr.Zero, 1, apidl, ref riid, ref rgfReserved, out obj2) == 0) {
                        IntPtr ptr2;
                        IExtractImage image = (IExtractImage)obj2;
                        StringBuilder pszPathBuffer = new StringBuilder(260);
                        int pdwPriority = 0;
                        Size prgSize = new Size(Config.Tips.PreviewMaxWidth, Config.Tips.PreviewMaxHeight);
                        int pdwFlags = 0x60;
                        if(((image.GetLocation(pszPathBuffer, pszPathBuffer.Capacity, ref pdwPriority, ref prgSize, 0x18, ref pdwFlags) == 0) && (image.Extract(out ptr2) == 0)) && (ptr2 != IntPtr.Zero)) {
                            Bitmap bmp = Image.FromHbitmap(ptr2);
                            Size size = bmp.Size;
                            sizeRaw = sizeActual = size;
                            ImageData data = new ImageData(bmp, null, path, dtLastWriteTime, size, size);
                            data.Thumbnail = true;
                            try {
                                toolTipText = data.TooltipText = ShellMethods.GetShellInfoTipText(zero, false);
                            }
                            catch (Exception e)
                            {
                                QTUtility2.MakeErrorLog(e, "GetShellInfoTipText");
                            }
                            return data;
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
            finally {
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ppv != null) {
                    QTUtility2.log("ReleaseComObject ppv");
                    Marshal.ReleaseComObject(ppv);
                }
                if(obj2 != null) {
                    QTUtility2.log("ReleaseComObject obj2");
                    Marshal.ReleaseComObject(obj2);
                }
            }
            return null;
        }

        internal static List<string> MakeDefaultImgExts() {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetGDIPSupportedImages());
            builder.Append(supportedMovies);
            var strs = builder.ToString();
            if (QTUtility.IsEmptyStr(strs))
            {
                return new List<string>();
            }
            return new List<string>(strs.Split(QTUtility.SEPARATOR_CHAR));
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(!QTUtility.IsXP && VisualStyleRenderer.IsSupported) {
                new VisualStyleRenderer(VisualStyleElement.ToolTip.Standard.Normal).DrawBackground(e.Graphics, new Rectangle(0, 0, Width, Height));
            }
            else {
                base.OnPaintBackground(e);
                e.Graphics.DrawRectangle(SystemPens.InfoText, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        public bool ShowToolTip(string path, Point pnt) {
            Size formSize = new Size(0x100, 0x80);
            if(!CreateThumbnail(path, ref formSize)) {
                return false;
            }
            Rectangle workingArea = Screen.FromPoint(pnt).WorkingArea;
            int num = ((formSize.Width + pnt.X) - workingArea.Right) + 8;
            int num2 = ((formSize.Height + pnt.Y) - workingArea.Bottom) + 0x10;
            bool flag = (workingArea.Right - pnt.X) < (pnt.X - workingArea.Left);
            bool flag2 = (workingArea.Bottom - pnt.Y) < (pnt.Y - workingArea.Top);
            bool flag3 = false;
            if((num > 0) && (num2 > 0)) {
                if(flag) {
                    pnt.X -= ((formSize.Width + 0x18) + 0x20) + 0x10;
                }
                if(flag2) {
                    pnt.Y -= formSize.Height + 0x20;
                    flag3 = true;
                }
            }
            else if(num > 0) {
                if(flag) {
                    pnt.X -= num + 0x20;
                }
            }
            else if(num2 > 0) {
                pnt.Y -= num2 + 0x10;
                flag3 = true;
            }
            if(pnt.X < workingArea.X) {
                pnt.X = workingArea.X + 8;
                if(!flag3) {
                    pnt.Y += 8;
                }
            }
            isShowing = true;
            PInvoke.SetWindowPos(Handle, (IntPtr)(-1), pnt.X + 0x18, pnt.Y + 0x10, formSize.Width, formSize.Height, 0x10);
            PInvoke.ShowWindow(Handle, 4);
            if(ThumbnailVisibleChanged != null) {
                ThumbnailVisibleChanged(this, new QEventArgs(ArrowDirection.Up));
            }
            return true;
        }

        public bool ShowToolTip(string path, Rectangle rctMenuItem) {
            Size formSize = new Size(0x100, 0x80);
            if(!CreateThumbnail(path, ref formSize)) {
                return false;
            }
            Point point = new Point(rctMenuItem.Right + 8, rctMenuItem.Bottom);
            Rectangle workingArea = Screen.FromPoint(rctMenuItem.Location).WorkingArea;
            bool flag = (workingArea.Right - point.X) < (point.X - workingArea.Left);
            bool flag2 = false;
            if(((((formSize.Width + point.X) - workingArea.Right) + 8) > 0) && flag) {
                point.X = (rctMenuItem.X - formSize.Width) - 8;
            }
            if((((formSize.Height + point.Y) - workingArea.Bottom) + 8) > 0) {
                point.Y = (workingArea.Bottom - formSize.Height) - 0x10;
                flag2 = true;
            }
            if(point.X < workingArea.X) {
                point.X = workingArea.X + 8;
                if(!flag2) {
                    point.Y += 8;
                }
            }
            isShowing = true;
            PInvoke.SetWindowPos(Handle, (IntPtr)(-1), point.X, point.Y, formSize.Width, formSize.Height, 0x10);
            PInvoke.ShowWindow(Handle, 4);
            return true;
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                createParams.ClassStyle |= 0x20000;
                return createParams;
            }
        }

        public bool IsShowing {
            get {
                return isShowing;
            }
        }

        public bool IsShownByKey {
            get {
                return fIsShownByKey;
            }
            set {
                fIsShownByKey = value;
            }
        }

        private sealed class ImageCacheStore : Collection<ImageData> {
            private int max_cache_length;
            private object syncObject = new object();

            public ImageCacheStore(int max_cache_length) {
                this.max_cache_length = max_cache_length;
            }

            protected override void ClearItems() {
                lock(syncObject) {
                    foreach(ImageData data in this) {
                        data.Dispose();
                    }
                    base.ClearItems();
                }
            }

            protected override void InsertItem(int index, ImageData item) {
                lock(syncObject) {
                    base.InsertItem(index, item);
                    if(Count > max_cache_length) {
                        base[0].Dispose();
                        base.RemoveItem(0);
                    }
                }
            }

            protected override void RemoveItem(int index) {
                lock(syncObject) {
                    base[index].Dispose();
                    base.RemoveItem(index);
                }
            }

            protected override void SetItem(int index, ImageData item) {
                lock(syncObject) {
                    base.SetItem(index, item);
                }
            }
        }

        private sealed class ImageData : IDisposable {
            public Bitmap Bitmap;
            public DateTime ModifiedDate;
            public MemoryStream ms;
            public string Path;
            public Size RawSize;
            public bool Thumbnail;
            public string TooltipText;
            public Size ZoomedSize;

            public ImageData(Bitmap bmp, MemoryStream memoryStream, string path, DateTime dtModified, Size sizeRaw, Size sizeZoomed) {
                Bitmap = bmp;
                ms = memoryStream;
                Path = path;
                ModifiedDate = dtModified;
                RawSize = sizeRaw;
                ZoomedSize = sizeZoomed;
            }

            public void Dispose() {
                try {
                    if(Bitmap != null) {
                        Bitmap.Dispose();
                        Bitmap = null;
                    }
                    if(ms != null) {
                        ms.Dispose();
                        ms = null;
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
            }
        }
    }
}
