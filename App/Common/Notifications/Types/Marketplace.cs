using Saber.Vendor;

namespace Saber.Common.Notifications.Types
{
    public class Marketplace : NotificationType
    {
        public override string Type { get; set; } = "market";
        public override string Icon { get; set; } = "icon-market";
    }
}
