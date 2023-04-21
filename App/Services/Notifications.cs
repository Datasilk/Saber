namespace Saber.Services
{
    public class Notifications: Service
    {
        public string Get(DateTime? lastChecked = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            //get notifications
            return "";
        }
    }
}
