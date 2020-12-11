using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using Saber.Common.Platform;
using Saber.Core.Extensions.Strings;

namespace Saber.Services
{
    public class PageSettings : Service
    {
        private enum TemplateFileType
        {
            none = 0,
            header = 1,
            footer = 2
        }

        public string Render(string path)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = Core.PageInfo.GetPageConfig(path);
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
                    prefixes.Append("<option value=\"" + t.Value + "\"" + (config.title.prefix == t.Value ? " selected" : "") + ">" + t.Value + "</option>\n");
                }
                else
                {
                    suffixes.Append("<option value=\"" + t.Value + "\"" + (config.title.suffix == t.Value ? " selected" : "") + ">" + t.Value + "</option>\n");
                }
            }

            //get all platform-specific html variables
            var htmlVars = Common.Vendors.HtmlComponentKeys;

            //generate list of page headers & footers
            var headers = new List<Models.Page.Template>();
            var footers = new List<Models.Page.Template>();
            var files = Directory.GetFiles(App.MapPath("/Content/partials/"), "*.html", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var paths = file.Replace(App.RootPath, "").Split('\\').ToList();
                var startIndex = paths.FindIndex(f => f == "partials");
                paths = paths.Skip(startIndex + 1).ToList();
                var filepath = "/Content/partials/" + string.Join('/', paths.ToArray());
                var filename = paths[paths.Count - 1];

                //get list of fields within html template
                TemplateFileType filetype = TemplateFileType.none;
                if (filename.IndexOf("header") >= 0)
                {
                    filetype = TemplateFileType.header;
                }
                else if (filename.IndexOf("footer") >= 0)
                {
                    filetype = TemplateFileType.footer;
                }
                if (filetype > 0)
                {
                    var details = new Models.Page.Template()
                    {
                        file = filepath.Replace("/Content/partials/", "")
                    };
                    switch (filetype)
                    {
                        case TemplateFileType.header:
                            headers.Add(details);
                            break;
                        case TemplateFileType.footer:
                            footers.Add(details);
                            break;
                    }
                }

            }

            //render header & footer select lists
            var headerList = new StringBuilder();
            var footerList = new StringBuilder();
            var headerFields = new StringBuilder();
            var footerFields = new StringBuilder();

            foreach (var header in headers)
            {
                headerList.Append("<option value=\"" + header.file + "\"" +
                    (config.header.file == header.file || config.header.file == "" ? " selected" : "") +
                    ">" + header.file + "</option>\n");
                if (config.header.file == header.file)
                {
                    foreach (var field in header.fields)
                    {
                        if(htmlVars.Any(a => a == field.Key)) { continue; }
                        fieldView["label"] = field.Key.Replace("-", " ").Capitalize();
                        fieldView["name"] = field.Key;
                        fieldView["value"] = field.Value;
                        headerFields.Append(fieldView.Render() + "\n");
                    }
                }

            }

            foreach (var footer in footers)
            {
                footerList.Append("<option value=\"" + footer.file + "\"" +
                    (config.footer.file == footer.file || config.footer.file == "" ? " selected" : "") +
                    ">" + footer.file + "</option>\n");
                if (config.footer.file == footer.file)
                {
                    foreach (var field in footer.fields)
                    {
                        if (htmlVars.Any(a => a == field.Key)) { continue; }
                        fieldView["label"] = field.Key.Replace("-", " ").Capitalize();
                        fieldView["name"] = field.Key;
                        fieldView["value"] = field.Value;
                        footerFields.Append(fieldView.Render() + "\n");
                    }
                }
            }

            //render various elements
            view["page-title"] = config.title.body;
            view["page-title-prefixes"] = prefixes.ToString();
            view["page-title-suffixes"] = suffixes.ToString();
            view["page-description"] = config.description;
            view["page-header-list"] = headerList.ToString();
            view["page-footer-list"] = footerList.ToString();
            view["page-template"] = path.Replace("content/", "/") + "/template";
            view["scripts-list"] = RenderScriptsList(config);
            view["security-list"] = RenderSecurityGroupsList(config);

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
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = Core.PageInfo.GetPageConfig(path);
                config.title.body = title;
                config.title.prefix = prefix;
                config.title.suffix = suffix;
                Core.PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string CreatePageTitlePart(string title, bool prefix)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
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
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
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
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = Core.PageInfo.GetPageConfig(path);
                config.description = description;
                Core.PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string UpdatePagePartials(string path, Models.Page.Template header, Models.Page.Template footer)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = Core.PageInfo.GetPageConfig(path);
                config.header = header;
                config.footer = footer;
                Core.PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        #region "Page Scripts"

        public string RenderScriptsList(string path)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = Core.PageInfo.GetPageConfig(path);
            return RenderScriptsList(config);
        }

        private string RenderScriptsList(Models.Page.Settings config)
        {
            var scriptItem = new View("/Views/PageSettings/script-item.html");
            var scripts = new StringBuilder();
            if (config.scripts.Count > 0)
            {
                foreach (var script in config.scripts)
                {
                    scriptItem["script"] = script;
                    scripts.Append(scriptItem.Render());
                }
            }
            return scripts.ToString();
        }

        public string GetAvailableScripts()
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
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
            var list = new List<string>();
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
            list.AddRange(dir.GetFiles().Select(a => a.FullName)
                .Where(a => filetypes.Any(b => a.Replace("\\", "/").Split("/")[^1].Split(".")[^1].ToLower() == b)));

            foreach(var d in dir.GetDirectories())
            {
                RecurseDirectoriesForScripts(list, d.FullName);
            }
        }

        public string AddScriptToPage(string file, string path)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                if(file == "") { return Error(); }
                var config = Core.PageInfo.GetPageConfig(path);
                if (!config.scripts.Contains(file))
                {
                    config.scripts.Add(file);
                }
                Core.PageInfo.SavePageConfig(path, config);
                return RenderScriptsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveScriptFromPage(string file, string path)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = Core.PageInfo.GetPageConfig(path);
                if (config.scripts.Contains(file))
                {
                    config.scripts.Remove(file);
                }
                Core.PageInfo.SavePageConfig(path, config);
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
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            var config = Core.PageInfo.GetPageConfig(path);
            return RenderSecurityGroupsList(config);
        }

        private string RenderSecurityGroupsList(Models.Page.Settings config)
        {
            var groupItem = new View("/Views/PageSettings/group-item.html");
            var html = new StringBuilder();
            if (config.security.groups.Length > 0)
            {
                var groups = Query.Security.Groups.GetListByIds(config.security.groups);
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
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            var groups = Query.Security.Groups.GetList();
            if(groups != null && groups.Count > 0)
            {
                return JsonResponse(groups.Select(a => new { id = a.groupId, name = a.name }).ToArray());
            }
            return "";
        }

        public string AddSecurityGroup(int groupId, string path)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = Core.PageInfo.GetPageConfig(path);
                if (!config.security.groups.Contains(groupId))
                {
                    var groups = config.security.groups.ToList();
                    groups.Add(groupId);
                    config.security.groups = groups.ToArray();
                }
                Core.PageInfo.SavePageConfig(path, config);
                return RenderSecurityGroupsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string RemoveSecurityGroup(int groupId, string path)
        {
            if (!CheckSecurity("page-settings")) { return AccessDenied(); }
            try
            {
                var config = Core.PageInfo.GetPageConfig(path);
                if (config.security.groups.Contains(groupId))
                {
                    var groups = config.security.groups.ToList();
                    groups.Remove(groupId);
                    config.security.groups = groups.ToArray();
                }
                Core.PageInfo.SavePageConfig(path, config);
                return RenderSecurityGroupsList(config);
            }
            catch (Exception)
            {
                return Error();
            }
        }

        #endregion
    }
}
