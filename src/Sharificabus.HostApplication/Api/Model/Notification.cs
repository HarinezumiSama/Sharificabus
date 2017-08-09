using System;
using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract(Name = "notification")]
    public sealed class Notification : NotificationInfo
    {
        [DataMember(Name = "clientTimestamp", IsRequired = true)]
        public Timestamp ClientTimestamp
        {
            get;
            set;
        }

        [DataMember(Name = "subject", IsRequired = true)]
        public string Subject
        {
            get;
            set;
        }

        public override string ToString()
            => $@"{GetType().GetQualifiedName()}: {nameof(Id)} = {Id.ToUIString()}, {
                nameof(Subject)} = {Subject.ToUIString()}";
    }
}