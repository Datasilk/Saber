﻿using System.Text;
using Saber.Core.Extensions.Strings;
using Saber.Core;
using Datasilk.Core.DOM;
using System.Net.Mail;

namespace Saber.Services
{
    public class WebsiteSettings : Service
    {
        #region "Render"
        public string Render()
        {
            //display all application settings
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var view = new View("/Views/WebsiteSettings/websitesettings.html");
            var accordion = new View("/Views/WebsiteSettings/accordion.html");
            var accordions = new StringBuilder();
            var html = new StringBuilder();
            
            //load website config
            var config = Common.Platform.Website.Settings.Load();

            //load website stylesheets
            var viewStyles = new View("/Views/WebsiteSettings/stylesheets.html");
            viewStyles["styles-list"] = RenderStylesheetsList(config);

            //render website stylesheets accordion
            accordion.Clear();
            accordion["id"] = "stylesheets";
            accordion["title"] = "Stylesheets";
            accordion["contents"] = viewStyles.Render();
            accordions.Append(accordion.Render());

            //load website scripts
            var viewScripts = new View("/Views/WebsiteSettings/scripts.html");
            viewScripts["scripts-list"] = RenderScriptsList(config);

            //render website stylesheets accordion
            accordion.Clear();
            accordion["id"] = "scripts";
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
            accordion["id"] = "icons";
            accordion["title"] = "Icons";
            accordion["contents"] = viewIcons.Render();
            accordions.Append(accordion.Render());

            //load page titles
            html = new StringBuilder();
            var viewPageTitle = new View("/Views/WebsiteSettings/page-title.html");
            foreach(var title in config.PageTitles)
            {
                viewPageTitle.Clear();
                viewPageTitle["title"] = title.Value;
                viewPageTitle["type"] = title.Type.ToString();
                viewPageTitle["typeid"] = ((int)title.Type).ToString();
                html.Append(viewPageTitle.Render());
            }

            //render page title accordion
            var viewPageTitles = new View("/Views/WebsiteSettings/page-titles.html");
            viewPageTitles["list"] = html.ToString();
            accordion.Clear();
            accordion["id"] = "page-titles";
            accordion["title"] = "Page Titles";
            accordion["contents"] = viewPageTitles.Render();
            accordions.Append(accordion.Render());

            //load languages
            html = new StringBuilder();
            if(config.Languages.Count > 0)
            {
                var viewLanguage = new View("/Views/WebsiteSettings/language.html");
                foreach (var lang in config.Languages)
                {
                    viewLanguage.Clear();
                    viewLanguage["title"] = lang.Name;
                    viewLanguage["abbr"] = lang.Id;
                    html.Append(viewLanguage.Render());
                }
                var viewLanguages = new View("/Views/WebsiteSettings/languages.html");
                viewLanguages["list"] = html.ToString();
                html = new StringBuilder(viewLanguages.Render());
            }
            else
            {
                var viewLanguage = new View("/Views/WebsiteSettings/no-languages.html");
                html = new StringBuilder(viewLanguage.Render());
            }

            //render page title accordion
            accordion.Clear();
            accordion["id"] = "languages";
            accordion["title"] = "Languages";
            accordion["contents"] = html.ToString();
            accordions.Append(accordion.Render());

            //load email settings
            var viewEmails = new View("/Views/WebsiteSettings/email/settings.html");
            viewEmails["email-clients"] = RenderEmailClients();

            //render email actions
            viewEmails["email-actions"] = RenderEmailActions();

            //render email settings accordion
            accordion.Clear();
            accordion["id"] = "email-settings";
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
            accordion["id"] = "password-settings";
            accordion["title"] = "Password Settings";
            accordion["contents"] = viewPasswords.Render();
            accordions.Append(accordion.Render());

            //render developer key accordion
            var viewDevkeys = new View("/Views/WebsiteSettings/dev-keys.html");
            var viewDevkey = new View("/Views/WebsiteSettings/dev-key.html");
            html.Clear();
            foreach (var key in Server.DeveloperKeys)
            {
                viewDevkey.Clear();
                viewDevkey["client_id"] = key.Client_ID;
                viewDevkey["dev-key"] = key.Key;
                viewDevkey["redirect_uri"] = key.Redirect_URI;
                html.Append(viewDevkey.Render());
            }
            viewDevkeys["dev-keys"] = html.ToString();
            accordion.Clear();
            accordion["id"] = "dev-keys";
            accordion["title"] = "Developer Keys";
            accordion["contents"] = viewDevkeys.Render();
            accordions.Append(accordion.Render());

            //render public api accordion
            var viewPublicApis = new View("/Views/WebsiteSettings/publicapis.html");
            var viewPublicApi = new View("/Views/WebsiteSettings/publicapi.html");
            var viewPublicApiParam = new View("/Views/WebsiteSettings/publicapi-param.html");
            html.Clear();
            foreach (var publicapi in Common.Platform.PublicApi.GetList())
            {
                viewPublicApi.Clear();
                viewPublicApi["path"] = App.ServicePaths[0] + "/" + publicapi.Path;
                viewPublicApi["description"] = publicapi.Description;
                viewPublicApi["id"] = publicapi.Path.Replace("/", "_");
                viewPublicApi["api"] = publicapi.Path;
                if(publicapi.Enabled == true)
                {
                    viewPublicApi.Show("enabled");
                }
                if (publicapi.Parameters.Count > 0)
                {
                    foreach(var param in publicapi.Parameters)
                    {
                        viewPublicApiParam.Clear();
                        viewPublicApiParam["name"] = param.Name;
                        viewPublicApiParam["data-type"] = param.DataType;
                        viewPublicApiParam["description"] = param.Description;
                        viewPublicApi["parameters"] += viewPublicApiParam.Render();
                    }
                    html.Append(viewPublicApi.Render());
                }
            }
            if (Common.Platform.PublicApi.Enabled == true)
            {
                viewPublicApis.Show("enabled");
            }
            viewPublicApis["api"] = Common.Platform.PublicApi.Name;
            viewPublicApis["public-apis"] = html.ToString();
            
            accordion.Clear();
            accordion["id"] = "public-apis";
            accordion["title"] = "Public APIs";
            accordion["contents"] = viewPublicApis.Render();
            accordions.Append(accordion.Render());

            //load restart app feature
            var viewRestart = new View("/Views/WebsiteSettings/restart.html");

            //render restart accordion
            accordion.Clear();
            accordion["id"] = "restart";
            accordion["title"] = "Restart Website";
            accordion["contents"] = viewRestart.Render();
            accordions.Append(accordion.Render());

            //render plugins management accordion
            var viewPlugin = new View("/Views/WebsiteSettings/plugin-item.html");
            var viewFeature = new View("/Views/WebsiteSettings/plugin-feature.html");
            var feature = new StringBuilder();
            var paramOpen = "<span class=\"color param\">";
            var paramClose = "</span>";
            html.Clear();
            foreach(var plugin in Core.Vendors.Details.Where(a => a.Version != "").OrderBy(a => a.Key))
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
                    viewFeature["text"] = string.Join(", ", plugin.Controllers.Select(a => paramOpen + a.Value.Name + paramClose));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.Services.Count > 0)
                {
                    viewFeature["field"] = "Services";
                    viewFeature["text"] = string.Join(", ", plugin.Services.Select(a => paramOpen + a.Value.Name + paramClose));
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
                    viewFeature["text"] = string.Join(", ", plugin.Keys.SelectMany(a => a.Keys).Select(a => paramOpen + a.Label + paramClose));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.HtmlComponents.Count > 0)
                {
                    viewFeature["field"] = "HTML Components";
                    viewFeature["text"] = string.Join(", ", plugin.HtmlComponents.Select(a => paramOpen + a.Value.Name + paramClose));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.SpecialVars.Count > 0)
                {
                    viewFeature["field"] = "Special Vars";
                    viewFeature["text"] = string.Join(", ", plugin.SpecialVars.Select(a => paramOpen + a.Value.Name + paramClose));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.EmailClients.Count > 0)
                {
                    viewFeature["field"] = "Email Clients";
                    viewFeature["text"] = string.Join(", ", plugin.EmailClients.Select(a => paramOpen + a.Value.Name + paramClose));
                    feature.Append(viewFeature.Render());
                    viewFeature.Clear();
                }

