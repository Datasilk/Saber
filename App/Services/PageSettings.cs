using System.Text;
using System.Text.Json;
using Saber.Common.Platform;

namespace Saber.Services
{
    public class PageSettings : Service
    {
        public string Render(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = PageInfo.GetPageConfig(path);
            var webconfig = Website.Settings.Load();
            var view = new View("/Views/PageSettings/pagesettings.html");
            var fieldView = new View("/Views/PageSettings/partial-field.html");
            var prefixes = new StringBuilder();
            var suffixes = new StringBuilder();

            //generate list of page prefixes
            var titles = webconfig.PageTitles;
            prefixes.Append("<option value=\"\">[None]</option>\n");
            suffixes.Append("<option value=\"\">[None]</option>\n");
            foreach (var t in titles)
            {
                if (t.Type == Models.Website.PageTitleType.Prefix)
                {
                    prefixes.Append("<option value=\"" + t.Value + "\"" + (config.Title.prefix == t.Value ? " selected" : "") + ">" + t.Value + "</option>\n");
                }
                else
                {
                    suffixes.Append("<option value=\"" + t.Value + "\"" + (config.Title.suffix == t.Value ? " selected" : "") + ">" + t.Value + "</option>\n");
                }
            }

            //get all platform-specific html variables
            var htmlVars = Core.Vendors.HtmlComponentKeys;

            //generate list of page headers & footers
            var headers = new List<string>();
            var footers = new List<string>();
            var files = Directory.GetFiles(App.MapPath("/Content/partials/"), "*.html", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var paths = file.Replace(App.RootPath, "").Split('\\').ToList();
                var startIndex = paths.FindIndex(f => f == "partials");
                paths = paths.Skip(startIndex + 1).ToList();
                var filepath = "/Content/partials/" + string.Join('/', paths.ToArray());
                var filename = paths[paths.Count - 1];

                //get list of fields within html template
                if (filename.IndexOf("header") >= 0 || filepath.IndexOf("header") > 0)
                {
                    headers.Add(filepath.Replace("/Content/partials/", ""));
                }
                else if (filename.IndexOf("footer") >= 0 || filepath.IndexOf("footer") > 0)
                {
                    footers.Add(filepath.Replace("/Content/partials/", ""));
                }

            }

            //render header & footer select lists
            var headerList = new StringBuilder();
            var footerList = new StringBuilder();
            var headerFields = new StringBuilder();
            var footerFields = new StringBuilder();

            foreach (var header in headers)
            {
                if(!config.UsesLiveTemplate || (config.UsesLiveTemplate && config.Header == header))
                {
                    headerList.Append("<option value=\"" + header + "\"" +
                        (config.Header == header || config.Header == "" ? " selected" : "") +
                        ">" + header + "</option>\n");
                }
            }

            foreach (var footer in footers)
            {
                if (!config.UsesLiveTemplate || (config.UsesLiveTemplate && config.Footer == footer))
                {
                    footerList.Append("<option value=\"" + footer + "\"" +
                        (config.Footer == footer || config.Footer == "" ? " selected" : "") +
                        ">" + footer + "</option>\n");
                }
            }

            //render various elements
            view["page-title"] = config.Title.body;
            view["page-title-prefixes"] = prefixes.ToString();
            view["page-title-suffixes"] = suffixes.ToString();
            view["page-description"] = config.Description;
            view["page-header-list"] = headerList.ToString();
            view["page-footer-list"] = footerList.ToString();
            view["styles-list"] = RenderStylesheetsList(config);
            view["scripts-list"] = RenderScriptsList(config);
            view["security-list"] = RenderSecurityGroupsList(config);
            var templateItemView = new View("/Views/PageSettings/template-item.html");
            var templateItemUrl = path.Replace("content/pages/", "/") + "/template";
            templateItemView["target"] = " target=\"_blank\"";
            templateItemView["url"] = templateItemUrl;
            templateItemView["title"] = templateItemUrl;
            view["page-template"] = templateItemView.Render();

            if (config.Paths[^1] == "template")
            {
                view.Show("is-template");
                if (config.IsLiveTemplate)
                {
                    view.Show("is-live-template");
                }
                else
                {
                    view.Show("convert-live-template");
                }
            }
            else
            {
                view.Show("subpage-template");
                if (config.IsFromTemplate || config.UsesLiveTemplate || config.FromLiveTemplate)
                {
                    view.Show("uses-page-template");
                    templateItemUrl = "/" + config.TemplatePath.ToLower().Replace("content/pages/", "");
                    templateItemView["url"] = templateItemUrl;
                    templateItemView["title"] = templateItemUrl;
                    view["parent-template"] = templateItemView.Render();
                    if (config.UsesLiveTemplate)
                    {
                        view.Show("uses-live-template");
                        //view.Show("no-live-template");
                        if(config.LiveStylesheets.Count > 0 || config.LiveScripts.Count > 0)
                        {
                            var liveFiles = new List<string>();
                            liveFiles.AddRange(config.LiveStylesheets);
                            liveFiles.AddRange(config.LiveScripts);
                            view["live-styles-scripts"] = "<p>The following resources are loaded onto this sub-page from the <b>Live Template</b>:</p>" +
                                string.Join("\n", liveFiles.Select(a =>
                                {
                                    var liveFile = a.Replace("/content/", "Content/");
                                    if (liveFile.Contains(".css"))
                                    {
                                        //check to see if LESS file exists
                                        if (File.Exists(App.MapPath("/" + liveFile.Replace(".css", ".less"))))
                                        {
                                            liveFile = liveFile.Replace(".css", ".less");
                                        }
                                    }
                                    liveFile = liveFile.ToLower();
                                    templateItemView.Clear();
                                    templateItemView["url"] = "javascript:S.editor.explorer.open('" + liveFile + "')";
                                    templateItemView["title"] = liveFile;
                                    return templateItemView.Render();
                                }));
                        }
                    }
                    else if (config.FromLiveTemplate)
                    {
                        view.Show("use-live-template");
                    }
                }
            }

            //build JSON Response object
            return JsonResponse(

                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .page-settings .settings-contents",
                    html = Common.Platform.Render.View(this, view),
                    css = Css.ToString(),
                    javascript = Scripts.ToString(),
                    json = JsonSerializer.Serialize(new { headers, footers, field_template = fieldView.HTML })
                }
            );
        }

