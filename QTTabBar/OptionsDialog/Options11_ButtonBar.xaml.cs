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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using QTPlugin;
using Size = System.Drawing.Size;

namespace QTTabBarLib {
    internal partial class Options11_ButtonBar : OptionsDialogTab {
        private ImageStrip imageStripLarge;
        private ImageStrip imageStripSmall;
        private ObservableCollection<ButtonEntry> ButtonPool;
        private ObservableCollection<ButtonEntry> CurrentButtons;

        public Options11_ButtonBar() {
            InitializeComponent();
        }


        internal static bool LoadExternalImage(string path, out Bitmap bmpLarge, out Bitmap bmpSmall)
        {
            bmpLarge = (bmpSmall = null);
            if (File.Exists(path))
            {
                try
                {
                    using (Bitmap bitmap = new Bitmap(path))
                    {
                        // if ((bitmap.Width >= 0x1b0) && (bitmap.Height >= 40))
                        /* if ((bitmap.Width >= 0x1b0) && (bitmap.Height >= 0x18))
                         {
                             bmpLarge = bitmap.Clone(new Rectangle(0, 0, 0x1b0, 0x18), PixelFormat.Format32bppArgb);
                             bmpSmall = bitmap.Clone(new Rectangle(0, 0x18, 0x120, 0x10), PixelFormat.Format32bppArgb);
                             return true;
                         }*/

                        if ((bitmap.Width >= 504) && (bitmap.Height >= 24))
                        {
                            bmpLarge = bitmap.Clone(new Rectangle(0, 0, 504, 24), PixelFormat.Format32bppArgb);
                            // bmpSmall = bitmap.Clone(new Rectangle(0, 0x18, 0x120, 0x10), PixelFormat.Format32bppArgb);
                            bmpSmall = (Bitmap)ResizeBitMap(bmpLarge, 336, 16);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex);
                }
            }
            return false;
        }

        internal static Image ResizeBitMap(Bitmap original, int desiredWidth, int desiredHeight)
        {
            //throw error if bouning box is to small
            if (desiredWidth < 4 || desiredHeight < 4)
                throw new InvalidOperationException("Bounding Box of Resize Photo must be larger than 4X4 pixels.");

            //store image widths in variable for easier use
            var oW = (decimal)original.Width;
            var oH = (decimal)original.Height;
            var dW = (decimal)desiredWidth;
            var dH = (decimal)desiredHeight;

            //check if image already fits
            if (oW < dW && oH < dH)
                return original; //image fits in bounding box, keep size (center with css) If we made it bigger it would stretch the image resulting in loss of quality.

            //check for double squares
            if (oW == oH && dW == dH)
            {
                //image and bounding box are square, no need to calculate aspects, just downsize it with the bounding box
                Bitmap square = new Bitmap(original, (int)dW, (int)dH);
                // original.Dispose();
                return square;
            }

            //check original image is square
            if (oW == oH)
            {
                //image is square, bounding box isn't.  Get smallest side of bounding box and resize to a square of that center the image vertically and horizontally with Css there will be space on one side.
                int smallSide = (int)Math.Min(dW, dH);
                Bitmap square = new Bitmap(original, smallSide, smallSide);
                // original.Dispose();
                return square;
            }

            //not dealing with squares, figure out resizing within aspect ratios            
            if (oW > dW && oH > dH) //image is wider and taller than bounding box
            {
                var r = Math.Min(dW, dH) / Math.Min(oW, oH); //two dimensions so figure out which bounding box dimension is the smallest and which original image dimension is the smallest, already know original image is larger than bounding box
                var nH = oH * r; //will downscale the original image by an aspect ratio to fit in the bounding box at the maximum size within aspect ratio.
                var nW = oW * r;
                var resized = new Bitmap(original, (int)nW, (int)nH);
                //  original.Dispose();
                return resized;
            }
            else
            {
                if (oW > dW) //image is wider than bounding box
                {
                    var r = dW / oW; //one dimension (width) so calculate the aspect ratio between the bounding box width and original image width
                    var nW = oW * r; //downscale image by r to fit in the bounding box...
                    var nH = oH * r;
                    var resized = new Bitmap(original, (int)nW, (int)nH);
                    //  original.Dispose();
                    return resized;
                }
                else
                {
                    //original image is taller than bounding box
                    var r = dH / oH;
                    var nH = oH * r;
                    var nW = oW * r;
                    var resized = new Bitmap(original, (int)nW, (int)nH);
                    //  original.Dispose();
                    return resized;
                }
            }
        }

        public override void InitializeConfig() {
            // Initialize the button bar tab.
            imageStripLarge = new ImageStrip(new Size(24, 24));
            imageStripSmall = new ImageStrip(new Size(16, 16));
            if ( null != WorkingConfig.bbar.ImageStripPath && WorkingConfig.bbar.ImageStripPath.Length > 0 )
            {
                Bitmap bitmap;
                Bitmap bitmap2;
                if (LoadExternalImage(WorkingConfig.bbar.ImageStripPath, out bitmap, out bitmap2))
                {
                    imageStripLarge.AddStrip(bitmap);
                    imageStripSmall.AddStrip(bitmap2);
                    bitmap.Dispose();
                    bitmap2.Dispose();
                    if (Path.GetExtension(WorkingConfig.bbar.ImageStripPath ).PathEquals(".bmp"))
                    {
                        imageStripLarge.TransparentColor = imageStripSmall.TransparentColor = Color.Magenta;
                    }
                    else
                    {
                        imageStripLarge.TransparentColor = imageStripSmall.TransparentColor = Color.Empty;
                    }
                }
            } else
            {
                using (Bitmap b = Resources_Image.ButtonStrip24)
                {
                    imageStripLarge.AddStrip(b);
                }
                imageStripSmall = new ImageStrip(new Size(16, 16));
                using (Bitmap b = Resources_Image.ButtonStrip16)
                {
                    imageStripSmall.AddStrip(b);
                }
            }
            
            ButtonPool = new ObservableCollection<ButtonEntry>();
            CurrentButtons = new ObservableCollection<ButtonEntry>();

            // Create a list of all the plugin buttons.
            int order = QTButtonBar.INTERNAL_BUTTON_COUNT;
            var lstPluginIDs = new List<string>();
            var dicPluginButtons = new Dictionary<string, ButtonEntry[]>();
            foreach(PluginInformation pi in PluginManager.PluginInformations.Where(pi => pi.Enabled).OrderBy(pi => pi.Name)) {
                if(pi.PluginType == PluginType.Interactive) {
                    lstPluginIDs.Add(pi.PluginID);
                    dicPluginButtons[pi.PluginID] = new ButtonEntry[] { new ButtonEntry(this, order++, 0, pi) };
                }
                else if(pi.PluginType == PluginType.BackgroundMultiple) {
                    Plugin plugin;
                    if(PluginManager.TryGetStaticPluginInstance(pi.PluginID, out plugin)) {
                        IBarMultipleCustomItems bmci = plugin.Instance as IBarMultipleCustomItems;
                        try {
                            if(bmci != null && bmci.Count > 0) {
                                lstPluginIDs.Add(pi.PluginID);
                                dicPluginButtons[pi.PluginID] = 
                                    bmci.Count.RangeSelect(i => new ButtonEntry(this, order++, i, pi)).ToArray();
                            }
                        }
                        catch { }
                    }
                }
            }

            // Add the current buttons (right pane)
            foreach(int i in WorkingConfig.bbar.ButtonIndexes) {
                int pluginIndex = i.HiWord() - 1;
                if(pluginIndex >= 0) {
                    string id = WorkingConfig.bbar.ActivePluginIDs[pluginIndex];
                    ButtonEntry[] buttons;
                    if(dicPluginButtons.TryGetValue(id, out buttons) && i.LoWord() < buttons.Length) {
                        CurrentButtons.Add(buttons[i.LoWord()]);
                    }
                }
                else {
                    CurrentButtons.Add(new ButtonEntry(this, i, i));
                }
            }

            // Add the rest of the buttons to the button pool (left pane)
            ButtonPool.Add(new ButtonEntry(this, 0, QTButtonBar.BII_SEPARATOR));
            for(int i = 1; i < QTButtonBar.INTERNAL_BUTTON_COUNT; i++) {
                if(!WorkingConfig.bbar.ButtonIndexes.Contains(i)) {
                    ButtonPool.Add(new ButtonEntry(this, i, i));
                }
            }

            foreach(ButtonEntry entry in lstPluginIDs.SelectMany(pid => dicPluginButtons[pid]).Except(CurrentButtons)) {
                ButtonPool.Add(entry);
            }
            lstButtonBarPool.ItemsSource = ButtonPool;
            lstButtonBarCurrent.ItemsSource = CurrentButtons;
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.bbar = new Config._BBar();
            InitializeConfig();
        }

        public override void CommitConfig() {
            List<string> activeIDs = new List<string>();
            WorkingConfig.bbar.ButtonIndexes = CurrentButtons.Select(entry => {
                int p = 0;
                if(entry.PluginInfo != null) {
                    p = activeIDs.IndexOf(entry.PluginInfo.PluginID) + 1;
                    if(p == 0) {
                        activeIDs.Add(entry.PluginInfo.PluginID);
                        p = activeIDs.Count;
                    }
                    p <<= 16;

                }
                return p + entry.Index;
            }).ToArray();
            WorkingConfig.bbar.ActivePluginIDs = activeIDs.ToArray();
            // TODO: Validate image strip

            // MessageBox.Show("image:" + WorkingConfig.bbar.ImageStripPath);
          
            if (null != WorkingConfig.bbar.ImageStripPath && WorkingConfig.bbar.ImageStripPath.Trim().Length > 0)
            {
                imageStripLarge = new ImageStrip(new Size(24, 24));
                imageStripSmall = new ImageStrip(new Size(16, 16));

                Bitmap bitmap;
                Bitmap bitmap2;
                if (LoadExternalImage(WorkingConfig.bbar.ImageStripPath, out bitmap, out bitmap2))
                {
                    imageStripLarge.AddStrip(bitmap);
                    imageStripSmall.AddStrip(bitmap2);
                    bitmap.Dispose();
                    bitmap2.Dispose();
                    if (Path.GetExtension(WorkingConfig.bbar.ImageStripPath).PathEquals(".bmp"))
                    {
                        imageStripLarge.TransparentColor = imageStripSmall.TransparentColor = Color.Magenta;
                    }
                    else
                    {
                        imageStripLarge.TransparentColor = imageStripSmall.TransparentColor = Color.Empty;
                    }
                }
            }
        }

        private void btnBBarAdd_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarPool.SelectedIndex;
            if(sel == -1) return;
            ButtonEntry entry = ButtonPool[sel];
            if(entry.Order == QTButtonBar.BII_SEPARATOR) {
                entry = new ButtonEntry(this, 0, QTButtonBar.BII_SEPARATOR);
            }
            else {
                ButtonPool.RemoveAt(sel);
                if(sel == ButtonPool.Count) --sel;
                if(sel >= 0) {
                    lstButtonBarPool.SelectedIndex = sel;
                    lstButtonBarPool.ScrollIntoView(lstButtonBarPool.SelectedItem);
                }
            }
            if(lstButtonBarCurrent.SelectedIndex == -1) {
                CurrentButtons.Add(entry);
                lstButtonBarCurrent.SelectedIndex = CurrentButtons.Count - 1;
            }
            else {
                CurrentButtons.Insert(lstButtonBarCurrent.SelectedIndex + 1, entry);
                lstButtonBarCurrent.SelectedIndex++;
            }
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        private void btnBBarRemove_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel == -1) return;
            ButtonEntry entry = CurrentButtons[sel];
            CurrentButtons.RemoveAt(sel);
            if(sel == CurrentButtons.Count) --sel;
            if(sel >= 0) {
                lstButtonBarCurrent.SelectedIndex = sel;
                lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
            }
            if(entry.Order != QTButtonBar.BII_SEPARATOR) {
                int i = 0;
                while(i < ButtonPool.Count && ButtonPool[i].Order < entry.Order) ++i;
                ButtonPool.Insert(i, entry);
                lstButtonBarPool.SelectedIndex = i;
            }
            else {
                lstButtonBarPool.SelectedIndex = 0;
            }
            lstButtonBarPool.ScrollIntoView(lstButtonBarPool.SelectedItem);
        }

