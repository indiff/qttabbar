// TranslateService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace QuizoPlugins
{
    public static class TranslateService
    {

        private static readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        public static string EnToZh2(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            if (_cache.TryGetValue(text, out var cached)) return cached;
            var result = EnToZhAsync(text).GetAwaiter().GetResult();// 原翻译逻辑
            //var result = EnToZh_TMT_Async(text).GetAwaiter().GetResult(); 
            _cache[text] = result;
            return result;
        }

        private static readonly string ApiBase = "https://api.mymemory.translated.net/get";
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        // ★ 改为异步 + 超时 + 线程安全
        public static async Task<string> EnToZhAsync(string text, int timeoutMs = 5000)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            try
            {
                // 截断处理：保留原始尾部标记
                string suffix = null;
                string toTranslate = text;
                if (text.Length > 450)
                {
                    toTranslate = text.Substring(0, 450);
                    suffix = "...";
                }

                var url = $"{ApiBase}?q={WebUtility.UrlEncode(toTranslate)}&langpair=en|zh-CN";

                // ★ 放在调用 WebClient 之前（建议放在程序启动时或静态构造函数中）
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls13;

                // 忽略所有 SSL 证书验证
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var cts = new CancellationTokenSource(timeoutMs))
                using (var http = new WebClient())
                {

                    http.Encoding = Encoding.UTF8;
                    //var json = http.DownloadString(url);
                    var json = await http.DownloadStringTaskAsync(url);

                    // ★ 每次新建实例，避免并行下的线程安全问题
                    var serializer = new JavaScriptSerializer();
                    var root = serializer.Deserialize<Dictionary<string, object>>(json);
                    if (root == null || !root.ContainsKey("responseData")) return text;

                    var responseData = root["responseData"] as Dictionary<string, object>;
                    if (responseData == null || !responseData.ContainsKey("translatedText")) return text;

                    var translated = responseData["translatedText"] as string;
                    if (string.IsNullOrEmpty(translated)) return text;

                    // 拼接截断后缀
                    return suffix != null ? translated + suffix : translated;
                }
            }
            catch (Exception e)
            {
                return text + $" translate failed {e.Message}"; // 超时/异常 → 返回原文
            }
        }


        #region 腾讯翻译
        // ===== 腾讯云机器翻译 TMT 配置（必填） =====
        private const string SecretId = "";
        private const string SecretKey = "";
        private const string Region = "ap-guangzhou";
        private const string Action = "TextTranslate";
        private const string Version = "2018-03-21";
        private const string Host = "tmt.tencentcloudapi.com";
        private const string Service = "tmt";
        private static readonly string ApiBase_TMT = "https://tmt.tencentcloudapi.com";

        private static readonly JavaScriptSerializer _serializerTMT = new JavaScriptSerializer();


        // ★ 异步 + 超时
        public static async Task<string> EnToZh_TMT_Async(string text, int timeoutMs = 5000)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            try
            {
                // 截断处理：保留原始尾部标记
                string suffix = null;
                string toTranslate = text;
                if (text.Length > 450)
                {
                    toTranslate = text.Substring(0, 450);
                    suffix = "...";
                }

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

                // 业务参数：源语言 en -> 目标语言 zh（简体中文）
                var payload = _serializer.Serialize(new Dictionary<string, object>
                {
                    { "SourceText", toTranslate },
                    { "Source", "en" },
                    { "Target", "zh" },
                    { "ProjectId", 0 }
                });

                var authorization = BuildAuthorization(timestamp, date, payload);

                using (var cts = new CancellationTokenSource(timeoutMs))
                using (var http = new WebClient())
                {
                    http.Encoding = Encoding.UTF8;
                    http.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    http.Headers["X-TC-Action"] = Action;
                    http.Headers["X-TC-Version"] = Version;
                    http.Headers["X-TC-Timestamp"] = timestamp;
                    http.Headers["X-TC-Region"] = Region;
                    http.Headers["Authorization"] = authorization;

                    var json = await http.UploadStringTaskAsync(ApiBase_TMT , payload);

                    var root = _serializerTMT.Deserialize<Dictionary<string, object>>(json);
                    if (root == null || !root.ContainsKey("Response")) return text;

                    var response = root["Response"] as Dictionary<string, object>;
                    if (response == null || !response.ContainsKey("TargetText")) return text;

                    var translated = response["TargetText"] as string;
                    if (string.IsNullOrEmpty(translated)) return text;

                    // 拼接截断后缀
                    return suffix != null ? translated + suffix : translated;
                }
            }
            catch (Exception e)
            {
                return text + $" translate failed {e.Message}"; // 超时/异常 → 返回原文
            }
        }


        // ===== 腾讯云 API 3.0 签名（TC3-HMAC-SHA256） =====
        private static string BuildAuthorization(string timestamp, string date, string payload)
        {
            var canonicalUri = "/";
            var canonicalQueryString = "";
            var canonicalHeaders = $"content-type:application/json; charset=utf-8\nhost:{Host}\nx-tc-action:{Action.ToLower()}\n";
            var signedHeaders = "content-type;host;x-tc-action";
            var hashedRequestPayload = Sha256Hex(payload);

            var canonicalRequest = $"POST\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{hashedRequestPayload}";

            var credentialScope = $"{date}/{Service}/tc3_request";
            var hashedCanonicalRequest = Sha256Hex(canonicalRequest);
            var stringToSign = $"TC3-HMAC-SHA256\n{timestamp}\n{credentialScope}\n{hashedCanonicalRequest}";

            var secretDate = HmacSha256(Encoding.UTF8.GetBytes("TC3" + SecretKey), date);
            var secretService = HmacSha256(secretDate, Service);
            var secretSigning = HmacSha256(secretService, "tc3_request");
            var signature = Hex(HmacSha256(secretSigning, stringToSign));

            return $"TC3-HMAC-SHA256 Credential={SecretId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
        }

        private static string Sha256Hex(string data)
        {
            using (var sha = SHA256.Create())
                return Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }

        private static byte[] HmacSha256(byte[] key, string data)
        {
            using (var hmac = new HMACSHA256(key))
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        private static string Hex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
        #endregion
    }
}