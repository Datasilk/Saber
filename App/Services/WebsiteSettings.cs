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
            var html = new StringBuilder();
            
            //load website config
            var config = Common.Platform.Website.Settings.Load();

            //load website stylesheets
            var viewStyles = new View("/Views/WebsiteSettings/stylesheets.html");
            var viewStyleItem = new View("/Views/WebsiteSettings/style-item.html");
            html.Clear();
            foreach (var style in config.Stylesheets)
            {
                viewStyleItem["style"] = style;
                html.Append(viewStyleItem.Render());
            }
            viewStyles["styles-list"] = html.ToString();

            //render website stylesheets accordion
            accordion.Clear();
            accordion["title"] = "Stylesheets";
            accordion["contents"] = viewStyles.Render();
            accordions.Append(accordion.Render());

            //load website scripts
            var viewScripts = new View("/Views/WebsiteSettings/scripts.html");
            var viewScriptItem = new View("/Views/WebsiteSettings/script-item.html");
            html.Clear();
            foreach (var style in config.Scripts)
            {
                viewScriptItem["style"] = style;
                html.Append(viewScriptItem.Render());
            }
            viewScripts["styles-list"] = html.ToString();

            //render website stylesheets accordion
            accordion.Clear();
            accordion["title"] = "Scripts";
            accordion["contents"] = viewScripts.Render();
            accordions.Append(accordion.Render());

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

            //load email settings
            var viewEmails = new View("/Views/WebsiteSettings/email-settings.html");
            var clientoptions = new StringBuilder();
            var clientforms = new StringBuilder();
            var emailClients = new List<Vendor.IVendorEmailClient>();
            emailClients.AddRange(Common.Platform.Email.Clients);
            emailClients.AddRange(Common.Vendors.EmailClients.Values.OrderBy(a => a.Name));

            foreach (var client in emailClients)
            {
                clientoptions.Append("<option value=\"" + client.Key + "\">" + client.Name + "</option>");

                //load vendor email client config
                var vendorConfig = client.GetConfig();

                //generate email client form
                clientforms.Append("<div class=\"email-client client-" + client.Key + (client.Key != "smtp" ? " hide" : "") + "\">");
                foreach(var param in client.Parameters)
                {
                    var value = (vendorConfig.ContainsKey(param.Key) ? vendorConfig[param.Key] : param.Value.DefaultValue).Replace("\"", "&quot;");
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
                            clientforms.Append("<div class=\"row input\"><input type=\"checkbox\"" + id + (value == "1" || value.ToLower() == "true" ? " checked=\"checked\"" : "") + " />" + 
                                "<label for=\"" + client.Key + "_" + param.Key + "\">" + param.Value.Name + "</label>" +
                                "</div>");
                            break;
                    }
                    
                }
                clientforms.Append("</div>");
            }
            viewEmails["emailclients"] = clientoptions.ToString();
            viewEmails["emailclients.forms"] = clientforms.ToString();

            //render email actions
            var viewEmailAction = new View("/Views/WebsiteSettings/email-action.html");
            var emailActions = new List<Vendor.EmailType>();
            emailActions.AddRange(Common.Platform.Email.Types);
            emailActions.AddRange(Common.Vendors.EmailTypes.Values);

            foreach(var action in emailActions)
            {
                var configAction = config.Email.Actions.Where(a => a.Type == action.Key).FirstOrDefault();
                viewEmailAction.Bind(new
                {
                    action = new
                    {
                        key = action.Key,
                        name = action.Name,
                        templatefile = action.TemplateFile,
                        subject = configAction?.Subject ?? "",
                        options = string.Join("", 
                            emailClients.Select(a => "<option value=\"" + a.Key + "\"" +
                            (configAction?.Client == a.Key ? " selected=\"selected\"" : "") + 
                            ">" + a.Name + "</option>"
                            ))
                    }
                });
                if (action.UserDefinedSubject) { viewEmailAction.Show("user-subject"); }
                if(action.TemplateFile == "") { viewEmailAction.Show("any-file"); }
                else { viewEmailAction.Show("template-file"); }
                html.Append(viewEmailAction.Render());
                viewEmailAction.Clear();
            }
            viewEmails["email-actions"] = html.ToString();

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

            //render plugins management accordion
            var viewPlugin = new View("/Views/WebsiteSettings/plugin-item.html");
            var viewFeature = new View("/Views/WebsiteSettings/plugin-feature.html");
            var feature = new StringBuilder();
            html.Clear();
            foreach(var plugin in Common.Vendors.Details.Where(a => a.Version != "").OrderBy(a => a.Key))
            {
                viewPlugin["name"] = plugin.Name;
                viewPlugin["key"] = plugin.Key;
                viewPlugin["description"] = plugin.Description;

                if(plugin.ViewRenderers.Count > 0)
                {
                    viewFeature["field"] = "View Renderers";
                    viewFeature["text"] = plugin.ViewRenderers.Count.ToString();
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.ContentFields.Count > 0)
                {
                    viewFeature["field"] = "Content Fields";
                    viewFeature["text"] = plugin.ContentFields.Count.ToString();
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.Controllers.Count > 0)
                {
                    viewFeature["field"] = "Controllers";
                    viewFeature["text"] = string.Join(", ", plugin.Controllers.Select(a => a.Value.Name));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.Services.Count > 0)
                {
                    viewFeature["field"] = "Services";
                    viewFeature["text"] = string.Join(", ", plugin.Services.Select(a => a.Value.Name));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.Startups.Count > 0)
                {
                    viewFeature["field"] = "Startup Config";
                    viewFeature["text"] = "Yes";
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.Keys.Count > 0)
                {
                    viewFeature["field"] = "Security Keys";
                    viewFeature["text"] = string.Join(", ", plugin.Keys.SelectMany(a => a.Keys).Select(a => a.Label));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.HtmlComponents.Count > 0)
                {
                    viewFeature["field"] = "HTML Components";
                    viewFeature["text"] = string.Join(", ", plugin.HtmlComponents.Select(a => a.Value.Name));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.SpecialVars.Count > 0)
                {
                    viewFeature["field"] = "Special Vars";
                    viewFeature["text"] = string.Join(", ", plugin.SpecialVars.Select(a => a.Value.Name));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.EmailClients.Count > 0)
                {
                    viewFeature["field"] = "Email Clients";
                    viewFeature["text"] = string.Join(", ", plugin.EmailClients.Select(a => a.Value.Name));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.EmailTypes.Count > 0)
                {
                    viewFeature["field"] = "Email Types";
                    viewFeature["text"] = string.Join(", ", plugin.EmailTypes.Select(a => a.Value.Name));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.WebsiteSettings.Count > 0)
                {
                    viewFeature["field"] = "Website Settings";
                    viewFeature["text"] = "Yes";
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }
                viewPlugin["features"] = feature.ToString();
                html.Append(viewPlugin.Render());
                feature.Clear();
            }

            //render plugins management accordion
            accordion.Clear();
            accordion["title"] = "Plugins";
            accordion["contents"] = html.ToString();
            accordions.Append(accordion.Render());


            //render accordions
            view["accordions"] = accordions.ToString();

            //add vendor plugins
            html.Clear();
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
                Common.Platform.Email.Clients.Where(a => a.Key == "smtp").FirstOrDefault()?.SaveConfig(parameters);
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

        public string SaveEmailActions(List<Models.Website.EmailAction> actions)
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

        public string UninstallPlugin(string key)
        {
            //create file delete.tmp in Vendor plugin folder
            File.WriteAllText(App.MapPath("/Vendors/" + key + "/uninstall.sbr"), "");
            Server.AppLifetime.StopApplication();
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
