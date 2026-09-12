// NewsSourceConfig.cs
namespace QuizoPlugins
{
    public class NewsSourceConfig
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public bool NeedTranslate { get; set; } = true; // 是否需要英译中

        public override string ToString() => DisplayName;
    }
}