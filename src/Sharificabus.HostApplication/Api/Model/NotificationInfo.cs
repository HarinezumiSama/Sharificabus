using System;
using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract(Name = "notificationInfo")]
    public class NotificationInfo
    {
        [DataMember(Name = "id")]
        public string Id
        {
            get;
            set;
        }

        [DataMember(Name = "serverTimestamp")]
        public Timestamp ServerTimestamp
        {
            get;
            set;
        }

        public override string ToString()
            => $@"{GetType().GetQualifiedName()}: {nameof(Id)} = {Id.ToUIString()}";
    }
}