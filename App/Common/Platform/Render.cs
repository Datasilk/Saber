using System.Collections.Generic;
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
            var content = new View("/Views/Editor/content.html");
            var header = new View("/Content/partials/" + (config.Header != "" ? config.Header : "header.html"));
            var footer = new View("/Content/partials/" + (config.Footer != "" ? config.Footer : "footer.html"));
            var paths = path.Split("/");
            var relpath = "/" + string.Join("/", PageInfo.GetRelativePath(path));
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
            if (config.UsesLiveTemplate)
            {
                //load live template instead
                relpath = config.TemplatePath + ".html";
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
                    view.HTML = Settings.DefaultHtml;
                }
            } 

            //check security
            if (config.Security.groups.Length > 0)
            {
                if (!request.CheckSecurity() || (request.User.IsAdmin == false && !config.Security.groups.Any(a => request.User.Groups.Contains(a))))
                {
                    throw new ServiceDeniedException("You do not have access to this page");
                }
            }

            //load user content from json file, depending on selected language
            var data = Core.ContentFields.GetPageContent(path, language).ToDictionary(a => a.Key, a => (object)a.Value);

            if(view.Elements.Count > 0)
            {
                if (data.Count > 0)
                {
                    //get view blocks
                    var blocks = view.Elements.Where(a => a.Name.StartsWith("/")).Select(a => a.Name.Substring(1));
                    foreach (var item in data)
                    {
                        var value = (string)item.Value;
                        if (blocks.Contains(item.Key))
                        {
                            if (value == "1")
                            {
                                view.Show(item.Key);
                            }
                        }
                        else
                        {
                            if (value.Contains('\n'))
                            {
                                view[item.Key] = Markdown.ToHtml(value, markdownPipeline);
                            }
                            else
                            {
                                view[item.Key] = value;
                            }
                        }
                    }
                }

                //load platform-specific data into view
                var results = HtmlComponents(view, request, data);
                if (results.Count > 0)
                {
                    foreach (var item in results)
                    {
                        view[item.Key] = item.Value;
                    }
                }
            }
            
            if (config.UsesLiveTemplate && view.Elements.Any(a => a.Name == "content"))
            {
                //load sub-page view into Live Template
                var pageview = new View("/" + string.Join("/", paths));
                if(pageview.Elements.Count > 0)
                {
                    if (data.Count > 0)
                    {
                        //get view blocks
                        var blocks = pageview.Elements.Where(a => a.Name.StartsWith("/")).Select(a => a.Name.Substring(1));
                        foreach (var item in data)
                        {
                            var value = (string)item.Value;
                            if (blocks.Contains(item.Key))
                            {
                                if (value == "1")
                                {
                                    pageview.Show(item.Key);
                                }
                            }
                            else
                            {
                                if (value.Contains('\n'))
                                {
                                    pageview[item.Key] = Markdown.ToHtml(value, markdownPipeline);
                                }
                                else
                                {
                                    pageview[item.Key] = value;
                                }
                            }
                        }
                    }

                    //load platform-specific data into sub-page view
                    var results = HtmlComponents(pageview, request, data);
                    if (results.Count > 0)
                    {
                        foreach (var item in results)
                        {
                            pageview[item.Key] = item.Value;
                        }
                    }
                }
                view["content"] = pageview.Render();
            }


            if (uselayout)
            {
                //render all content
                var data2 = Core.ContentFields.GetPageContent("/Content/partials/" + config.Header, language).ToDictionary(a => a.Key, a => (object)a.Value); ;
                var results = HtmlComponents(header, request, data2);

                foreach (var item in results)
                {
                    header[item.Key] = item.Value;
                }
                foreach (var item in data2)
                {
                    if (!header.ContainsKey(item.Key))
                    {
                        header[item.Key] = (string)item.Value;
                    }
                }
                data2 = Core.ContentFields.GetPageContent("/Content/partials/" + config.Footer, language).ToDictionary(a => a.Key, a => (object)a.Value);
                results = HtmlComponents(footer, request, data2);
                foreach (var item in results)
                {
                    footer[item.Key] = item.Value;
                }
                foreach (var item in data2)
                {
                    if (!footer.ContainsKey(item.Key))
                    {
                        footer[item.Key] = (string)item.Value;
                    }
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

        public static List<KeyValuePair<string, string>> HtmlComponents(View view, IRequest request, Dictionary<string, object> data)
        {
            var results = new List<KeyValuePair<string, string>>();
            var prefix = "";
            var components = Core.Vendors.HtmlComponents;

            for (var x = -1; x < view.Partials.Count; x++)
            {
                if(x >= 0)
                {
                    //find variables within html template partials (child templates)
                    prefix = view.Partials[x].Prefix;
                }

                //get HTML components from vendors
                foreach (var component in components)
                {
                    var fields = view.Fields.Where(a => {
                        if(component.Value.KeyIsPrefix == true && a.Key.IndexOf(prefix + component.Key) == 0)
                        {
                            //component uses key as a prefix
                            return true;
                        }
                        return a.Key == prefix + component.Key;
                    });
                    if(fields.Count() > 0) {
                        foreach(var field in fields)
                        {
                            var elem = view.Elements[field.Value[0]];
                            var args = elem.Vars ?? new Dictionary<string, string>();
                            //run the Data Binder callback method
                            var range = component.Value.Render(view, request, args, data, prefix, elem.Name);
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
            if (Core.Vendors.ViewRenderers.ContainsKey(view.Filename))
            {
                var renderers = Core.Vendors.ViewRenderers.ContainsKey(view.Filename) == true ? Core.Vendors.ViewRenderers[view.Filename] : null;
                if(renderers != null && renderers.Count > 0)
                {
                    foreach (var renderer in renderers)
                    {
                        vendors.Append(itemHead + renderer.Render(request, view) + itemFoot);
                    }
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
