using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract(Name = "userSecurityPrincipal")]
    internal sealed class UserSecurityPrincipal : SecurityPrincipal
    {
        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }
    }
}