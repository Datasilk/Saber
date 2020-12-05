using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Saber.Common.Utility;

namespace Saber.Services
{
    public class WebsiteSettings : Service
    {
        #region "Render"
        public string Render()
        {
            //display all application settings
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            var view = new View("/Views/WebsiteSettings/websitesettings.html");
            var accordion = new View("/Views/WebsiteSettings/accordion.html");
            var accordions = new StringBuilder();

            //add icons accordion
            var viewIcons = new View("/Views/WebsiteSettings/icons.html");
            //add website icon
            if (!File.Exists(App.MapPath("/wwwroot/images/web-icon.png")))
            {
                viewIcons.Show("favicon-missing");
            }
            else {

                viewIcons["favicon-src"] = "/images/web-icon.png";
            }

            //add icons
            var viewIcon = new View("/Views/WebsiteSettings/appleicon.html");
            var appleIcons = new StringBuilder();
            var androidIcons = new StringBuilder();
            appleIcons.Append(RenderAppleIcon(viewIcon, 60));
            appleIcons.Append(RenderAppleIcon(viewIcon, 76));
            appleIcons.Append(RenderAppleIcon(viewIcon, 120));
            appleIcons.Append(RenderAppleIcon(viewIcon, 152));
            viewIcons["apple-icons"] = appleIcons.ToString();

            //add android icons
            viewIcon = new View("/Views/WebsiteSettings/androidicon.html");
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
            var config = Common.Platform.Website.Settings.Load();

            //load email settings
            var viewEmails = new View("/Views/WebsiteSettings/email-settings.html");
            var emailclients = new StringBuilder("<option value=\"smtp\">SMTP Client</option>");
            var clientforms = new StringBuilder();
            viewEmails["smtp.domain"] = config.Email.Smtp.Domain;
            viewEmails["smtp.port"] = config.Email.Smtp.Port.ToString();
            if (config.Email.Smtp.SSL == true) { viewEmails.Show("smtp.ssl"); }
            viewEmails["smtp.user"] = config.Email.Smtp.Username;
            viewEmails["smtp.pass"] = config.Email.Smtp.Password != "" ? "********" : "";

            foreach (var client in Common.Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\">" + client.Value.Name + "</option>");

                //load vendor email client config
                var vendorConfig = client.Value.GetConfig();

                //generate email client form
                clientforms.Append("<div class=\"email-client client-" + client.Key + " hide\">");
                foreach(var param in client.Value.Parameters)
                {
                    var value = (vendorConfig.ContainsKey(param.Key) ? vendorConfig[param.Key] : "").Replace("\"", "&quot;");
                    var id = " id=\"" + client.Key + "_" + param.Key + "\"";
                    switch (param.Value.DataType)
                    {
                        case Vendor.EmailClientDataType.Boolean:
                            break;
                        default:
                            clientforms.Append("<div class=\"row field\">" + param.Value.Name + "</div>");
                            break;
                    }
                    switch (param.Value.DataType)
                    {
                        case Vendor.EmailClientDataType.Text:
                            clientforms.Append("<div class=\"row input\"><input type=\"text\"" + id + " value=\"" + value + "\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.UserOrEmail:
                            clientforms.Append("<div class=\"row input\"><input type=\"text\"" + id + " value=\"" + value + "\" autocomplete=\"new-email\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.Password:
                            clientforms.Append("<div class=\"row input\"><input type=\"password\"" + id + " value=\"" + (value != "" ? "********" : "") + "\" autocomplete=\"new-password\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.Number:
                            clientforms.Append("<div class=\"row input\"><input type=\"number\"" + id + " value=\"" + value + "\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.List:
                            clientforms.Append("<div class=\"row input\"><select" + id + ">" + 
                                string.Join("", param.Value.ListOptions?.Select(a => "<option value=\"" + a + "\">" + a + "</option>") ?? new string[] { "" }) +
                                "</select></div>");
                            break;
                        case Vendor.EmailClientDataType.Boolean:
                            clientforms.Append("<div class=\"row input\"><input type=\"checkbox\"" + id + " />" + 
                                "<label for=\"" + client.Key + "_" + param.Key + "\">" + param.Value.Name + "</label>" +
                                "</div>");
                            break;
                    }
                    
                }
                clientforms.Append("</div>");
            }
            viewEmails["emailclients"] = emailclients.ToString();
            viewEmails["emailclients.forms"] = clientforms.ToString();

            //email action: signup
            emailclients = new StringBuilder("<option value=\"smtp\"" +
                    (config.Email.Actions.SignUp.Client == "smtp" ? " selected=\"selected\"" : "") +
                    ">SMTP Client</option>");
            foreach (var client in Common.Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\"" + 
                    (config.Email.Actions.SignUp.Client == client.Key ? " selected=\"selected\"" : "") + 
                    ">" + client.Value.Name + "</option>");
            }
            viewEmails["emailaction.signup"] = emailclients.ToString();
            viewEmails["emailaction.signup.subject"] = config.Email.Actions.SignUp.Subject;

            //email action: forgot password
            emailclients = new StringBuilder("<option value=\"smtp\"" +
                    (config.Email.Actions.ForgotPass.Client == "smtp" ? " selected=\"selected\"" : "") +
                    ">SMTP Client</option>");
            foreach (var client in Common.Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\"" +
                    (config.Email.Actions.ForgotPass.Client == client.Key ? " selected=\"selected\"" : "") +
                    ">" + client.Value.Name + "</option>");
            }
            viewEmails["emailaction.forgotpass"] = emailclients.ToString();
            viewEmails["emailaction.forgotpass.subject"] = config.Email.Actions.ForgotPass.Subject;

            //email action: newsletter
            emailclients = new StringBuilder("<option value=\"smtp\"" +
                    (config.Email.Actions.Newsletter.Client == "smtp" ? " selected=\"selected\"" : "") +
                    ">SMTP Client</option>");
            foreach (var client in Common.Vendors.EmailClients)
            {
                emailclients.Append("<option value=\"" + client.Key + "\"" +
                    (config.Email.Actions.Newsletter.Client == client.Key ? " selected=\"selected\"" : "") +
                    ">" + client.Value.Name + "</option>");
            }
            viewEmails["emailaction.newsletter"] = emailclients.ToString();

            //render email settings accordion
            accordion.Clear();
            accordion["title"] = "Email Settings";
            accordion["contents"] = viewEmails.Render();
            accordions.Append(accordion.Render());

            //load passwords settings
            var viewPasswords = new View("/Views/WebsiteSettings/passwords.html");
            viewPasswords.Bind(new { pass = config.Passwords });
            if (config.Passwords.NoSpaces)
            {
                viewPasswords.Show("nospaces");
            }

            //render passwords accordion
            accordion.Clear();
            accordion["title"] = "Password Settings";
            accordion["contents"] = viewPasswords.Render();
            accordions.Append(accordion.Render());

            //render accordions
            view["accordions"] = accordions.ToString();

            //add vendor plugins
            var html = new StringBuilder();
            foreach(var vendor in Common.Vendors.WebsiteSettings)
            {
                accordion.Clear();
                accordion["title"] = vendor.Name;
                accordion["contents"] = vendor.Render(this);
                if (accordion["contents"] != "") { html.Append(accordion.Render()); }
            }
            view["vendor.accordions"] = html.ToString();

            //add js file
            AddScript("/editor/js/views/websitesettings/websitesettings.js");

            //render view
            return JsonResponse(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .web-settings .settings-contents",
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

        #region "Save Settings"
        public string SaveEmailClient(string id, Dictionary<string, string> parameters)
        {
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            if (id == "smtp")
            {
                //save default email client settings
                var settings = Common.Platform.Website.Settings.Load();
                int.TryParse(parameters["port"], out var port);
                var ssl = parameters["ssl"] ?? "";
                var pass = parameters["pass"] ?? "";
                settings.Email.Smtp.Domain = parameters["domain"] ?? "";
                settings.Email.Smtp.Port = port;
                settings.Email.Smtp.SSL = ssl.ToLower() == "true";
                settings.Email.Smtp.Username = parameters["user"];
                if(pass != "" && pass.Any(a => a != '*'))
                {
                    settings.Email.Smtp.Password = parameters["pass"];
                }
                Common.Platform.Website.Settings.Save(settings);
            }
            else
            {
                //save vendor email client settings
                var vendor = Common.Vendors.EmailClients.Where(a => a.Key == id).FirstOrDefault().Value;
                if(vendor != null)
                {
                    vendor.SaveConfig(parameters);
                }
            }
            return Success();
        }

        public string SaveEmailActions(Models.Website.EmailActions actions)
        {
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            var settings = Common.Platform.Website.Settings.Load();
            settings.Email.Actions = actions;
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }

        public string SavePasswords(Models.Website.Passwords passwords)
        {
            if(passwords == null) { return Error("No data recieved"); }
            if (!CheckSecurity("website-settings")) { return AccessDenied(); }
            var settings = Common.Platform.Website.Settings.Load();
            settings.Passwords = passwords;
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }
        #endregion

        #region "Upload"
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
        #endregion
    }
}
