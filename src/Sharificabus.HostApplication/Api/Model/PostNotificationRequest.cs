using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract]
    public sealed class PostNotificationRequest
    {
        //// TODO [HarinezumiSama] Add properties for associated app/user

        [DataMember]
        public Notification Notification
        {
            get;
            set;
        }
    }
}