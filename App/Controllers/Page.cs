using System.Linq;
using System.Text;
using System.IO;
using Datasilk.Core.Web;
using Saber.Core;
using Saber.Core.Extensions;

namespace Saber.Controllers
{
    public class Page : Controller
    {
        public override string Render(string body = "")
        {
            if (!Server.HasAdmin) { return Redirect("/login"); }

            //get selected language
            var lang = User.Language ?? "en";
            if (Parameters.ContainsKey("lang"))
            {
                lang = Parameters["lang"];
                User.Language = lang;
                User.Save(true);
            }

            //get relative paths
            var pathname = string.Join("/", PathParts);
            if (pathname == "")
            {
                pathname = "home";
            }

            //load page configuration
            var uselayout = true;
            if (Parameters.ContainsKey("nolayout"))
            {
                uselayout = false;
            }
            var config = PageInfo.GetPageConfig("content/" + pathname);
            var webconfig = Common.Platform.Website.Settings.Load();

            if (uselayout)
            {
                var html = new StringBuilder();
                var rpath = "/Content/pages/";
                var rfile = "";
                if (PathParts.Length > 1)
                {
                    rpath += string.Join("/", PathParts.Take(PathParts.Length - 1)) + "/";
                    rfile = PathParts[PathParts.Length - 1].ToLower();
                }
                else if (PathParts.Length > 0)
                {
                    rfile = PathParts[0].ToLower();
                }
                if (pathname == "home")
                {
                    rfile = pathname;
                }

                //load page layout
                Title = config.title.prefix + config.title.body + config.title.suffix;
                Description = config.description;
                UsePlatform = false;

                if (User.UserId >= 1 && !Parameters.ContainsKey("live"))
                {
                    var view = new View("/Views/Editor/editor-iframe.html");
                    view["path"] = pathname;
                    view["preload"] = "0";
                    html.Append(view.Render());
                }
                html.Append("<div class=\"website\">");

                //add page-specific references
                Scripts.Append(
                    "<script language=\"javascript\">" +
                        "window.language = '" + User.Language + "';" +
                    "</script>\n"
                );
                if (Parameters.ContainsKey("live"))
                {
                    Footer = new StringBuilder();
                    Footer.Append(Cache.LoadFile("/Views/Editor/live-preview-min.html"));
                }

                //add all custom website styles
                var styleIndex = 1;
                foreach (var style in webconfig.Stylesheets)
                {
                    AddCSS(style, "custom_css_" + styleIndex);
                    styleIndex++;
                }

                //add website css
                AddCSS("/css/website.css", "website_css");

                //add all custom page styles before loading page style
                foreach (var style in config.stylesheets)
                {
                    AddCSS(style, "custom_css_" + styleIndex);
                    styleIndex++;
                }

                //add all custom website scripts
                var scriptIndex = 1;
                foreach (var script in webconfig.Scripts)
                {
                    AddScript(script, "custom_js_" + scriptIndex);
                    scriptIndex++;
                }

                //add website.js after custom website scripts
                AddScript("/js/website.js", "website_js");

                //add all custom page scripts before loading page script
                foreach (var script in config.scripts)
                {
                    AddScript(script, "custom_js_" + scriptIndex);
                    scriptIndex++;
                }
                try
                {
                    if (File.Exists(App.MapPath(rpath + rfile + ".html")))
                    {
                        //page exists
                        html.Append(Common.Platform.Render.Page("content/pages/" + pathname + ".html", this, config, lang));
                        AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
                        AddScript(rpath.ToLower() + rfile + ".js", "page_js");
                    }
                    else if (User.UserId == 0 || Parameters.ContainsKey("live"))
                    {
                        //show 404 error
                        Context.Response.StatusCode = 404;
                        if (File.Exists(App.MapPath("content/pages/404.html")))
                        {
                            config = PageInfo.GetPageConfig("content/404");
                            html.Append(Common.Platform.Render.Page("content/pages/404.html", this, config, lang));
                            AddCSS("/content/pages/404.css", "page_css");
                            AddScript("/content/pages/404.js", "page_js");
                        }
                        else
                        {
                            html.Append(Common.Platform.Render.Page("content/pages/404.html", this, config, lang));
                        }
                    }
                    else if (File.Exists(App.MapPath(rpath + "/template.html")))
                    {
                        //page does not exist, try to load template page from parent
                        var templatePath = string.Join('/', PathParts.Take(PathParts.Length - 1).ToArray());
                        html.Append(Common.Platform.Render.Page("content/pages/" + templatePath + "/template.html", this, config, lang));
                        AddCSS(rpath.ToLower() + "template.css", "page_css");
                        AddScript(rpath.ToLower() + "template.js", "page_js");
                    }
                    else
                    {
                        //last resort, page & template doesn't exists
                        html.Append(Common.Platform.Render.Page("content/pages/" + pathname + ".html", this, config, lang));
                        AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
                        AddScript(rpath.ToLower() + rfile + ".js", "page_js");
                    }
                }
                catch (ServiceDeniedException)
                {
                    html.Append(Common.Platform.Render.Page("content/access-denied.html", this, config, lang));
                }
                catch (ServiceErrorException)
                {
                    html.Append(Common.Platform.Render.Page("content/error.html", this, config, lang));
                }

                html.Append("</div>");

                if (User.UserId >= 1 && !Parameters.ContainsKey("live"))
                {
                    AddCSS("/editor/css/iframe.css");
                    AddScript("/editor/js/iframe.js");
                }

                //log page request
                var url = string.Join("/", PathParts) + (Context.Request.QueryString.HasValue ? "?" + Context.Request.QueryString.Value : "");
                Query.Logs.LogUrl(url, Context.Connection.RemoteIpAddress.ToInt());

                return base.Render(html.ToString());
            }
            else
            {
                //don't load layout, which includes CSS & Javascript files
                try
                {
                    return "<span style=\"display:none;\"></span>\n" + //CORS fix: add empty span at top of page to trick CORB validation
                        Common.Platform.Render.Page("content/pages/" + pathname + ".html", this, config, lang);
                }
                catch (ServiceDeniedException)
                {
                    return "<span style=\"display:none;\"></span>\n" + //CORS fix: add empty span at top of page to trick CORB validation
                        Common.Platform.Render.Page("content/access-denied.html", this, config, lang);
                }
                catch (ServiceErrorException)
                {
                    return "<span style=\"display:none;\"></span>\n" + //CORS fix: add empty span at top of page to trick CORB validation
                        Common.Platform.Render.Page("content/error.html", this, config, lang);
                }
            }
        }
    }
}
