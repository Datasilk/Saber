using System.Text;
using System.Text.Json;

namespace Saber.Services
{
    public class AppSettings : Service
    {
        public string Render()
        {
            //display all application settings
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/AppSettings/appsettings.html");
            return JsonSerializer.Serialize(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .app-settings .settings-contents",
                    html = RenderView(view),
                    css = Css.ToString(),
                    javascript = Scripts.ToString()
                }
            );
        }

        protected override string RenderView(View view)
        {
            //check for vendor-related View rendering
            var vendors = new StringBuilder("<ul class=\"vendors\">");
            if (Server.viewRenderers.ContainsKey(view.Filename))
            {
                var renderers = Server.viewRenderers[view.Filename];
                foreach (var renderer in renderers)
                {
                    vendors.Append("<li>" + renderer.Render(this, view) + "</li>");
                }
            }
            if (vendors.Length > 0)
            {
                view["vendor"] = vendors.ToString();
            }

            return view.Render();
        }
    }
}
