﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saber.Core;
using Markdig;

namespace Saber.Common.Platform
{
    public class Render
    {
        #region "Page"

        private static MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public static string Page(string path, IRequest request, Models.Page.Settings config, string language = "en")
        {
            //translate root path to relative path
            if (App.Environment == Environment.development) { ViewCache.Clear(); }
            var content = new View("/Views/Editor/content.html");
            var header = new View("/Content/partials/" + (config.header != "" ? config.header : "header.html"));
            var footer = new View("/Content/partials/" + (config.footer != "" ? config.footer : "footer.html"));
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }

            //check file path on drive for (estimated) OS folder structure limitations 
            if (App.MapPath(relpath).Length > 180)
            {
                throw new ServiceErrorException("The URL path you are accessing is too long to handle for the web server");
            }

            var uselayout = true;
            if (request.Parameters.ContainsKey("nolayout"))
            {
                uselayout = false;
            }
            var view = new View(relpath);
            if (view.Elements.Count == 0)
            {
                if (request.User.UserId == 0 || request.Parameters.ContainsKey("live"))
                {
                    if(path != "/Content/pages/404.html")
                    {
                        //Show user-generated 404 error
                        return Page("/Content/pages/404.html", request, config, language);
                    }
                    else
                    {
                        //Show internal 404 error
                        view = new View("/Views/404.html");
                    }
                }
                else
                {
                    //try to load template page from parent
                    view.HTML = "<p>Write content using HTML & CSS</p>";
                }
            }

            //check security
            if (config.security.groups.Length > 0)
            {
                if (!request.CheckSecurity() || (request.User.UserId != 1 && !config.security.groups.Any(a => request.User.Groups.Contains(a))))
                {
                    throw new ServiceDeniedException("You do not have access to this page");
                }
            }

            //load user content from json file, depending on selected language
            var data = ContentFields.GetPageContent(path, language);

            if (data.Count > 0)
            {
                //get view blocks
                var blocks = view.Elements.Where(a => a.Name.StartsWith("/")).Select(a => a.Name.Substring(1));
                foreach (var item in data)
                {
                    if (blocks.Contains(item.Key))
                    {
                        if (item.Value == "1")
                        {
                            view.Show(item.Key);
                        }
                    }
                    else
                    {
                        if (item.Value.Contains('\n'))
                        {
                            view[item.Key] = Markdown.ToHtml(item.Value, markdownPipeline);
                        }
                        else
                        {
                            view[item.Key] = item.Value;
                        }
                    }
                }
            }
            

            //load platform-specific data into view template
            var results = HtmlComponents(view, request, data);
            if (results.Count > 0)
            {
                foreach (var item in results)
                {
                    view[item.Key] = item.Value;
                }
            }

            if(uselayout)
            {
                //render all content
                results = HtmlComponents(header, request, data);

                foreach (var item in results)
                {
                    header[item.Key] = item.Value;
                }
                var data2 = ContentFields.GetPageContent("/Content/partials/" + config.header, language);
                foreach (var item in data2)
                {
                    header[item.Key] = item.Value;
                }
                results = HtmlComponents(footer, request, data);
                foreach (var item in results)
                {
                    footer[item.Key] = item.Value;
                }
                data2 = ContentFields.GetPageContent("/Content/partials/" + config.footer, language);
                foreach (var item in data2)
                {
                    footer[item.Key] = item.Value;
                }
                content["content"] = view.Render();
                return header.Render() + content.Render() + footer.Render();
            }
            else
            {
                //don't render header or footer
                return view.Render();
            }
        }

        private static List<KeyValuePair<string, string>> HtmlComponents(View view, IRequest request, Dictionary<string, string> data)
        {
            var results = new List<KeyValuePair<string, string>>();
            var prefix = "";
            for(var x = -1; x < view.Partials.Count; x++)
            {
                if(x >= 0)
                {
                    //find variables within html template partials (child templates)
                    prefix = view.Partials[x].Prefix;
                }

                //get HTML components from vendors
                var vars = Vendors.HtmlComponents;
                foreach (var item in vars)
                {
                    var fields = view.Fields.Where(a => a.Key.IndexOf(prefix + item.Key) == 0);
                    if(fields.Count() > 0) {
                        foreach(var field in fields)
                        {
                            var elem = view.Elements[field.Value[0]];
                            var args = elem.Vars ?? new Dictionary<string, string>();
                            var d = data.ContainsKey(elem.Name) ? data[elem.Name] : "";
                            //run the Data Binder callback method
                            var range = item.Value.Render(view, request, args, d, prefix, elem.Name);
                            if (range.Count > 0)
                            {
                                //add HTML component render results to list
                                results.AddRange(range);
                            }
                        }
                    }
                }
            }
            

            return results;
        }
        #endregion

        #region "View"
        public static string View(IRequest request, View view, string head = "", string foot = "", string itemHead = "", string itemFoot = "")
        {
            //check for vendor-related View rendering
            var vendors = new StringBuilder();
            if (Vendors.ViewRenderers.ContainsKey(view.Filename))
            {
                var renderers = Vendors.ViewRenderers[view.Filename];
                foreach (var renderer in renderers)
                {
                    vendors.Append(itemHead + renderer.Render(request, view) + itemFoot);
                }
            }
            if (vendors.Length > 0)
            {
                view["vendor"] = head + vendors.ToString() + foot;
            }

            return view.Render();
        }
        #endregion
    }
}
