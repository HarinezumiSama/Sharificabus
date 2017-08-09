using System;
using Newtonsoft.Json;
using Omnifactotum.Annotations;

namespace Sharificabus.HostApplication.Api.Model
{
    internal static class ModelExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = CreateSerializerSettings();

        [NotNull]
        public static Notification Copy(this Notification notification)
            => notification?.CopyInternal() ?? throw new ArgumentNullException(nameof(notification));

        [NotNull]
        public static Timestamp Copy(this Timestamp timestamp)
            => timestamp?.CopyInternal() ?? throw new ArgumentNullException(nameof(timestamp));

        [NotNull]
        public static Notification ValidateNew([CanBeNull] this Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (!string.IsNullOrEmpty(notification.Id))
            {
                throw new ArgumentException("A new notification must not have an ID.", nameof(notification));
            }

            if (notification.ServerTimestamp != null)
            {
                throw new ArgumentException(
                    "A new notification must not have a server timestamp.",
                    nameof(notification));
            }

            //// TODO [HarinezumiSama] Implement ValidateNew

            return notification;
        }

        [NotNull]
        public static NotificationInfo ToInfo([NotNull] this Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            return new NotificationInfo
            {
                Id = notification.Id,
                ServerTimestamp = notification.ServerTimestamp.Copy()
            };
        }

        private static JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None
            };

            settings.AssignApplicationWideSettings();

            return settings;
        }

        [NotNull]
        private static T CopyInternal<T>([NotNull] this T value)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var serializedObject = JsonConvert.SerializeObject(value, SerializerSettings);
            return JsonConvert.DeserializeObject<T>(serializedObject, SerializerSettings).EnsureNotNull();
        }
    }
}