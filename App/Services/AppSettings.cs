using System.IO;
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

            //add website icon
            if (File.Exists(Server.MapPath("/wwwroot/images/favicon.ico")))
            {
                view["favicon-src"] = "/images/favicon.ico";
                view.Show("favicon-exists");
            }

            //add icons
            var viewIcon = new View("/Views/AppSettings/appleicon.html");
            var icons = new StringBuilder();
            icons.Append(RenderAppleIcon(viewIcon, 60));
            icons.Append(RenderAppleIcon(viewIcon, 76));
            icons.Append(RenderAppleIcon(viewIcon, 120));
            icons.Append(RenderAppleIcon(viewIcon, 152));

            //add android icons
            viewIcon = new View("/Views/AppSettings/androidicon.html");
            icons.Append(RenderAndroidIcon(viewIcon, 36));
            icons.Append(RenderAndroidIcon(viewIcon, 48));
            icons.Append(RenderAndroidIcon(viewIcon, 72));
            icons.Append(RenderAndroidIcon(viewIcon, 96));
            icons.Append(RenderAndroidIcon(viewIcon, 144));
            icons.Append(RenderAndroidIcon(viewIcon, 192));
            icons.Append(RenderAndroidIcon(viewIcon, 512));
            view["icons"] = icons.ToString();


            //add js file
            AddScript("/editor/js/views/appsettings/appsettings.js");

            //render view
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

        private string RenderAppleIcon(View viewIcon, int px)
        {
            if (File.Exists(Server.MapPath("/wwwroot/images/mobile/apple-" + px + "x" + px+ ".png")))
            {
                viewIcon.Show("favicon-exists");
            }
            viewIcon["src"] = "/images/mobile/apple-" + px + "x" + px + ".png";
            viewIcon["px"] = px.ToString();
            return viewIcon.Render();
        }

        private string RenderAndroidIcon(View viewIcon, int px)
        {
            if (File.Exists(Server.MapPath("/wwwroot/images/mobile/android-" + px + "x" + px + ".png")))
            {
                viewIcon.Show("favicon-exists");
            }
            viewIcon["src"] = "/images/mobile/android-" + px + "x" + px + ".png";
            viewIcon["px"] = px.ToString();
            return viewIcon.Render();
        }

        protected override string RenderView(View view)
        {
            //check for vendor-related View rendering
            var vendors = new StringBuilder("<ul class=\"vendors list\">");
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

        public string UploadAppleIcon(int px)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            
            return Success();
        }
    }
}
