using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract(Name = "securityPrincipal")]
    public abstract class SecurityPrincipal
    {
        [DataMember(Name = "id")]
        public string Id
        {
            get;
            set;
        }
    }
}