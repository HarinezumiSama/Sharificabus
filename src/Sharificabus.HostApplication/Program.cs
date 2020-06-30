using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Web.Http.Dependencies;
using log4net;
using log4net.Config;
using Microsoft.Practices.Unity;
using Omnifactotum;
using Sharificabus.HostApplication.Api;
using Sharificabus.HostApplication.Api.Hosting;
using Sharificabus.HostApplication.DataAccess;

namespace Sharificabus.HostApplication
{
    internal static class Program
    {
        private const int FatalExitCode = -1;

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static int Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += ProcessDomainUnhandledException;
                XmlConfigurator.Configure();

                WriteConsole();
                WriteConsole(ConsoleColor.Green, Constants.AppFullName, true);

                var mode = Environment.UserInteractive ? @"interactive" : @"service";
                WriteConsole(ConsoleColor.Yellow, $@"Application is starting ({mode} mode)...", true);

                using (var container = CreateContainer())
                {
                    using (var apiHost = new ApiHost(container))
                    {
                        if (Environment.UserInteractive)
                        {
                            Console.Title = Constants.AppFullName;

                            WriteConsole("The services are starting...", true);
                            apiHost.Start();

                            var serviceHandles = apiHost.GetServiceHandles();

                            serviceHandles.DoForEach(
                                handle => WriteConsole(
                                    $@"The hosted Web service '{handle.Description}' has started at '{
                                        handle.ServiceUrl}' (registration: '{handle.ServiceRegistrationUrl}')."));

                            WriteConsole(ConsoleColor.Green, @"Application has started.", true);

                            WriteConsole();
                            WriteConsole(ConsoleColor.White, @"Press ESC to stop and exit.");
                            WaitForKey(ConsoleKey.Escape);

                            WriteConsole();
                            WriteConsole(ConsoleColor.Yellow, @"Application is stopping...", true);
                        }
                        else
                        {
                            using (var sharificabusService = new SharificabusService(Log, apiHost))
                            {
                                ServiceBase.Run(sharificabusService);
                            }
                        }
                    }
                }

                WriteConsole();
                WriteConsole(ConsoleColor.Green, @"Application has stopped.", true);
                WriteConsole();
            }
            catch (Exception ex)
                when (!ex.IsFatal())
            {
                LogUnhandledException(ex);
                return FatalExitCode;
            }

            return 0;
        }

        private static void LogUnhandledException(Exception exception)
        {
            var errorDetails = exception?.ToString() ?? "(Unknown error)";

            WinEventLog.Write(EventLogEntryType.Error, $@"Unhandled exception has occurred: {errorDetails}");

            Log.Fatal(
                "*** Unhandled exception has occurred. The application will be terminated ***",
                exception);

            WriteConsole();
            WriteConsole(ConsoleColor.Red, "*** UNHANDLED EXCEPTION ***");
            WriteConsole(ConsoleColor.Red, errorDetails);
            WriteConsole();
        }

        private static void ProcessDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                LogUnhandledException(args.ExceptionObject as Exception);
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private static void WriteConsole(ConsoleColor? color, string text, bool writeToLog = false)
        {
            if (writeToLog)
            {
                Log.Info(text);
            }

            if (!Environment.UserInteractive)
            {
                return;
            }

            Console.ResetColor();
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteConsole() => WriteConsole(null, string.Empty);

        private static void WriteConsole(string text, bool writeToLog = false) => WriteConsole(null, text, writeToLog);

        private static void WaitForKey(ConsoleKey? key = null)
        {
            if (!Environment.UserInteractive || Console.IsInputRedirected)
            {
                return;
            }

            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (!key.HasValue ||
                    keyInfo.Key == key && keyInfo.Modifiers == 0)
                {
                    break;
                }
            }
        }

        private static UnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            container.RegisterInstance(Log);

            container.RegisterType<IDependencyResolver, UnityDependencyResolver>(
                new ContainerControlledLifetimeManager());

            container.RegisterType<INotificationRepository, NotificationRepository>(
                new ContainerControlledLifetimeManager());

            return container;
        }
    }
}