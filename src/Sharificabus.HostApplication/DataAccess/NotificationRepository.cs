using System;
using System.Collections.Generic;
using Sharificabus.HostApplication.Api.Model;

namespace Sharificabus.HostApplication.DataAccess
{
    internal sealed class NotificationRepository : INotificationRepository
    {
        private readonly object _syncLock;
        private readonly Dictionary<string, Notification> _notifications;

        public NotificationRepository()
        {
            _syncLock = new object();
            _notifications = new Dictionary<string, Notification>(Constants.ObjectIdComparer);
        }

        public NotificationInfo Add(Notification notification)
        {
            var copy = notification.ValidateNew().Copy();
            copy.Id = LocalHelper.GenerateObjectId();
            copy.ServerTimestamp = Timestamp.GetCurrent();

            lock (_syncLock)
            {
                _notifications.Add(copy.Id, copy);
            }

            var result = copy.ToInfo();
            return result;
        }

        public Notification Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(@"The ID can be neither empty string nor null.", nameof(id));
            }

            lock (_syncLock)
            {
                return _notifications.GetValueOrDefault(id);
            }
        }
    }
}