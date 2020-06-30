using System;
using Omnifactotum.Annotations;

namespace Sharificabus.HostApplication.Properties
{
    internal static class SettingsExtensions
    {
        public static Settings Validate([NotNull] this Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return settings;
        }
    }
}