using System;
using System.ServiceProcess;
using log4net;
using Omnifactotum.Annotations;
using Sharificabus.HostApplication.Api.Hosting;

namespace Sharificabus.HostApplication
{
    internal sealed partial class SharificabusService : ServiceBase
    {
        private readonly ILog _log;
        private readonly ApiHost _apiHost;

        public SharificabusService([NotNull] ILog log, [NotNull] ApiHost apiHost)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _apiHost = apiHost ?? throw new ArgumentNullException(nameof(apiHost));

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.Info($@"Starting service '{nameof(SharificabusService)}'.");
            _apiHost.Start();
        }

        protected override void OnStop()
        {
            _log.Info($@"Stopping service '{nameof(SharificabusService)}'.");
            _apiHost.Dispose();
        }
    }
}