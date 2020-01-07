using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Saber.Common.Utility;

namespace Saber.Services
{
    public class AppSettings : Service
    {
        #region "Render"
        public string Render()
        {
            //display all application settings
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/AppSettings/appsettings.html");

            //add website icon
            if (!File.Exists(Server.MapPath("/wwwroot/images/favicon.ico")))
            {
                view.Show("favicon-missing");
            }
            else {

                view["favicon-src"] = "/images/favicon.ico";
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
            viewIcon["favicon-missing"] = "";
            viewIcon["src"] = "";
            viewIcon["px"] = px.ToString();
            if (!File.Exists(Server.MapPath($"/wwwroot/images/mobile/apple-{px}x{px}.png")))
            {
                viewIcon.Show("favicon-missing");
            }
            else
            {
                viewIcon["src"] = $"/images/mobile/apple-{px}x{px}.png";
            }
            return viewIcon.Render();
        }

        private string RenderAndroidIcon(View viewIcon, int px)
        {
            viewIcon["favicon-missing"] = "";
            viewIcon["src"] = "";
            viewIcon["px"] = px.ToString();
            if (!File.Exists(Server.MapPath($"/wwwroot/images/mobile/android-{px}x{px}.png")))
            {
                viewIcon.Show("favicon-missing");
            }
            else
            {
                viewIcon["src"] = $"/images/mobile/android-{px}x{px}.png";
            }
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

        #endregion

        public string UploadPngIcon(int type, int px)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            if (Context.Request.Form.Files.Count == 0)
            {
                return Error("File upload was not found");
            }
            if (Context.Request.Form.Files[0].ContentType != "image/png")
            {
                return Error("Icon must be PNG image format.");
            }
            var iconType = "apple";
            switch (type)
            {
                case 2: iconType = "android"; break;
            }

            try
            {
                //check icon dimensions
                using (var file = Context.Request.Form.Files[0].OpenReadStream())
                {
                    var image = Image.Load(file);
                    if (image.width != px || image.height != px)
                    {
                        return Error($"Icon must be {px} pixels in width & height.");
                    }
                }
                //save image to disk
                if (!Directory.Exists(Server.MapPath("/wwwroot/images/mobile/")))
                {
                    Directory.CreateDirectory(Server.MapPath("/wwwroot/images/mobile/"));
                }
                using (var file = Context.Request.Form.Files[0].OpenReadStream())
                {
                    var filepath = Server.MapPath($"/wwwroot/images/mobile/{iconType}-{px}x{px}.png");
                    File.Delete(filepath);
                    using (var fs = new FileStream(filepath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                }
            }
            catch (Exception)
            {
                return Error("Error reading image file");
            }
            return Success();
        }
    }
}
