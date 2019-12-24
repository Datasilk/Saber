using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using Saber.Common.Platform;
using Saber.Common.Extensions.Strings;

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
            if (!CheckSecurity()) { return AccessDenied(); }
            var config = PageInfo.GetPageConfig(path);
            var view = new View("/Views/PageSettings/pagesettings.html");
            var fieldScaffold = new View("/Views/PageSettings/partial-field.html");
            var prefixes = new StringBuilder();
            var suffixes = new StringBuilder();

            //generate list of page prefixes
            var titles = Query.PageTitles.GetList(Query.PageTitles.TitleType.all);
            prefixes.Append("<option value=\"0\">[None]</option>\n");
            suffixes.Append("<option value=\"0\">[None]</option>\n");
            foreach (var t in titles)
            {
                if (t.pos == false)
                {
                    prefixes.Append("<option value=\"" + t.titleId + "\"" + (config.title.prefixId == t.titleId ? " selected" : "") + ">" + t.title + "</option>\n");
                }
                else
                {
                    suffixes.Append("<option value=\"" + t.titleId + "\"" + (config.title.suffixId == t.titleId ? " selected" : "") + ">" + t.title + "</option>\n");
                }
            }

            //get all platform-specific html variables
            var htmlVars = ViewDataBinder.GetHtmlVariables();

            //generate list of page headers & footers
            var headers = new List<Models.Page.Template>();
            var footers = new List<Models.Page.Template>();
            var files = Directory.GetFiles(Server.MapPath("/Content/partials/"), "*.html", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var paths = file.Split('\\').ToList();
                var startIndex = paths.FindIndex(f => f == "partials");
                paths = paths.Skip(startIndex + 1).ToList();
                var filepath = string.Join('/', paths.ToArray());
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
                    var fileScaffold = new View("/Content/partials/" + filepath);
                    var details = new Models.Page.Template()
                    {
                        file = filepath,
                        fields = fileScaffold.Fields.Select(a =>
                        {
                            var configElem = new Models.Page.Template();
                            switch (filetype)
                            {
                                case TemplateFileType.header:
                                    configElem = config.header;
                                    break;
                                case TemplateFileType.footer:
                                    configElem = config.footer;
                                    break;
                            }
                            return new KeyValuePair<string, string>(a.Key,
                                configElem.fields.ContainsKey(a.Key) ? configElem.fields[a.Key] : "");
                        }).Where(a => {
                            var partial = fileScaffold.Partials.Where(b => a.Key.IndexOf(b.Prefix) == 0)
                                .OrderByDescending(o => o.Prefix.Length).FirstOrDefault();
                            var prefix = "";
                            var naturalKey = a.Key;
                            if (partial != null)
                            {
                                prefix = partial.Prefix;
                                naturalKey = a.Key.Replace(prefix, "");
                            }
                            return !htmlVars.Contains(naturalKey);
                        }
                    ).ToDictionary(a => a.Key, b => b.Value)
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
                        fieldScaffold["label"] = field.Key.Replace("-", " ").Capitalize();
                        fieldScaffold["name"] = field.Key;
                        fieldScaffold["value"] = field.Value;
                        headerFields.Append(fieldScaffold.Render() + "\n");
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
                        fieldScaffold["label"] = field.Key.Replace("-", " ").Capitalize();
                        fieldScaffold["name"] = field.Key;
                        fieldScaffold["value"] = field.Value;
                        headerFields.Append(fieldScaffold.Render() + "\n");
                    }
                }
            }


            //render template elements
            view["page-title"] = config.title.body;
            view["page-title-prefixes"] = prefixes.ToString();
            view["page-title-suffixes"] = suffixes.ToString();
            view["page-description"] = config.description;
            view["page-header-list"] = headerList.ToString();
            view["page-footer-list"] = footerList.ToString();
            view["page-template"] = path.Replace("content/", "/") + "/template";
            view["no-header-fields"] = headerFields.Length == 0 ? "1" : "";
            view["no-footer-fields"] = footerFields.Length == 0 ? "1" : "";
            view["header-fields"] = headerFields.ToString();
            view["footer-fields"] = footerFields.ToString();

            //build JSON Response object
            return JsonSerializer.Serialize(

                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .page-settings .settings-contents",
                    html = RenderView(view),
                    css = Css.ToString(),
                    javascript = Scripts.ToString(),
                    json = JsonSerializer.Serialize(new { headers, footers, field_template = fieldScaffold.HTML })
                }
            );
        }

        public string UpdatePageTitle(string path, int prefixId, int suffixId, string title)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.title.body = title;
                config.title.prefixId = prefixId;
                config.title.suffixId = suffixId;
                if (prefixId == 0)
                {
                    config.title.prefix = "";
                }
                else
                {
                    config.title.prefix = Query.PageTitles.Get(prefixId);
                }
                if (suffixId == 0)
                {
                    config.title.suffix = "";
                }
                else
                {
                    config.title.suffix = Query.PageTitles.Get(suffixId);
                }
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        /// <summary>
        /// Creates a partial page title, such as the name of the website or the authors name, 
        /// which can be used as a prefix or suffix for the web page title
        /// </summary>
        /// <param name="title"></param>
        /// <param name="prefix">Whether or not the page title part is a prefix (true) or suffix (false)</param>
        /// <returns></returns>
        public string CreatePageTitlePart(string title, bool prefix)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
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
                var id = Query.PageTitles.Create(title, !prefix);
                return id + "|" + title;
            }
            catch (Exception) { return Error(); }
        }

        public string UpdatePageDescription(string path, string description)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.description = description;
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public string UpdatePagePartials(string path, Models.Page.Template header, Models.Page.Template footer)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.header = header;
                config.footer = footer;
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }
    }
}
