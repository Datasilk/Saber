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
    }
}
