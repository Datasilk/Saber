namespace Saber.Services
{
    public class Marketplace : Service
    {
        public string Toolbar()
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            return Cache.LoadFile("Views/Market/toolbar.html");
        }
    }
}
