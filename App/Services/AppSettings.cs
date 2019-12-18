using System.Text.Json;

namespace Saber.Services
{
    public class AppSettings : Service
    {
        public string Render()
        {
            //display all application settings
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/AppSettings/settings.html");
            return JsonSerializer.Serialize(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .app-settings .settings-contents",
                    html = view.Render(),
                    css = Css.ToString(),
                    javascript = Scripts.ToString()
                }
            );
        }
    }
}
