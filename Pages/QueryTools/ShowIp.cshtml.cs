using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class ShowIpModel : PageModel
    {
        private readonly ILogger<ShowIpModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShowIpModel(ILogger<ShowIpModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string ClientIp { get; private set; } = "";
        public bool LookupOk { get; private set; }
        public string ErrorMessage { get; private set; } = "";
        public string LookupNote { get; private set; } = "";
        public IpLookupResult Info { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ClientIp = GetClientIp(HttpContext) ?? "";
            var usePublicFallback = string.IsNullOrWhiteSpace(ClientIp) || IsPrivateOrLoopback(ClientIp);

            if (usePublicFallback)
            {
                LookupNote = string.IsNullOrWhiteSpace(ClientIp)
                    ? "未能识别访问 IP，已改为查询当前网络的公网地址。"
                    : "当前为本地或内网访问（" + ClientIp + "），已改为查询当前网络的公网 IP。";
            }

            try
            {
                var lookupUrl = usePublicFallback
                    ? "https://ipwho.is/"
                    : $"https://ipwho.is/{Uri.EscapeDataString(ClientIp)}";

                var payload = await FetchIpInfoAsync(lookupUrl);
                if (payload == null || !payload.Success)
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(payload?.Message)
                        ? "IP 归属地查询失败，请稍后重试。"
                        : payload.Message;
                    return Page();
                }

                LookupOk = true;
                Info = MapLookupResult(payload, usePublicFallback ? payload.Ip : ClientIp);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IP lookup failed for {Ip}", ClientIp);
                ErrorMessage = "查询服务暂时不可用，请稍后重试。";
            }

            return Page();
        }

        private async Task<IpWhoIsResponse?> FetchIpInfoAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ToolApp/1.0");

            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<IpWhoIsResponse>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static IpLookupResult MapLookupResult(IpWhoIsResponse payload, string? fallbackIp)
        {
            return new IpLookupResult
            {
                Ip = payload.Ip ?? fallbackIp ?? "—",
                Country = payload.Country ?? "—",
                Region = payload.Region ?? "—",
                City = payload.City ?? "—",
                Isp = payload.Connection?.Isp ?? "—",
                Org = payload.Connection?.Org ?? "—",
                Timezone = payload.Timezone?.Id ?? "—",
                FlagEmoji = payload.Flag?.Emoji ?? ""
            };
        }

        private static string? GetClientIp(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                var first = forwarded.Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(first))
                {
                    return first;
                }
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp.Trim();
            }

            var remote = context.Connection.RemoteIpAddress;
            if (remote == null)
            {
                return null;
            }

            if (remote.IsIPv4MappedToIPv6)
            {
                remote = remote.MapToIPv4();
            }

            return remote.ToString();
        }

        private static bool IsPrivateOrLoopback(string ipText)
        {
            if (!IPAddress.TryParse(ipText, out var ip))
            {
                return false;
            }

            if (IPAddress.IsLoopback(ip))
            {
                return true;
            }

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = ip.GetAddressBytes();
                return bytes[0] == 10
                    || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    || (bytes[0] == 192 && bytes[1] == 168)
                    || (bytes[0] == 169 && bytes[1] == 254);
            }

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return ip.IsIPv6LinkLocal || ip.IsIPv6UniqueLocal || ip.IsIPv6SiteLocal;
            }

            return false;
        }

        public sealed class IpLookupResult
        {
            public string Ip { get; set; } = "";
            public string Country { get; set; } = "";
            public string Region { get; set; } = "";
            public string City { get; set; } = "";
            public string Isp { get; set; } = "";
            public string Org { get; set; } = "";
            public string Timezone { get; set; } = "";
            public string FlagEmoji { get; set; } = "";
        }

        private sealed class IpWhoIsResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public string? Ip { get; set; }
            public string? Country { get; set; }
            public string? Region { get; set; }
            public string? City { get; set; }
            public IpWhoIsConnection? Connection { get; set; }
            public IpWhoIsTimezone? Timezone { get; set; }
            public IpWhoIsFlag? Flag { get; set; }
        }

        private sealed class IpWhoIsConnection
        {
            public string? Isp { get; set; }
            public string? Org { get; set; }
        }

        private sealed class IpWhoIsTimezone
        {
            public string? Id { get; set; }
        }

        private sealed class IpWhoIsFlag
        {
            public string? Emoji { get; set; }
        }
    }
}
