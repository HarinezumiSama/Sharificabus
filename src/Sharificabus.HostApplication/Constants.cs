using System;
using System.Reflection;
using log4net;

namespace Sharificabus.HostApplication
{
    internal static class Constants
    {
        public static readonly ILog Logger = LogManager.GetLogger(typeof(Constants));

        public static readonly string AppName = GetAppName();
        public static readonly Version AppVersion = GetAppVersion();

        public static readonly string AppFullName = $@"{AppName} {AppVersion}";

        private static string GetAppName()
            => GetHostAssembly().GetSingleCustomAttribute<AssemblyProductAttribute>(false).Product;

        private static Version GetAppVersion() => GetHostAssembly().GetName().Version;

        private static Assembly GetHostAssembly() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
    }
}