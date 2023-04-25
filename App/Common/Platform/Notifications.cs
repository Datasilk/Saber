namespace Saber.Common.Platform
{
    public static class Notifications
    {
        public static void CreateNotification(string text, string url, string type, int? userId = null, int? groupId = null, string securityKey = "")
        {
            var model = new Query.Models.Notification()
            {
                notification = text,
                url = url,
                type = type,
                userId = userId,
                groupId = groupId,
                securityKey = securityKey
            };
            Query.Notifications.Create(model);
        }

        public static string Render(Vendor.NotificationType type, View defaultView, Guid id, string text, string url, bool read, DateTime? dateCreated = null)
        {
            defaultView["id"] = id.ToString();
            defaultView["url"] = url;
            defaultView["icon"] = type.Icon;
            defaultView["notification"] = text;
            if (!read) { defaultView.Show("unread"); }
            return defaultView.Render();
        }
    }
}
