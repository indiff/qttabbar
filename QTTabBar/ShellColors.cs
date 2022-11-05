using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace QTTabBarLib
{
    public static class ShellColors
    {
        public static Color LightModeColor = Color.White;

        public static Color ControlMainColor = Color.FromArgb(41, 128, 204);

        public static Color NightModeColor = Color.Black;

        public static Color NightModeTreeViewBackColor = Color.FromArgb(25, 25, 25);

        public static Color NightModeLightColor = Color.FromArgb(43, 43, 43);

        public static Color NightModeTextColor = Color.White;

        public static Color NightModeBorderColor = Color.FromArgb(83, 83, 83);

        public static Color NightModeDisabledColor = Color.FromArgb(140, 140, 140);

        public static Color NightModeTabColor = Color.FromArgb(217, 217, 217);

        public static Color NightModeTextShadow = Color.Gray;

        public static Color NightModeViewBackColor = Color.FromArgb(32, 32, 32);

        public static Color NightModeViewSelectionColor = Color.FromArgb(98, 98, 98);

        public static Color NightModeViewSelectionColorInactive = Color.FromArgb(51, 51, 51);

        public static Color NightModeViewSelectedAndFocusedColor = Color.FromArgb(119, 119, 119);

        public static Color NightModeViewSelectedAndHiliteColor = Color.FromArgb(119, 119, 119);

        public static Color NightModeViewSelectedAndHiliteColorInactive = Color.FromArgb(119, 119, 119);

        public static Color NightModeViewHiliteColor = Color.FromArgb(77, 77, 77);

        public static Color NightModeViewHeaderHiliteColor = Color.FromArgb(67, 67, 67);

        public static Color FaceColor17666 = !QTUtility.InNightMode ? ShellColors.LightModeColor : ShellColors.NightModeColor;

        public static Color NightModeOptionColor = Color.FromArgb(44, 44, 44);

        private static ShellColors.ShellColorSet colorSet = ShellColors.Create();

        public static Color Light
        {
            get
            {
                return ShellColors.colorSet.Light;
            }
        }


        public static Color Default
        {
            get
            {
                return ShellColors.colorSet.Default;
            }
        } 

        public static Color Text {
            get
            {
                return ShellColors.colorSet.Text;
            }
        } 

        public static Color Border {
            get
            {
                return ShellColors.colorSet.Border;
            }
        } 

        public static Color Separator {
            get
            {
                return ShellColors.colorSet.Separator;
            }
        } 

        public static Color Disabled {
            get
            {
                return ShellColors.colorSet.Disabled;
            }
        } 

        public static Color Tab {
            get
            {
                return ShellColors.colorSet.Tab;
            }
        } 

        public static Color TextShadow {
            get
            {
                return ShellColors.colorSet.TextShadow;

            }
        } 

        public static void Refresh()
        {
            ShellColors.colorSet = ShellColors.Create();
        } 

        private static ShellColors.ShellColorSet Create()
        {
            if (!QTUtility.InNightMode)
                return new ShellColors.ShellColorSet();
            return QTUtility.IsWin11 ?
                new ShellColors.Windows10Dark() : 
                new ShellColors.Windows11Dark();
        }

        private class ShellColorSet
        {
          public  Color Default = Color.White;

          public  Color TreeViewBack = Color.White;

          public  Color Light = Color.FromArgb(242, 242, 242);

          public  Color Text = Color.Black;

          public  Color Border = Color.FromArgb(217, 217, 217);

          public  Color Separator = Color.FromKnownColor(KnownColor.GrayText);

          public  Color Disabled = Color.Gray;

          public  Color Tab;

          public Color TextShadow;

          public Color ViewBack;

          public Color ViewSelection;

          public  Color ViewSelectionInactive ;
          public Color ViewSelectionAndFocused;

          public Color ViewSelectionAndHilite;

          public Color ViewSelectionAndHiliteInactive;

          public Color ViewHilite;

          public Color ViewHeaderHilite;

          public Color Option;

          public  Color MenuSelection = Color.FromArgb(217, 217, 217);
        }

        private class Windows10Dark : ShellColors.ShellColorSet
        {
          public  Color Default = Color.Black;

          public  Color TreeViewBack = Color.FromArgb(25, 25, 25);

          public  Color Light = Color.FromArgb(43, 43, 43);

          public  Color Text = Color.White;

          public  Color Border = Color.FromArgb(83, 83, 83);

          public  Color Disabled = Color.FromArgb(140, 140, 140);

          public  Color Separator = Color.FromArgb(140, 140, 140);

          public  Color Tab = Color.FromArgb(217, 217, 217);

          public  Color TextShadow = Color.Gray;

          public  Color ViewBack = Color.FromArgb(32, 32, 32);

          public  Color ViewSelection = Color.FromArgb(98, 98, 98);

          public  Color ViewSelectionInactive = Color.FromArgb(51, 51, 51);

          public  Color ViewSelectionAndFocused = Color.FromArgb(119, 119, 119);

          public  Color ViewSelectionAndHilite = Color.FromArgb(119, 119, 119);

          public  Color ViewSelectionAndHiliteInactive = Color.FromArgb(119, 119, 119);

          public  Color ViewHilite = Color.FromArgb(77, 77, 77);

          public  Color ViewHeaderHilite = Color.FromArgb(67, 67, 67);

          public  Color Option = Color.FromArgb(44, 44, 44);

          public  Color MenuSelection = Color.FromArgb(65, 65, 65);
        }

        private class Windows11Dark : ShellColors.Windows10Dark
        {
          public  Color Default  = Color.FromArgb(30, 32, 35);

          public  Color TreeViewBack = Color.FromArgb(25, 25, 25);

          public  Color Light = Color.FromArgb(44, 44, 44);

          public  Color Border = Color.FromArgb(62, 62, 62);

          public  Color Separator = Color.FromArgb(62, 62, 62);

          public  Color Tab = Color.FromArgb(169, 169, 169);

          public  Color MenuSelection = Color.FromArgb(51, 51, 51);
        }
    }



}
