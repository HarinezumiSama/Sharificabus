using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Omnifactotum;

namespace Sharificabus.HostApplication
{
    internal static class LocalHelper
    {
        public const string ForwardSlash = "/";

        private const string Localhost = "localhost";

        public static void AssignApplicationWideSettings(this JsonSerializerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.Converters.Add(new StringEnumConverter());
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTimeOffset;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
        }

        public static string GenerateObjectId()
            => Factotum.GenerateIdString(2 * Factotum.MinimumGeneratedIdPartSize, IdGenerationModes.UniqueAndRandom);

        public static string AppendSlash(this string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return url.EndsWith(ForwardSlash, StringComparison.Ordinal) ? url : url + ForwardSlash;
        }

        public static string GetHostName()
        {
            var getters = new Func<string>[]
            {
                () => Dns.GetHostEntry(IPAddress.Loopback).HostName,
                () => Environment.MachineName
            };

            try
            {
                foreach (var getter in getters)
                {
                    var hostname = getter();
                    if (StringComparer.OrdinalIgnoreCase.Equals(hostname, Localhost))
                    {
                        continue;
                    }

                    return hostname;
                }
            }
            catch (Exception ex)
                when (!ex.IsFatal())
            {
                return Localhost;
            }

            return Localhost;
        }

    }
}