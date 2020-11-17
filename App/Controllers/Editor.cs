using System.IO;
using System.Text;
using System.Linq;
using Datasilk.Core.Web;
using Saber.Core;
using Saber.Core.Extensions;

namespace Saber.Controllers
{
    public class Editor : Controller
    {
        public override string Render(string body = "")
        {
            Theme = "dark";
            View view;

            //get selected language
            var lang = "en";
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

            if (uselayout)
            {
                var rpath = "/Content/pages/";
                var rfile = "";
                if (PathParts.Length > 1)
                {
                    rpath += string.Join("/", PathParts.Take(PathParts.Length - 1)) + "/";
                    rfile = PathParts[PathParts.Length - 1].ToLower();
                }
                else if(PathParts.Length > 0)
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

                if (User.UserId > 0 && !Parameters.ContainsKey("live"))
                {
                    //use editor.html
                    view = new View("/Views/Editor/editor.html");

                    //load editor resources
                    switch (EditorUsed)
                    {
                        case EditorType.Monaco:
                            AddCSS("/editor/js/utility/monaco/min/vs/editor/editor.main.css");
                            AddScript("/editor/js/utility/monaco/min/vs/loader.js");
                            view["editor-type"] = "monaco";
                            break;

                        case EditorType.Ace:
                            AddScript("/editor/js/utility/ace/ace.js");
                            view["editor-type"] = "ace";
                            break;
                    }

                    AddScript("/editor/js/editor.js");
                    AddCSS("/editor/css/views/editor/editor.css");
                    if (EditorUsed != EditorType.Monaco)
                    {
                        Scripts.Append(
                        "<script language=\"javascript\">" +
                            "S.editor.type = " + (int)EditorUsed + ";" +
                        "</script>");
                    }
                    UsePlatform = true;
                }
                else
                {
                    //use no-editor.html
                    view = new View("/Views/Editor/no-editor.html");
                }

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

                //add all custom scripts before loading page script
                var scriptIndex = 1;
                foreach (var script in config.scripts)
                {
                    AddScript(script, "custom_js_" + scriptIndex);
                    scriptIndex++;
                }

                if (File.Exists(App.MapPath(rpath + rfile + ".html")))
                {
                    //page exists
                    view["content"] = Common.Platform.Render.Page("content/" + pathname + ".html", this, config, lang);
                    AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
                    AddScript(rpath.ToLower() + rfile + ".js", "page_js");
                }
                else if(User.UserId == 0 || Parameters.ContainsKey("live"))
                {
                    //show 404 error
                    Context.Response.StatusCode = 404;
                    if (File.Exists(App.MapPath("content/pages/404.html")))
                    {
                        config = PageInfo.GetPageConfig("content/404");
                        view["content"] = Common.Platform.Render.Page("content/pages/404.html", this, config, lang);
                        AddCSS("/content/pages/404.css", "page_css");
                        AddScript("/content/pages/404.js", "page_js");
                    }
                    else
                    {
                        view["content"] = Common.Platform.Render.Page("Views/404.html", this, config, lang);
                    }
                }
                else if (File.Exists(App.MapPath(rpath + "/template.html")))
                {
                    //page does not exist, try to load template page from parent
                    var templatePath = string.Join('/', PathParts.Take(PathParts.Length - 1).ToArray());
                    view["content"] = Common.Platform.Render.Page("content/" + templatePath + "/template.html", this, config, lang);
                    AddCSS(rpath.ToLower() + "template.css", "page_css");
                    AddScript(rpath.ToLower() + "template.js", "page_js");
                }
                else
                {
                    //last resort, page & template doesn't exists
                    view["content"] = Common.Platform.Render.Page("content/" + pathname + ".html", this, config, lang);
                    AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
                    AddScript(rpath.ToLower() + rfile + ".js", "page_js");
                }

                //log page request
                var url = string.Join("/", PathParts) + (Context.Request.QueryString.HasValue ? "?" + Context.Request.QueryString.Value : "");
                Query.Logs.LogUrl(url, Context.Connection.RemoteIpAddress.ToInt());

                return base.Render(view.Render());
            }
            else
            {
                //don't load layout, which includes CSS & Javascript files
                return "<span style=\"display:none;\"></span>\n" + //CORS fix: add empty span at top of page to trick CORB validation
                    Common.Platform.Render.Page("content/" + pathname + ".html", this, config, lang);
            }
        }
    }
}
