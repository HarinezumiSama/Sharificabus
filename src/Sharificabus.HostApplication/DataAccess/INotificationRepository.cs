using Omnifactotum.Annotations;
using Sharificabus.HostApplication.Api.Model;

namespace Sharificabus.HostApplication.DataAccess
{
    public interface INotificationRepository
    {
        [NotNull]
        NotificationInfo Add([NotNull] Notification notification);

        [CanBeNull]
        Notification Get([NotNull] string id);
    }
}