                if (plugin.EmailActions.Count > 0)
                {
                    viewFeature["field"] = "Email Actions";
                    viewFeature["text"] = string.Join(", ", plugin.EmailActions.Select(a => paramOpen + a.Value.Name + paramClose));
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
            accordion["id"] = "plugins";
            accordion["title"] = "Plugins";
            accordion["contents"] = html.ToString();
            accordions.Append(accordion.Render());


            //render accordions
            view["accordions"] = accordions.ToString();

            //add vendor plugins
            html.Clear();
            foreach(var vendor in Core.Vendors.WebsiteSettings)
            {
                accordion.Clear();
                accordion["id"] = "plugin-" + vendor.Name.ReplaceOnlyAlphaNumeric(true, true, true).Replace(" ", "-").ToLower();
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
                    selector = ".web-settings .settings-contents",
                    html = Common.Platform.Render.View(this, view),
                    css = Css.ToString(),
                    javascript = Scripts.ToString()
                }
            );
        }
        #endregion

        #region "Website Stylesheets"

        public string RenderStylesheetsList(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var config = Common.Platform.Website.Settings.Load();
            return RenderStylesheetsList(config);
        }

        private string RenderStylesheetsList(Models.Website.Settings config)
        {
            var styleItem = new View("/Views/WebsiteSettings/style-item.html");
            var styles = new StringBuilder();
            if (config.Stylesheets.Count > 0)
            {
                foreach (var style in config.Stylesheets)
                {
                    styleItem["style"] = style;
                    styles.Append(styleItem.Render());
                }
            }
            return styles.ToString();
        }

