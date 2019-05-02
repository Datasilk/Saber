using System.Collections.Generic;
using System.Linq;
using Utility.Serialization;
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
        public static string Page(string path, Datasilk.Web.Request request, Models.Page.Settings config)
        {
            //translate root path to relative path
            var content = new Scaffold("/Views/Editor/content.html");
            var header = new Scaffold("/Content/partials/" + (config.header.file != "" ? config.header.file : "header.html"));
            var footer = new Scaffold("/Content/partials/" + (config.footer.file != "" ? config.footer.file : "footer.html"));
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
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
            if (request.parameters.ContainsKey("nolayout"))
            {
                uselayout = false;
            }
            var scaffold = new Scaffold(relpath);
            if (scaffold.elements.Count == 0)
            {
                if (request.User.userId == 0)
                {
                    //TODO: Show user-generated 404 error
                    scaffold.HTML = "<p>This page does not exist. Please log into your account to write content for this page.</p>";
                }
                else
                {
                    //try to load template page from parent
                    scaffold.HTML = "<p>Write content using HTML & CSS</p>";
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
            var data = (Dictionary<string, string>)Serializer.ReadObject(Server.LoadFileFromCache(contentfile), typeof(Dictionary<string, string>));
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item.Value.IndexOf("\n") >= 0)
                    {
                        scaffold.Data[item.Key] = CommonMarkConverter.Convert(item.Value);
                    }
                    else
                    {
                        scaffold.Data[item.Key] = item.Value;
                    }
                }
            }

            //load platform-specific data into scaffold template
            var results = GetPlatformData(scaffold, request);
            if (results.Count > 0)
            {
                foreach (var item in results)
                {
                    scaffold.Data[item.Key] = item.Value;
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
                        header.Data[item.Key] = item.Value;
                    }
                }
                results = GetPlatformData(footer, request);
                results.AddRange(config.footer.fields);
                if (results.Count > 0)
                {
                    foreach (var item in results)
                    {
                        footer.Data[item.Key] = item.Value;
                    }
                }
                content.Data["content"] = scaffold.Render();
                return header.Render() + content.Render() + footer.Render();
            }
            else
            {
                //don't render header or footer
                return scaffold.Render();
            }
        }

        private static List<KeyValuePair<string, string>> GetPlatformData(Scaffold scaffold, Datasilk.Web.Request request)
        {
            var results = new List<KeyValuePair<string, string>>();
            if(request.User.userId > 0)
            {
                //user logged in
                if (scaffold.fields.ContainsKey("user"))
                {
                    results.Add(new KeyValuePair<string, string>("user", "1"));
                    results.Add(new KeyValuePair<string, string>("username", request.User.name));
                    results.Add(new KeyValuePair<string, string>("userid", request.User.userId.ToString()));
                }
            }
            else
            {
                //user not logged in
                if (scaffold.fields.ContainsKey("no-user"))
                {
                    results.Add(new KeyValuePair<string, string>("no-user", "1"));
                }
            }

            //finally, get platform data from the Scaffold Data Binder
            var vars = ScaffoldDataBinder.HtmlVars;
            foreach(var item in vars)
            {
                if (scaffold.fields.ContainsKey(item.Key))
                {
                    var index = results.FindAll(f => f.Key == item.Key).Count();
                    var elemIndex = scaffold.fields[item.Key][index];
                    var args = scaffold.elements[elemIndex].vars ?? new Dictionary<string, string>();
                    var argList = args.Select(a => a.Key + ":\"" + a.Value + "\"").ToArray();
                    var argsStr = "";
                    if(argList.Length > 0)
                    {
                        argsStr = string.Join(',', argList);
                    }
                    results.Add(new KeyValuePair<string, string>(item.Key, item.Value.Callback(request, argsStr)));
                }
            }

            return results;
        }
    }
}
