using System;
using System.Collections.Generic;

namespace Query
{
    public static class Notifications
    {
        public static void Create(Models.Notification notification)
        {
            Sql.ExecuteNonQuery("Notification_Create", new { 
                notification.userId,
                notification.groupId,
                securitykey = notification.securityKey,
                notification.type,
                notification.notification,
                notification.url
            });
        }

        public static List<Models.Notification> GetList(int userId, DateTime lastChecked, int length = 10)
        {
            return Sql.Populate<Models.Notification>("Notifications_GetList", new { userId, lastChecked, length });
        }

        public static void Read(Guid notifId, int userId)
        {
            Sql.ExecuteNonQuery("Notification_Read", new { notifId, userId });
        }
    }
}
