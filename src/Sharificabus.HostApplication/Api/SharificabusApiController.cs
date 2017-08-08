using System.Web.Http;

namespace Sharificabus.HostApplication.Api
{
    public sealed class SharificabusApiController : ApiController
    {
        [HttpGet]
        [Route("")]
        public string Test()
        {
            return Constants.AppFullName;
        }
    }
}