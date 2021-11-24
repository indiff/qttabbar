using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace QTTabBarLib
{
    internal static class ShellColors
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
    }
}