        private void btnBBarUp_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel <= 0) return;
            CurrentButtons.Move(sel, sel - 1);
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        private void btnBBarDown_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel == -1 || sel == CurrentButtons.Count - 1) return;
            CurrentButtons.Move(sel, sel + 1);
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class ButtonEntry : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            private Options11_ButtonBar parent;

            public PluginInformation PluginInfo { get; private set; }
            public int Index { get; private set; }
            public int Order { get; private set; }
            public bool IsPluginButton { get { return PluginInfo != null; } }
            public string PluginButtonText {
                get {
                    if(!IsPluginButton) return "";
                    if(PluginInfo.PluginType == PluginType.BackgroundMultiple) {
                        Plugin plugin;
                        if(PluginManager.TryGetStaticPluginInstance(PluginInfo.PluginID, out plugin)) {
                            try {
                                return ((IBarMultipleCustomItems)plugin.Instance).GetName(Index);
                            }
                            catch { }
                        }
                    }
                    return PluginInfo.Name;
                }
            }

            public Image LargeImage { get { return getImage(true); } }
            public Image SmallImage { get { return getImage(false); } }
            private Image getImage(bool large) {
                if(IsPluginButton) {
                    if(PluginInfo.PluginType == PluginType.BackgroundMultiple) {
                        Plugin plugin;
                        if(PluginManager.TryGetStaticPluginInstance(PluginInfo.PluginID, out plugin)) {
                            try {
                                return ((IBarMultipleCustomItems)plugin.Instance).GetImage(large, Index);
                            }
                            catch { }
                        }
                    }
                    return large
                            ? PluginInfo.ImageLarge ?? Resources_Image.imgPlugin24
                            : PluginInfo.ImageSmall ?? Resources_Image.imgPlugin16;
                }
                else if(Index == 0 || Index >= QTButtonBar.BII_WINDOWOPACITY) {
                    return null;
                }
                else {
                    return large
                            ? parent.imageStripLarge[Index - 1]
                            : parent.imageStripSmall[Index - 1];
                }
            }

            public ButtonEntry(Options11_ButtonBar parent, int Order, int Index, PluginInformation PluginInfo = null) {
                this.parent = parent;
                this.Order = Order;
                this.Index = Index;
                this.PluginInfo = PluginInfo;
            }
        }

        #endregion

    }
}
