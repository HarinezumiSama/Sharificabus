using System;
using System.Runtime.Serialization;
using Omnifactotum.Annotations;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract]
    public sealed class Timestamp
    {
        [DataMember(Name = "dateTimeOffset", IsRequired = true)]
        public DateTimeOffset DateTimeOffset
        {
            get;
            set;
        }

        //// TODO [HarinezumiSama] ISO-formatted strings for date, time, date/time; UNIX-time?

        [NotNull]
        public static Timestamp GetCurrent()
        {
            //// TODO [HarinezumiSama] UtcNow for server?
            return new Timestamp { DateTimeOffset = DateTimeOffset.Now };
        }
    }
}