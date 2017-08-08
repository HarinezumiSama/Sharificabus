using System;
using System.Diagnostics;
using System.Web.Http.Dependencies;
using log4net.Config;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Omnifactotum;
using Sharificabus.HostApplication.Api;

namespace Sharificabus.HostApplication
{
    internal static class Program
    {
        private const int FatalExitCode = -1;
        private const string ServiceUrl = "http://+:80/sharificabus";

        private static int Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += ProcessDomainUnhandledException;
                XmlConfigurator.Configure();

                using (var container = CreateContainer())
                {
                    if (Environment.UserInteractive)
                    {
                        Console.Title = Constants.AppFullName;
                    }

                    var dependencyResolver = container.Resolve<IDependencyResolver>();

                    WriteConsole();
                    WriteConsole(ConsoleColor.Green, Constants.AppFullName, true);
                    WriteConsole();

                    WriteConsole("The hosted Web services are starting...", true);
                    WriteConsole($@"Service registration: {ServiceUrl}");
                    var startOptions = new StartOptions(ServiceUrl);
                    using (WebApp.Start(startOptions, builder => builder.UseSharificabusApi(dependencyResolver)))
                    {
                        WriteConsole("The hosted Web services has started.", true);

                        WriteConsole();
                        WriteConsole(ConsoleColor.White, @"Press ESC to stop and exit.");
                        WaitForKey(ConsoleKey.Escape);

                        WriteConsole();
                        WriteConsole("The hosted Web services are stopping...", true);
                    }

                    WriteConsole("The hosted Web services has stopped.", true);
                }
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

            Constants.Logger.Fatal(
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
                Constants.Logger.Info(text);
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

            container.RegisterType<IDependencyResolver, UnityDependencyResolver>(
                new ContainerControlledLifetimeManager());

            return container;
        }
    }
}