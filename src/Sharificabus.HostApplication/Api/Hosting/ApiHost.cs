using System;
using System.Web.Http.Dependencies;
using log4net;
using Microsoft.Practices.Unity;
using Omnifactotum.Annotations;

namespace Sharificabus.HostApplication.Api.Hosting
{
    internal sealed class ApiHost : IDisposable
    {
        private readonly UnityContainer _container;
        private readonly ServiceHandle _serviceHandle;

        public ApiHost([NotNull] UnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            RegisterDependencies(_container);

            _container.Resolve<IHostingSettings>().Validate();

            var log = _container.Resolve<ILog>();
            var hostingSettingsProvider = _container.Resolve<IHostingSettingsProvider>();
            var dependencyResolver = _container.Resolve<IDependencyResolver>();

            _serviceHandle = new ServiceHandle(
                Constants.ApiTitle,
                log,
                hostingSettingsProvider,
                (handle, builder) => builder.UseSharificabusApi(log, dependencyResolver));
        }

        public IServiceHandle[] GetServiceHandles() => new IServiceHandle[] { _serviceHandle };

        public void Start()
        {
            _serviceHandle.Start();
        }

        public void Dispose()
        {
            _serviceHandle.Dispose();
            _container.Dispose();
        }

        private static void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IHostingSettings, HostingSettings>(new ContainerControlledLifetimeManager());
            container.RegisterType<IHostingSettingsProvider, HostingSettingsProvider>(
                new ContainerControlledLifetimeManager());

            container.RegisterType<ApiExceptionHandler>(new ContainerControlledLifetimeManager());
        }
    }
}