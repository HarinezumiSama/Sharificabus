using System;
using System.Reflection;

namespace Sharificabus.HostApplication
{
    internal static class Constants
    {
        public static readonly string ApiTitle = "Sharificabus API";

        public static readonly string AppName = GetAppName();
        public static readonly Version AppVersion = GetAppVersion();

        public static readonly string AppFullName = $@"{AppName} {AppVersion}";

        public static readonly StringComparer ObjectIdComparer = StringComparer.Ordinal;

        private static string GetAppName()
            => GetHostAssembly().GetSingleCustomAttribute<AssemblyProductAttribute>(false).Product;

        private static Version GetAppVersion() => GetHostAssembly().GetName().Version;

        private static Assembly GetHostAssembly() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
    }
}