        public string GetAvailableStylesheets()
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                return JsonResponse(RenderAvailableStylesheetsList());
            }
            catch (Exception)
            {
                return Error();
            }
        }

        private List<string> RenderAvailableStylesheetsList()
        {
            var list = new List<string>();
            RecurseDirectoriesForStylesheets(list, App.MapPath("/wwwroot/editor/vendors"));
            RecurseDirectoriesForStylesheets(list, App.MapPath("/wwwroot/css"));
            RecurseDirectoriesForStylesheets(list, App.MapPath("/wwwroot/content"));
            var root = App.MapPath("/") + "\\";
            var rel = new List<string>();
            foreach (var i in list)
            {
                rel.Add("/" + i.Replace(root, "").Replace("\\", "/").Replace("wwwroot/", ""));
            }
            return rel;
        }

        private void RecurseDirectoriesForStylesheets(List<string> list, string path)
        {
            var dir = new DirectoryInfo(path);
            var filetypes = new string[] { "css" };
            var excluded = new string[] { "website.css", "/pages/" };
            list.AddRange(dir.GetFiles().Select(a => a.FullName)
                .Where(a => filetypes.Any(b => a.Replace("\\", "/").Split("/")[^1].Split(".")[^1].ToLower() == b) &&
                !excluded.Any(b => a.Replace("\\", "/").Contains(b))));

            foreach (var d in dir.GetDirectories())
            {
                RecurseDirectoriesForStylesheets(list, d.FullName);
            }
        }

        public string AddStylesheetToSite(string file)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                if (file == "") { return Error(); }
                var config = Common.Platform.Website.Settings.Load();
                if (!config.Stylesheets.Contains(file))
                {
                    config.Stylesheets.Add(file);
                }
                Common.Platform.Website.Settings.Save(config);
                return RenderStylesheetsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveStylesheet(string file)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                var config = Common.Platform.Website.Settings.Load();
                if (config.Stylesheets.Contains(file))
                {
                    config.Stylesheets.Remove(file);
                }
                Common.Platform.Website.Settings.Save(config);
                return RenderStylesheetsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string SortStylesheets(List<string> stylesheets)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                var config = Common.Platform.Website.Settings.Load();
                config.Stylesheets = stylesheets;
                Common.Platform.Website.Settings.Save(config);
                return Success();
            }
            catch (Exception)
            {
                return Error("Error sorting stylesheets");
            }
        }

        #endregion

        #region "Website Scripts"

        public string RenderScriptsList(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var config = Common.Platform.Website.Settings.Load();
            return RenderScriptsList(config);
        }

        private string RenderScriptsList(Models.Website.Settings config)
        {
            var scriptItem = new View("/Views/WebsiteSettings/script-item.html");
            var scripts = new StringBuilder();
            if (config.Scripts.Count > 0)
            {
                foreach (var script in config.Scripts)
                {
                    scriptItem["script"] = script;
                    scripts.Append(scriptItem.Render());
                }
            }
            return scripts.ToString();
        }

        public string GetAvailableScripts()
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                return JsonResponse(RenderAvailableScriptsList());
            }
            catch (Exception)
            {
                return Error();
            }
        }

        private List<string> RenderAvailableScriptsList()
        {
            var list = new List<string>(){
                "editor/js/platform.js",
                "editor/js/selector.js",
                "editor/js/utility/velocity.min.js"
            };
            RecurseDirectoriesForScripts(list, App.MapPath("/wwwroot/editor/vendors"));
            RecurseDirectoriesForScripts(list, App.MapPath("/wwwroot/js"));
            RecurseDirectoriesForScripts(list, App.MapPath("/wwwroot/content"));
            var root = App.MapPath("/") + "\\";
            var rel = new List<string>();
            foreach (var i in list)
            {
                rel.Add("/" + i.Replace(root, "").Replace("\\", "/").Replace("wwwroot/", ""));
            }
            return rel;
        }

        private void RecurseDirectoriesForScripts(List<string> list, string path)
        {
            var dir = new DirectoryInfo(path);
            var filetypes = new string[] { "js" };
            var excluded = new string[] { "website.js", "/pages/" };
            list.AddRange(dir.GetFiles().Select(a => a.FullName)
                .Where(a => filetypes.Any(b => a.Replace("\\", "/").Split("/")[^1].Split(".")[^1].ToLower() == b) &&
                !excluded.Any(b => a.Replace("\\", "/").Contains(b))));

            foreach (var d in dir.GetDirectories())
            {
                RecurseDirectoriesForScripts(list, d.FullName);
            }
        }

        public string AddScriptToSite(string file)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                if (file == "") { return Error(); }
                var config = Common.Platform.Website.Settings.Load();
                if (!config.Scripts.Contains(file))
                {
                    config.Scripts.Add(file);
                }
                Common.Platform.Website.Settings.Save(config);
                return RenderScriptsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveScript(string file, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                var config = Common.Platform.Website.Settings.Load();
                if (config.Scripts.Contains(file))
                {
                    config.Scripts.Remove(file);
                }
                Common.Platform.Website.Settings.Save(config);
                return RenderScriptsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string SortScripts(List<string> scripts)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            try
            {
                var config = Common.Platform.Website.Settings.Load();
                config.Scripts = scripts;
                Common.Platform.Website.Settings.Save(config);
                return Success();
            }
            catch (Exception)
            {
                return Error("Error sorting scripts");
            }
        }

        #endregion

        #region "Icons"
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

        public string UploadPngIcon(int type, int px)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
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
                var image = Common.Utility.Image.Load(file);
                if (px != 0 && (image.width != px || image.height != px))
                {
                    return Error($"Icon must be {px} pixels in width & height.");
                }
                else if (px == 0 && (image.width > 128))
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

        #region "Email Settings"
        public string RenderEmailClients()
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var config = Common.Platform.Website.Settings.Load();
            var viewClientList = new View("/Views/WebsiteSettings/email/client-list.html");
            var viewClient = new View("/Views/WebsiteSettings/email/client.html");
            if (config.Email.Clients.Count > 0)
            {
                var html = new StringBuilder();
                foreach (var client in config.Email.Clients)
                {
                    var emailClient = Common.Platform.Email.GetClient(client.Key);
                    viewClient.Clear();
                    viewClient["id"] = client.Id.ToString();
                    viewClient["label"] = client.Label;
                    viewClient["type"] = emailClient.Name;
                    viewClient["from-name"] = client.Parameters[emailClient.FromNameKey];
                    viewClient["from"] = client.Parameters[emailClient.FromKey];
                    html.Append(viewClient.Render());
                }
                viewClientList["clients"] = html.ToString();
                return viewClientList.Render();
            }
            else
            {
                //no clients exist yet
                return Cache.LoadFile("/Views/WebsiteSettings/email/no-clients.html");
            }
        }

        public string RenderEmailClient(string clientId)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }

            var view = new View("/Views/WebsiteSettings/email/client-form.html");
            var clientoptions = new StringBuilder();
            var clientforms = new StringBuilder();
            var emailClients = new List<Vendor.IVendorEmailClient>();
            emailClients.AddRange(Saber.Core.Vendors.EmailClients.Values.OrderBy(a => a.Name));
            var id = new Guid();
            if (!string.IsNullOrEmpty(clientId)) { id = new Guid(clientId); }
            var clientConfig = !string.IsNullOrEmpty(clientId) ? Email.GetClientConfig(id) : null;
            view["id"] = clientId;
            if (clientConfig != null)
            {
                view["label"] = clientConfig.Label;
            }

            foreach (var client in emailClients)
            {
                clientoptions.Append("<option value=\"" + client.Key + "\"" + (
                    clientConfig != null && clientConfig.Key == client.Key ? " selected=\"selected\"" : ""
                    ) + ">" + client.Name + "</option>");
            
                //generate email client form
                clientforms.Append("<div class=\"email-client client-" + client.Key + (client.Key != "smtp" ? " hide" : "") + "\">");
                foreach(var param in client.Parameters)
                {
                    var value = (clientConfig != null && clientConfig.Parameters.ContainsKey(param.Key) ? clientConfig.Parameters[param.Key] : param.Value.DefaultValue).Replace("\"", "&quot;");
                    var idAttr = " id=\"" + client.Key + "_" + param.Key + "\"";
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
                            clientforms.Append("<div class=\"row input\"><input type=\"text\"" + idAttr + " value=\"" + value + "\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.UserOrEmail:
                            clientforms.Append("<div class=\"row input\"><input type=\"text\"" + idAttr + " value=\"" + value + "\" autocomplete=\"new-email\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.Password:
                            clientforms.Append("<div class=\"row input\"><input type=\"password\"" + idAttr + " value=\"" + (value != "" ? "********" : "") + "\" autocomplete=\"new-password\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.Number:
                            clientforms.Append("<div class=\"row input\"><input type=\"number\"" + idAttr + " value=\"" + value + "\"/></div>");
                            break;
                        case Vendor.EmailClientDataType.List:
                            clientforms.Append("<div class=\"row input\"><select" + idAttr + ">" + 
                                string.Join("", param.Value.ListOptions?.Select(a => "<option value=\"" + a + "\">" + a + "</option>") ?? new string[] { "" }) +
                                "</select></div>");
                            break;
                        case Vendor.EmailClientDataType.Boolean:
                            clientforms.Append("<div class=\"row input\"><input type=\"checkbox\"" + idAttr + (value == "1" || value.ToLower() == "true" ? " checked=\"checked\"" : "") + " />" + 
                                "<label for=\"" + client.Key + "_" + param.Key + "\">" + param.Value.Name + "</label>" +
                                "</div>");
                            break;
                    }   
                }
                clientforms.Append("</div>");
            }
            view["email-clients"] = clientoptions.ToString();
            view["forms"] = clientforms.ToString();
            return view.Render();

        }

        public string SaveEmailClient(string clientId, string label, string key, Dictionary<string, string> parameters)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            //save vendor email client settings
            if(label == "") { return Error("Email Client requires a unique label"); }
            if (key == "") { return Error("Email Client requires a selected key"); }
            var config = Website.Settings.Load();
            var id = new Guid();
            if (!string.IsNullOrEmpty(clientId))
            {
                id = new Guid(clientId);
            }
            if (string.IsNullOrEmpty(clientId) && config.Email.Clients.Any(a => a.Label == label))
            {
                return Error("Another Email Client already uses the specified label");
            }

            var vendor = Saber.Core.Vendors.EmailClients.Where(a => a.Key == key).FirstOrDefault().Value;
            if (vendor != null)
            {
                if (id == Guid.Empty)
                {
                    //save new client
                    id = Guid.NewGuid();
                    config.Email.Clients.Add(new Models.Website.EmailClient()
                    {
                        Id = id,
                        Key = key,
                        Label = label,
                        Parameters = parameters
                    });
                }
                else
                {
                    //save existing client
                    var client = config.Email.Clients.FirstOrDefault(a => a.Id == id);
                    if (client != null)
                    {
                        client.Label = label;
                        client.Key = key;

                        //check all parameters
                        foreach(var param in vendor.Parameters)
                        {
                            if(param.Value.DataType == Vendor.EmailClientDataType.Password)
                            {
                                //make sure user is actually updating password field
                                if (parameters[param.Key].StartsWith("***"))
                                {
                                    //password has not changed
                                    parameters[param.Key] = client.Parameters[param.Key];
                                }
                            }
                        }
                        client.Parameters = parameters;
                    }
                }
                Website.Settings.Save(config);
            }
            return Success();
        }

        public string RemoveEmailClient(string clientId)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var config = Website.Settings.Load();
            var id = new Guid(clientId);
            if(config.Email.Clients.Any(a => a.Id == id))
            {
                config.Email.Clients.Remove(config.Email.Clients.FirstOrDefault(a => a.Id == id));
            }
            Website.Settings.Save(config);
            return Success();
        }

        public string RenderEmailAction(string key)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var view = new View("/Views/WebsiteSettings/email/action-form.html");
            var config = Website.Settings.Load();
            var action = Email.GetAction(key);
            var actionConfig = Email.GetActionConfig(key);
            var clients = config.Email.Clients.OrderBy(a => a.Label);
            view["key"] = key;
            view["name"] = action.Name;
            view["email-clients"] = (actionConfig == null ? "<option value=\"\"></option>" : "") + 
                string.Join("", clients.Select(a => "<option value=\"" + a.Id + "\"" + 
                (actionConfig != null && actionConfig.ClientId == a.Id ? " selected=\"selected\"" : "") + 
                ">" + a.Label + "</option>"));
            if(actionConfig != null)
            {
                view["subject"] = actionConfig.Subject;
            }
            return view.Render();
        }

        public string RenderEmailActions()
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var config = Common.Platform.Website.Settings.Load();
            var viewEmailAction = new View("/Views/WebsiteSettings/email/action.html");
            var actions = Common.Platform.Email.Actions;
            var html = new StringBuilder();

            foreach (var action in actions)
            {
                var configAction = config.Email.Actions.Where(a => a.Type == action.Key).FirstOrDefault();
                var configEmail = configAction != null ? Common.Platform.Email.GetClientConfig(configAction.ClientId) : null;
                viewEmailAction.Bind(new
                {
                    action = new
                    {
                        key = action.Key,
                        name = action.Name,
                        templatefile = action.TemplateFile,
                        subject = configAction?.Subject ?? "<span class=\"err\">No subject written</span>",
                        client = configEmail != null ? configEmail.Label : "<span class=\"err\">No client selected</span>"
                    }
                });
                if (action.UserDefinedSubject) { viewEmailAction.Show("user-subject"); }
                if (action.TemplateFile == "") { viewEmailAction.Show("any-file"); }
                else { viewEmailAction.Show("template-file"); }
                html.Append(viewEmailAction.Render());
                viewEmailAction.Clear();
            }
            return html.ToString();
        }

        public string SaveEmailAction(string type, string clientId, string subject)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            if(string.IsNullOrEmpty(subject)) { return Error("Subject is a required field"); }
            if(string.IsNullOrEmpty(clientId)) { return Error("You must select an Email Client to use for this action"); }
            var config = Website.Settings.Load();
            var configAction = config.Email.Actions.FirstOrDefault(a => a.Type == type);
            if (configAction == null)
            {
                //save new action
                config.Email.Actions.Add(new Models.Website.EmailAction()
                {
                    ClientId = new Guid(clientId),
                    Type = type,
                    Subject = subject
                });
            }
            else
            {
                //save existing action
                configAction.ClientId = new Guid(clientId);
                configAction.Subject = subject;
            }
            Website.Settings.Save(config);
            return Success();
        }

        public string SendTestEmail(string key, string email)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }

            try
            {
                var action = Common.Platform.Email.GetAction(key);
                var templateFile = "/Content/emails/" + (action?.TemplateFile ?? "signup.html");
                var body = "This is a test email sent out from Saber. The email template file \"" +
                    templateFile + "\" was not found, " +
                    "so this suppliment text was automatically generated as a result.";
                if (File.Exists(App.MapPath(templateFile)))
                {
                    var view = new View(templateFile);
                    body = view.Render();
                }
                Email.Send(key, new MailAddress(email, "Anonymous Tester"), body);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
            return Success();
        }
        #endregion

        #region "Passwords"
        public string SavePasswords(Models.Website.Passwords passwords)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            if(passwords == null) { return Error("No data recieved"); }
            var settings = Common.Platform.Website.Settings.Load();
            settings.Passwords = passwords;
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }
        #endregion

        #region "Public API"
        public string SavePublicApi(bool enabled, string api)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            if (Common.Platform.PublicApi.Name == api)
            {
                if (Common.Platform.PublicApi.Enabled != enabled)
                {
                    Query.PublicApis.Update(Common.Platform.PublicApi.Name, enabled);
                    Common.Platform.PublicApi.Enabled = true;
                }
            }
            else
            {
                var publicApis = Common.Platform.PublicApi.GetList();
                if (!publicApis.Any(a => a.Path == api) || publicApis.Where(a => a.Path == api).FirstOrDefault()?.Enabled != enabled)
                {
                    Common.Platform.PublicApi.Update(api, enabled);
                }
            }
            return Success();
        }
        #endregion

        #region "Plugins"
        public string UninstallPlugin(string key)
        {
            //create file delete.tmp in Vendor plugin folder
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            File.WriteAllText(App.MapPath("/Vendors/" + key + "/uninstall.sbr"), "");
            Server.AppLifetime.StopApplication();
            return Success();
        }
        #endregion

        #region "Page Title"
        public string UpdatePageTitle(string oldTitle, string newTitle, Models.Website.PageTitleType type)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            if(newTitle == "") { return Error("Page Title is empty"); }
            var settings = Common.Platform.Website.Settings.Load();
            if (settings.PageTitles.Any(a => a.Value == newTitle && a.Type == type)) { return Error("Page Title already exists"); }
            var pageTitle = settings.PageTitles.FirstOrDefault(a => a.Value == oldTitle && a.Type == type);
            if (pageTitle != null)
            {
                pageTitle.Value = newTitle;
            }
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }

        public string RemovePageTitle(string title, Models.Website.PageTitleType type)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var settings = Common.Platform.Website.Settings.Load();
            var pageTitle = settings.PageTitles.FirstOrDefault(a => a.Value == title && a.Type == type);
            if(pageTitle != null)
            {
                settings.PageTitles.Remove(pageTitle);
            }
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }
        #endregion

        #region "Languages"
        public string UpdateLanguage(string abbr, string name)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            if (name == "") { return Error("Language is empty"); }
            var settings = Common.Platform.Website.Settings.Load();
            if (settings.Languages.Any(a => a.Name == name && a.Id != abbr)) { return Error("Language already exists"); }
            var language = settings.Languages.FirstOrDefault(a => a.Id == abbr);
            if (language != null)
            {
                language.Name = name;
            }
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }

        public string RemoveLanguage(string abbr)
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var settings = Common.Platform.Website.Settings.Load();
            var language = settings.Languages.FirstOrDefault(a => a.Id == abbr);
            if (language != null)
            {
                settings.Languages.Remove(language);
            }
            Common.Platform.Website.Settings.Save(settings);
            return Success();
        }
        #endregion

        #region "Restart"
        public string Restart()
        {
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            Common.Platform.Website.Restart();
            return Success();
        }
        #endregion
    }
}
