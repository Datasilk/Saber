using System.Text;

namespace Saber.Services
{
    public class Notifications: Service
    {
        public string RenderList(DateTime? lastChecked = null, int length = 10)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            //get notifications
            var dateChecked = DateTime.Now.AddDays(-30);
            if (lastChecked.HasValue == true)
            {
                dateChecked = lastChecked.Value;
            }
            var notifs = Query.Notifications.GetList(User.UserId, dateChecked, length);
            var view = new View("/Views/Notifications/list-item.html");
            var html = new StringBuilder();
            var unreadCount = 0;
            if (!lastChecked.HasValue)
            {
                //render all dynamic notifications first
                unreadCount = Query.Notifications.GetUnreadCount(User.UserId);
                foreach(var type in Core.Vendors.NotificationTypes)
                {
                    var items = type.GetDynamicList(User);
                    foreach(var item in items)
                    {
                        html.Append(type.Render(view, item.notifId, item.notification, item.url, item.datecreated));
                        view.Clear();
                        unreadCount += 1;
                    }
                }
            }

            //render all generated notifications next
            DateTime lastchecked = DateTime.Now;
            foreach(var notif in notifs)
            {
                //find associated notification type
                var type = Core.Vendors.NotificationTypes.Where(a => a.Type == notif.type).FirstOrDefault();
                if(type != null)
                {
                    //render notification
                    html.Append(type.Render(view, notif.notifId, notif.notification, notif.url, notif.datecreated));
                    view.Clear();
                }
                lastchecked = notif.datecreated;
            }
            return unreadCount + "|!|" + lastchecked.ToString("yyyy/MM/dd hh:mm tt") + "|!|" + html.ToString();
        }
    }
}
