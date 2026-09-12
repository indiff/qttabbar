using System;
using System.Globalization;

namespace QTPlugin {
    public static class PluginCulture {
        public static CultureInfo Normalize(CultureInfo culture) {
            if(culture == null) {
                return CultureInfo.GetCultureInfo("en-US");
            }

            var name = culture.Name;
            if(string.IsNullOrEmpty(name)) {
                return CultureInfo.GetCultureInfo("en-US");
            }

            name = name.ToLowerInvariant();
            if(name.StartsWith("zh")) {
                return CultureInfo.GetCultureInfo("zh-CN");
            }

            if(name.StartsWith("ja")) {
                return CultureInfo.GetCultureInfo("ja-JP");
            }

            if(name.StartsWith("en")) {
                return CultureInfo.GetCultureInfo("en-US");
            }

            return CultureInfo.GetCultureInfo("en-US");
        }

        public static CultureInfo GetDefaultCulture() {
            return Normalize(CultureInfo.CurrentUICulture);
        }

        public static void ApplyDefaultCulture() {
            var culture = GetDefaultCulture();
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public static bool IsChinese(CultureInfo culture) {
            culture = Normalize(culture);
            return culture.Name.Equals("zh-CN", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsJapanese(CultureInfo culture) {
            culture = Normalize(culture);
            return culture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsEnglish(CultureInfo culture) {
            culture = Normalize(culture);
            return culture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase);
        }
    }
}
