using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Saber.Common.Utility;

namespace Saber.Services
{
    public class AppSettings : Service
    {
        #region "Render"
        public string Render()
        {
            //display all application settings
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            var view = new View("/Views/AppSettings/appsettings.html");

            //add website icon
            if (!File.Exists(App.MapPath("/wwwroot/images/web-icon.png")))
            {
                view.Show("favicon-missing");
            }
            else {

                view["favicon-src"] = "/images/web-icon.png";
            }

            //add icons
            var viewIcon = new View("/Views/AppSettings/appleicon.html");
            var appleIcons = new StringBuilder();
            var androidIcons = new StringBuilder();
            appleIcons.Append(RenderAppleIcon(viewIcon, 60));
            appleIcons.Append(RenderAppleIcon(viewIcon, 76));
            appleIcons.Append(RenderAppleIcon(viewIcon, 120));
            appleIcons.Append(RenderAppleIcon(viewIcon, 152));
            view["apple-icons"] = appleIcons.ToString();

            //add android icons
            viewIcon = new View("/Views/AppSettings/androidicon.html");
            androidIcons.Append(RenderAndroidIcon(viewIcon, 36));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 48));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 72));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 96));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 144));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 192));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 512));
            view["android-icons"] = androidIcons.ToString();


            //add js file
            AddScript("/editor/js/views/appsettings/appsettings.js");

            //render view
            return JsonResponse(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .app-settings .settings-contents",
                    html = Common.Platform.Render.View(this, view, "<ul class=\"vendors list\">", "</ul>", "<li>", "</li>"),
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
            if (!File.Exists(App.MapPath($"/wwwroot/images/mobile/apple-{px}x{px}.png")))
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
            if (!File.Exists(App.MapPath($"/wwwroot/images/mobile/android-{px}x{px}.png")))
            {
                viewIcon.Show("favicon-missing");
            }
            else
            {
                viewIcon["src"] = $"/images/mobile/android-{px}x{px}.png";
            }
            viewIcon["px"] = px.ToString();
            if(px > 240)
            {
                viewIcon["icon-img-attrs"] = " style=\"width:240px;\"";
            }
            return viewIcon.Render();
        }
        #endregion

        public string UploadPngIcon(int type, int px)
        {
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            if (Parameters.Files.Count == 0 || !Parameters.Files.ContainsKey("file"))
            {
                return Error("File upload was not found");
            }
            var file = Parameters.Files["file"];
            if (file.ContentType != "image/png")
            {
                return Error("Icon must be PNG image format.");
            }
            var iconType = "apple";
            var imgpath = "mobile/";
            var imgSuffix = $"-{px}x{px}";
            switch (type)
            {
                case 0: iconType = "web"; imgpath = ""; imgSuffix = "-icon"; break;
                case 2: iconType = "android"; break;
            }

            try
            {
                //check icon dimensions
                var image = Image.Load(file);
                if (px != 0 && (image.width != px || image.height != px))
                {
                    return Error($"Icon must be {px} pixels in width & height.");
                }else if(px == 0 && (image.width > 128))
                {
                    return Error($"Icon must be less than 129 pixels in width & height.");
                }
                //save image to disk
                if (!Directory.Exists(App.MapPath("/wwwroot/images/" + imgpath)))
                {
                    Directory.CreateDirectory(App.MapPath("/wwwroot/images/" + imgpath));
                }
                var filepath = App.MapPath($"/wwwroot/images/{imgpath}{iconType}{imgSuffix}.png");
                File.Delete(filepath);
                file.Seek(0, SeekOrigin.Begin);
                using (var fs = new FileStream(filepath, FileMode.Create))
                {
                    file.CopyTo(fs);
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