        public string UpdatePageTitle(string path, string prefix, string suffix, string title)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.Title.body = title;
                config.Title.prefix = prefix;
                config.Title.suffix = suffix;
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string CreatePageTitlePart(string title, bool prefix)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            //add space at end if user didn't
            if (prefix == true)
            {
                if (title.Last() != ' ') { title += " "; }
            }
            else
            {
                if (title.First() != ' ') { title = " " + title; }
            }

            try
            {
                var config = Website.Settings.Load();
                var type = prefix ? Models.Website.PageTitleType.Prefix : Models.Website.PageTitleType.Suffix;
                if (!config.PageTitles.Any(a => a.Value == title && a.Type == type))
                {
                    config.PageTitles.Add(new Models.Website.PageTitle() { 
                        Value = title, 
                        Type = type
                    });
                    Website.Settings.Save(config);
                    return title;
                }
                return Error();
            }
            catch (Exception) { return Error(); }
        }

        public string DeletePageTitlePart(string title, bool prefix)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            //add space at end if user didn't
            if (prefix == true)
            {
                if (title.Last() != ' ') { title += " "; }
            }
            else
            {
                if (title.First() != ' ') { title = " " + title; }
            }

            try
            {
                var config = Website.Settings.Load();
                var type = prefix ? Models.Website.PageTitleType.Prefix : Models.Website.PageTitleType.Suffix;
                if (config.PageTitles.Any(a => a.Value == title && a.Type == type))
                {
                    config.PageTitles.Remove(config.PageTitles.Where(a => a.Value == title && a.Type == type).FirstOrDefault());
                    Website.Settings.Save(config);
                    return title;
                }
                return Error();
            }
            catch (Exception) { return Error(); }
        }

        public string UpdatePageDescription(string path, string description)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.Description = description;
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string UpdatePagePartials(string path, string header, string footer)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.Header = header;
                config.Footer = footer;
                PageInfo.SavePageConfig(path, config);
                //check if page HTML file exists
                //var paths = PageInfo.GetRelativePath(path);
                //var relpath = string.Join("/", paths);
                //if(!File.Exists(App.MapPath(relpath + ".html")))
                //{
                //    File.WriteAllText(App.MapPath(relpath + ".html"), Settings.DefaultHtml);
                //}
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        #region "Page Stylesheets"

        public string RenderStylesheetsList(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = PageInfo.GetPageConfig(path);
            return RenderStylesheetsList(config);
        }

        private string RenderStylesheetsList(Models.Page.Settings config)
        {
            var styleItem = new View("/Views/PageSettings/style-item.html");
            var styles = new StringBuilder();
            if (config.Stylesheets.Count > 0)
            {
                foreach (var style in config.Stylesheets)
                {
                    styleItem["style"] = style;
                    styleItem["style-path"] = style.Replace("/content/", "");
                    styles.Append(styleItem.Render());
                }
            }
            return styles.ToString();
        }

        public string GetAvailableStylesheets ()
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
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
            RecurseDirectoriesForStylesheets(list, App.MapPath("/wwwroot/css"));
            RecurseDirectoriesForStylesheets(list, App.MapPath("/wwwroot/content"));
            var root = App.MapPath("/") + "\\";
            var rel = new List<string>();
            foreach (var i in list)
            {
                rel.Add("/" + i.Replace(root, "").Replace("\\", "/").Replace("wwwroot/",""));
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

            foreach(var d in dir.GetDirectories())
            {
                RecurseDirectoriesForStylesheets(list, d.FullName);
            }
        }

        public string AddStylesheetToPage(string file, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                if(file == "") { return Error(); }
                var config = PageInfo.GetPageConfig(path);
                if (!config.Stylesheets.Contains(file))
                {
                    config.Stylesheets.Add(file);
                }
                PageInfo.SavePageConfig(path, config);
                return RenderStylesheetsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveStylesheetFromPage(string file, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                if (config.Stylesheets.Contains(file))
                {
                    config.Stylesheets.Remove(file);
                }
                PageInfo.SavePageConfig(path, config);
                return RenderStylesheetsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string SortStylesheets(List<string> stylesheets, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.Stylesheets = stylesheets;
                PageInfo.SavePageConfig(path, config);
                return RenderStylesheetsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        #endregion

        #region "Page Scripts"

        public string RenderScriptsList(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = PageInfo.GetPageConfig(path);
            return RenderScriptsList(config);
        }

        private string RenderScriptsList(Models.Page.Settings config)
        {
            var scriptItem = new View("/Views/PageSettings/script-item.html");
            var scripts = new StringBuilder();
            if (config.Scripts.Count > 0)
            {
                foreach (var script in config.Scripts)
                {
                    scriptItem["script"] = script;
                    scriptItem["script-path"] = script.Replace("/content/", "");
                    scripts.Append(scriptItem.Render());
                }
            }
            return scripts.ToString();
        }

        public string GetAvailableScripts()
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
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
            var list = new List<string>()
            {
                "editor/js/platform.js",
                "editor/js/selector.js",
                "editor/js/utility/velocity.min.js"
            };
            RecurseDirectoriesForScripts(list, App.MapPath("/wwwroot/js"));
            RecurseDirectoriesForScripts(list, App.MapPath("/wwwroot/content"));
            var root = App.MapPath("/") + "\\";
            var rel = new List<string>();
            foreach (var i in list)
            {
                rel.Add("/" + i.Replace(root, "").Replace("\\", "/").Replace("wwwroot/",""));
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

        public string AddScriptToPage(string file, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                if(file == "") { return Error(); }
                var config = PageInfo.GetPageConfig(path);
                if (!config.Scripts.Contains(file))
                {
                    config.Scripts.Add(file);
                }
                PageInfo.SavePageConfig(path, config);
                return RenderScriptsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveScriptFromPage(string file, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                if (config.Scripts.Contains(file))
                {
                    config.Scripts.Remove(file);
                }
                PageInfo.SavePageConfig(path, config);
                return RenderScriptsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string SortScripts(List<string> scripts, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.Scripts = scripts;
                PageInfo.SavePageConfig(path, config);
                return RenderScriptsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        #endregion

        #region "Security Groups"

        public string RenderSecurityGroupsList(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = PageInfo.GetPageConfig(path);
            return RenderSecurityGroupsList(config);
        }

        private string RenderSecurityGroupsList(Models.Page.Settings config)
        {
            var groupItem = new View("/Views/PageSettings/group-item.html");
            var html = new StringBuilder();
            if (config.Security.groups.Length > 0)
            {
                var groups = Query.Security.Groups.GetListByIds(config.Security.groups);
                if (groups != null && groups.Count > 0)
                {
                    foreach(var group in groups)
                    {
                        groupItem["id"] = group.groupId.ToString();
                        groupItem["name"] = group.name;
                        html.Append(groupItem.Render());
                        groupItem.Clear();
                    }
                }
            }
            return html.ToString();
        }

        public string GetAvailableSecurityGroups()
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            var groups = Query.Security.Groups.GetList();
            if(groups != null && groups.Count > 0)
            {
                return JsonResponse(groups.Select(a => new { id = a.groupId, name = a.name }).ToArray());
            }
            return "";
        }

        public string AddSecurityGroup(int groupId, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                if (!config.Security.groups.Contains(groupId))
                {
                    var groups = config.Security.groups.ToList();
                    groups.Add(groupId);
                    config.Security.groups = groups.ToArray();
                }
                PageInfo.SavePageConfig(path, config);
                return RenderSecurityGroupsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveSecurityGroup(int groupId, string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                if (config.Security.groups.Contains(groupId))
                {
                    var groups = config.Security.groups.ToList();
                    groups.Remove(groupId);
                    config.Security.groups = groups.ToArray();
                }
                PageInfo.SavePageConfig(path, config);
                return RenderSecurityGroupsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        #endregion

        #region "Live Template"
        public string ConvertToLiveTemplate(string path)
        {
            if (IsPublicApiRequest || !CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.IsLiveTemplate = true;
                config.Save();
                var newpath = string.Join("/", config.Paths);

                //update all sub-pages to use live template
                var dir = new DirectoryInfo(string.Join("/", config.Paths.Take(config.Paths.Length - 1)));
                foreach (var file in dir.GetFiles("*.html", SearchOption.AllDirectories))
                {
                    if (file.FullName.Replace("\\", "/").Contains(newpath + "/template.html")) { continue; }
                    config = PageInfo.GetPageConfig(file.FullName.Replace("\\", "/").Replace(App.RootPath, "").Replace(".html", ""));
                    config.UsesLiveTemplate = true;
                    config.FromLiveTemplate = true;
                    config.Save();
                }
            }
            catch (Exception)
            {
                return Error();
            }
            return Success();
        }
        #endregion
    }
}
