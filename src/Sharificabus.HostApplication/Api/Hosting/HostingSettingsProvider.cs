using System;
using System.Text;

namespace Sharificabus.HostApplication.Api.Hosting
{
    internal sealed class HostingSettingsProvider : IHostingSettingsProvider
    {
        public HostingSettingsProvider(IHostingSettings hostingSettings)
        {
            if (hostingSettings == null)
            {
                throw new ArgumentNullException(nameof(hostingSettings));
            }

            var scheme = hostingSettings.IsSecure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            var port = hostingSettings.IsSecure ? hostingSettings.SecurePort : hostingSettings.Port;
            var root = $@"/{hostingSettings.Suffix.TrimSafely()}".AppendSlash();

            string CreateUrl(string hostSpecifier) => $@"{scheme}://{hostSpecifier}:{port}{root}";

            RootUrl = CreateUrl(LocalHelper.GetHostName());
            RootRegistrationUrl = CreateUrl("+");
        }

        public string RootUrl
        {
            get;
        }

        public string RootRegistrationUrl
        {
            get;
        }

        public string GetServiceUrl(string serviceSuffix)
        {
            if (serviceSuffix == null)
            {
                throw new ArgumentNullException(nameof(serviceSuffix));
            }

            return BuildUrl(RootUrl, serviceSuffix);
        }

        public string GetServiceRegistrationUrl(string serviceSuffix)
        {
            if (serviceSuffix == null)
            {
                throw new ArgumentNullException(nameof(serviceSuffix));
            }

            return BuildUrl(RootRegistrationUrl, serviceSuffix);
        }

        private static string BuildUrl(string baseUrl, string suffix)
        {
            var resultBuilder = new StringBuilder(baseUrl);
            if (!string.IsNullOrEmpty(suffix))
            {
                resultBuilder.Append($@"{suffix}/");
            }

            return resultBuilder.ToString();
        }
    }
}