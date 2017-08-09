using System;
using System.Web.Http;
using Omnifactotum.Annotations;
using Sharificabus.HostApplication.Api.Model;
using Sharificabus.HostApplication.DataAccess;

namespace Sharificabus.HostApplication.Api
{
    [RoutePrefix("api/v1")]
    public sealed class SharificabusApiController : ApiController
    {
        private readonly INotificationRepository _notificationRepository;

        public SharificabusApiController([NotNull] INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository
                ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        [HttpGet]
        [Route("")]
        public string GetAppFullName() //// For testing
        {
            return Constants.AppFullName;
        }

        [NotNull]
        [HttpPost]
        [Route("notification")]
        public NotificationInfo PostNotification([NotNull] PostNotificationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _notificationRepository.Add(request.Notification);
        }

        [NotNull]
        [HttpGet]
        [Route("notification/{id}")]
        public Notification GetNotificationById([NotNull] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(@"The value can be neither empty string nor null.", nameof(id));
            }

            var notification = _notificationRepository.Get(id)
                ?? throw new ObjectNotFoundException($@"Notification with ID = ""{id}"" is not found.");

            return notification;
        }
    }
}