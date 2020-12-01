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
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            var view = new View("/Views/AppSettings/appsettings.html");
            var accordion = new View("/Views/AppSettings/accordion.html");
            var accordions = new StringBuilder();

            //add icons accordion
            var viewIcons = new View("/Views/AppSettings/icons.html");
            //add website icon
            if (!File.Exists(App.MapPath("/wwwroot/images/web-icon.png")))
            {
                viewIcons.Show("favicon-missing");
            }
            else {

                viewIcons["favicon-src"] = "/images/web-icon.png";
            }

            //add icons
            var viewIcon = new View("/Views/AppSettings/appleicon.html");
            var appleIcons = new StringBuilder();
            var androidIcons = new StringBuilder();
            appleIcons.Append(RenderAppleIcon(viewIcon, 60));
            appleIcons.Append(RenderAppleIcon(viewIcon, 76));
            appleIcons.Append(RenderAppleIcon(viewIcon, 120));
            appleIcons.Append(RenderAppleIcon(viewIcon, 152));
            viewIcons["apple-icons"] = appleIcons.ToString();

            //add android icons
            viewIcon = new View("/Views/AppSettings/androidicon.html");
            androidIcons.Append(RenderAndroidIcon(viewIcon, 36));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 48));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 72));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 96));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 144));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 192));
            androidIcons.Append(RenderAndroidIcon(viewIcon, 512));
            viewIcons["android-icons"] = androidIcons.ToString();

            //render icons accordion
            accordion.Clear();
            accordion["title"] = "Icons";
            accordion["contents"] = viewIcons.Render();
            accordions.Append(accordion.Render());

            //load website config
            Models.Website.Settings config = new Models.Website.Settings();
            var configFile = App.MapPath("website.json");
            if (File.Exists(configFile))
            {
                config = JsonSerializer.Deserialize<Models.Website.Settings>(File.ReadAllText(configFile));
            }

            //load email settings
            var viewEmails = new View("/Views/AppSettings/email-settings.html");
            var emailclients = new StringBuilder("<option value=\"smtp\">SMTP Client</option>");
            foreach(var client in Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\">" + client.Value.Name + "</option>");
            }
            viewEmails["emailclients"] = emailclients.ToString();

            //email action: signup
            emailclients = new StringBuilder("<option value=\"smtp\"" +
                    (config.Email.Actions.SignUp == "smtp" ? " selected=\"selected\"" : "") +
                    ">SMTP Client</option>");
            foreach (var client in Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\"" + 
                    (config.Email.Actions.SignUp == client.Key ? " selected=\"selected\"" : "") + 
                    ">" + client.Value.Name + "</option>");
            }
            viewEmails["emailaction.signup"] = emailclients.ToString();

            //email action: forgot password
            emailclients = new StringBuilder("<option value=\"smtp\"" +
                    (config.Email.Actions.ForgotPass == "smtp" ? " selected=\"selected\"" : "") +
                    ">SMTP Client</option>");
            foreach (var client in Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\"" +
                    (config.Email.Actions.ForgotPass == client.Key ? " selected=\"selected\"" : "") +
                    ">" + client.Value.Name + "</option>");
            }
            viewEmails["emailaction.forgotpass"] = emailclients.ToString();

            //email action: newsletter
            emailclients = new StringBuilder("<option value=\"smtp\"" +
                    (config.Email.Actions.Newsletter == "smtp" ? " selected=\"selected\"" : "") +
                    ">SMTP Client</option>");
            foreach (var client in Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\"" +
                    (config.Email.Actions.Newsletter == client.Key ? " selected=\"selected\"" : "") +
                    ">" + client.Value.Name + "</option>");
            }
            viewEmails["emailaction.newsletter"] = emailclients.ToString();

            //render email settings accordion
            accordion.Clear();
            accordion["title"] = "Email Settings";
            accordion["contents"] = viewEmails.Render();
            accordions.Append(accordion.Render());

            //render accordions
            view["accordions"] = accordions.ToString();

            //add vendor plugins
            var html = new StringBuilder();
            foreach(var vendor in Vendors.WebsiteSettings)
            {
                accordion.Clear();
                accordion["title"] = vendor.Name;
                accordion["contents"] = vendor.Render(this);
                if (accordion["contents"] != "") { html.Append(accordion.Render()); }
            }
            view["vendor.accordions"] = html.ToString();

            //add js file
            AddScript("/editor/js/views/appsettings/appsettings.js");

            //render view
            return JsonResponse(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .app-settings .settings-contents",
                    html = Common.Platform.Render.View(this, view),
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
