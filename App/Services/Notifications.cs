using System.Text;

namespace Saber.Services
{
    public class Notifications: Service
    {
        public string RenderList(DateTime? lastChecked = null, int length = 10)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            //get notifications
            if(lastChecked.HasValue == false)
            {
                lastChecked = new DateTime().AddDays(-30);
            }
            var notifs = Query.Notifications.GetList(User.UserId, lastChecked.Value, length);
            var view = new View("/Views/Notifications/list-item.html");
            var html = new StringBuilder();

            if (!lastChecked.HasValue)
            {
                //render all dynamic notifications first

            }

            //render all generated notifications next
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
            }
            return html.ToString();
        }
    }
}
