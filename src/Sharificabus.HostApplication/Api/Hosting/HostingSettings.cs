using Sharificabus.HostApplication.Properties;

namespace Sharificabus.HostApplication.Api.Hosting
{
    internal sealed class HostingSettings : IHostingSettings
    {
        private readonly Settings _settings;

        public HostingSettings()
        {
            _settings = Settings.Default;
        }

        public bool IsSecure => _settings.ApiIsSecure;

        public int Port => _settings.ApiPort;

        public int SecurePort => _settings.ApiSecurePort;

        public string Suffix => _settings.ApiSuffix;

        public IHostingSettings Validate()
        {
            _settings.Validate();
            return this;
        }
    }
}