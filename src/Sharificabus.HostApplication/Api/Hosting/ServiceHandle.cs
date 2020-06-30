using System;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;

namespace Sharificabus.HostApplication.Api.Hosting
{
    internal sealed class ServiceHandle : IServiceHandle, IDisposable
    {
        private readonly ILog _log;
        private readonly Action<ServiceHandle, IAppBuilder> _configure;

        private IDisposable _webApplication;

        public ServiceHandle(
            string description,
            ILog log,
            IHostingSettingsProvider hostingSettingsProvider,
            Action<IServiceHandle, IAppBuilder> configure,
            string serviceSuffix)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(
                    @"The value can be neither empty nor whitespace-only string nor null.",
                    nameof(description));
            }

            Description = description;
            _log = log ?? throw new ArgumentNullException(nameof(log));
            HostingSettingsProvider = hostingSettingsProvider
                ?? throw new ArgumentNullException(nameof(hostingSettingsProvider));
            ServiceSuffix = serviceSuffix ?? throw new ArgumentNullException(nameof(serviceSuffix));
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));

            ServiceRegistrationUrl = hostingSettingsProvider.GetServiceRegistrationUrl(serviceSuffix);
            ServiceUrl = hostingSettingsProvider.GetServiceUrl(serviceSuffix);
        }

        public ServiceHandle(
            string description,
            ILog log,
            IHostingSettingsProvider hostingSettingsProvider,
            Action<IServiceHandle, IAppBuilder> configure)
            : this(description, log, hostingSettingsProvider, configure, string.Empty)
        {
            // Nothing to do
        }

        public string Description
        {
            get;
        }

        public IHostingSettingsProvider HostingSettingsProvider
        {
            get;
        }

        public string ServiceSuffix
        {
            get;
        }

        public string ServiceRegistrationUrl
        {
            get;
        }

        public string ServiceUrl
        {
            get;
        }

        public void Start()
        {
            if (_webApplication != null)
            {
                throw new InvalidOperationException($@"The service '{Description}' has already started.");
            }

            _webApplication = WebApp.Start(ServiceRegistrationUrl, appBuilder => _configure(this, appBuilder));

            _log.Info(
                $@"The Web service '{Description}' hosted at '{ServiceUrl}' has started (registration: '{
                    ServiceRegistrationUrl}').");
        }

        public void Dispose()
        {
            if (_webApplication == null)
            {
                return;
            }

            _webApplication.Dispose();
            _webApplication = null;

            _log.Info($@"The Web service '{Description}' hosted at '{ServiceUrl}' has stopped.");
        }
    }
}