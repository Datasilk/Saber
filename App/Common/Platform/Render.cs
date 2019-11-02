using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CommonMark;

namespace Saber.Common.Platform
{
    public class Render
    {
        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public static string Page(string path, Request request, Models.Page.Settings config)
        {
            //translate root path to relative path
            var content = new View("/Views/Editor/content.html");
            var header = new View("/Content/partials/" + (config.header.file != "" ? config.header.file : "header.html"));
            var footer = new View("/Content/partials/" + (config.footer.file != "" ? config.footer.file : "footer.html"));
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }

            //check file path on drive for (estimated) OS folder structure limitations 
            if (Server.MapPath(relpath).Length > 180)
            {
                throw new ServiceErrorException("The URL path you are accessing is too long to handle for the web server");
            }

            var uselayout = true;
            if (request.Parameters.ContainsKey("noLayout"))
            {
                uselayout = false;
            }
            var view = new View(relpath);
            if (view.Elements.Count == 0)
            {
                if (request.User.userId == 0)
                {
                    //TODO: Show user-generated 404 error
                    view.HTML = "<p>This page does not exist. Please log into your account to write content for this page.</p>";
                }
                else
                {
                    //try to load template page from parent
                    view.HTML = "<p>Write content using HTML & CSS</p>";
                }
            }

            //check security
            if (config.security.secure == true)
            {
                if (!request.CheckSecurity() || !config.security.read.Contains(request.User.userId))
                {
                    throw new ServiceDeniedException("You do not have read access for this page");
                }
            }

            //load user content from json file, depending on selected language
            var lang = request.User.language;
            var contentfile = ContentFields.ContentFile(path, lang);
            var contents = Server.LoadFileFromCache(contentfile);
            if(contents != "")
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(Server.LoadFileFromCache(contentfile));
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        if (item.Value.IndexOf("\n") >= 0)
                        {
                            view[item.Key] = CommonMarkConverter.Convert(item.Value);
                        }
                        else
                        {
                            view[item.Key] = item.Value;
                        }
                    }
                }
            }
            

            //load platform-specific data into scaffold template
            var results = GetPlatformData(view, request);
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
                results = GetPlatformData(header, request);
                results.AddRange(config.header.fields);
                if (results.Count > 0)
                {
                    foreach (var item in results)
                    {
                        header[item.Key] = item.Value;
                    }
                }
                results = GetPlatformData(footer, request);
                results.AddRange(config.footer.fields);
                if (results.Count > 0)
                {
                    foreach (var item in results)
                    {
                        footer[item.Key] = item.Value;
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

        private static List<KeyValuePair<string, string>> GetPlatformData(View view, Request request)
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

                //get platform data from the Scaffold Data Binder
                var vars = ViewDataBinder.HtmlVars;
                foreach (var item in vars)
                {
                    if (view.Fields.ContainsKey(prefix + item.Key))
                    {
                        var index = results.FindAll(f => f.Key == item.Key).Count();
                        var elemIndex = view.Fields[prefix + item.Key][index];
                        var args = view.Elements[elemIndex].Vars ?? new Dictionary<string, string>();
                        //prepare html template variable arguments for the Data Binder
                        var argList = args.Select(a => a.Key + ":\"" + a.Value + "\"").ToArray();
                        var argsStr = "";
                        if (argList.Length > 0)
                        {
                            argsStr = string.Join(',', argList);
                        }

                        //run the Data Binder callback method
                        var range = item.Value.Callback(request, argsStr, prefix);
                        if(range.Count > 0)
                        {
                            //add Data Binder callback method results to list
                            results.AddRange(range);
                        }
                    }
                }
            }
            

            return results;
        }
    }
}
