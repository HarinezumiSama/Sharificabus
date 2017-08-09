using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Omnifactotum;

namespace Sharificabus.HostApplication
{
    internal static class LocalHelper
    {
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
    }
}