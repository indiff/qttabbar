using System;
using System.Globalization;
using System.Resources;
using Microsoft.Win32;
using QTPlugin;

namespace QuizoPlugins
{
    public static class ClockLocalization
    {
        private const string RegistryPath = @"Software\QTTabBar\QTClock";
        private const string RegistryValueLanguage = "Language";
        private static readonly ResourceManager ResourceManager = new ResourceManager("QuizoPlugins.ClockStrings", typeof(ClockLocalization).Assembly);
        private static CultureInfo culture;

        public static CultureInfo Culture
        {
            get
            {
                if (culture == null || !string.Equals(culture.Name, CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    culture = ResolveCulture();
                }

                return culture;
            }
        }

        public static void ApplyCurrentCulture()
        {
            var current = Culture;
            CultureInfo.CurrentCulture = current;
            CultureInfo.CurrentUICulture = current;
        }

        public static string Get(string key)
        {
            try
            {
                return ResourceManager.GetString(key, Culture) ?? key;
            }
            catch (MissingManifestResourceException)
            {
                return key;
            }
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Culture, Get(key), args);
        }

        public static string[] GetList(string key)
        {
            var value = Get(key);
            return value == key ? new string[0] : value.Split(new[] { '|' }, StringSplitOptions.None);
        }

        public static string GetDayGreeting(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return Get("Greeting.Sunday");
                case DayOfWeek.Monday:
                    return Get("Greeting.Monday");
                case DayOfWeek.Tuesday:
                    return Get("Greeting.Tuesday");
                case DayOfWeek.Wednesday:
                    return Get("Greeting.Wednesday");
                case DayOfWeek.Thursday:
                    return Get("Greeting.Thursday");
                case DayOfWeek.Friday:
                    return Get("Greeting.Friday");
                default:
                    return Get("Greeting.Saturday");
            }
        }

        public static string GetWeekDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return Get("Week.Sunday");
                case DayOfWeek.Monday:
                    return Get("Week.Monday");
                case DayOfWeek.Tuesday:
                    return Get("Week.Tuesday");
                case DayOfWeek.Wednesday:
                    return Get("Week.Wednesday");
                case DayOfWeek.Thursday:
                    return Get("Week.Thursday");
                case DayOfWeek.Friday:
                    return Get("Week.Friday");
                default:
                    return Get("Week.Saturday");
            }
        }

        public static string TranslateHoliday(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var key = "Holiday." + name;
            var translated = ResourceManager.GetString(key, Culture);
            return string.IsNullOrEmpty(translated) ? name : translated;
        }

        public static CultureInfo ResolveCulture()
        {
            string overrideName = null;
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    overrideName = key.GetValue(RegistryValueLanguage) as string;
                }
            }

            if (!string.IsNullOrWhiteSpace(overrideName) && !overrideName.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return CultureInfo.GetCultureInfo(overrideName);
                }
                catch (CultureNotFoundException)
                {
                    return CultureInfo.InstalledUICulture;
                }
            }

            return QTPlugin.PluginCulture.Normalize(CultureInfo.CurrentUICulture);
        }

        public static void SetCulture(string cultureName)
        {
            culture = string.IsNullOrWhiteSpace(cultureName) || cultureName.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? ResolveCulture()
                : CultureInfo.GetCultureInfo(cultureName);
            ApplyCurrentCulture();
        }
    }

    public sealed class ClockStringProvider : LocalizedStringProvider
    {
        public override void SetKey(int iKey)
        {
            ClockLocalization.ApplyCurrentCulture();
        }

        public override string Author => ClockLocalization.Get("Plugin.Author");

        public override string Description => ClockLocalization.Get("Plugin.Description");

        public override string Name => ClockLocalization.Get("Plugin.Name");
    }
}
