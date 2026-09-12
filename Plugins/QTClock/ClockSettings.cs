using System;
using Microsoft.Win32;

namespace QuizoPlugins
{
    internal enum NewsDockSide
    {
        Left = 0,
        Right = 1
    }

    internal static class ClockSettings
    {
        private const string RegistryPath = @"Software\QTTabBar\QTClock";
        private const string DockSideValue = "NewsDockSide";

        public static NewsDockSide DockSide { get; private set; }

        static ClockSettings()
        {
            Load();
        }

        public static void Load()
        {
            DockSide = NewsDockSide.Right;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null)
                    {
                        return;
                    }

                    int value = Convert.ToInt32(key.GetValue(DockSideValue, (int)NewsDockSide.Right));
                    if (Enum.IsDefined(typeof(NewsDockSide), value))
                    {
                        DockSide = (NewsDockSide)value;
                    }
                }
            }
            catch
            {
                DockSide = NewsDockSide.Right;
            }
        }

        public static void Save(NewsDockSide dockSide)
        {
            DockSide = dockSide;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue(DockSideValue, (int)dockSide, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
