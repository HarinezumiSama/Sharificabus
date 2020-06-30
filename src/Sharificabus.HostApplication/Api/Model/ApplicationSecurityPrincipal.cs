using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api.Model
{
    [DataContract(Name = "applicationSecurityPrincipal")]
    internal sealed class ApplicationSecurityPrincipal : SecurityPrincipal
    {
        [DataMember(Name = "applicationId")]
        public string ApplicationId
        {
            get;
            set;
        }
    }
}