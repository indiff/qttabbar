using System;
using System.Globalization;
using System.Resources;
using QTPlugin;

namespace Qwop
{
    internal static class QTQuickLocalization
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("Qwop.Resource", typeof(QTQuickLocalization).Assembly);

        private static CultureInfo Culture
        {
            get
            {
                return PluginCulture.GetDefaultCulture();
            }
        }

        internal static string Get(string key)
        {
            string resourceKey = Culture.TwoLetterISOLanguageName.Equals(
                "zh", StringComparison.OrdinalIgnoreCase)
                ? key + ".zh-CN"
                : key;
            string value = ResourceManager.GetString(resourceKey, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(value) ? key : value;
        }
    }
}
