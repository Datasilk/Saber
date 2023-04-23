using Saber.Vendor;

namespace Saber.Common.Notifications
{
    public class NotificationTypes : IVendorNotificationTypes
    {
        //list of notification types specific to Saber
        public NotificationType[] NotificationType { get; set; } = new NotificationType[]
        {
            new Types.GettingStarted()
        };
    }
}
