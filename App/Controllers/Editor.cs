using System.Linq;
using Microsoft.AspNetCore.Http;
using Saber.Common;

namespace Saber.Pages
{
    public class Editor : Page
    {
        public Editor(HttpContext context) : base(context)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            theme = "dark";
            Scaffold scaffold;

            //get relative paths
            var rpath = "/Content/pages/";
            var rfile = "";
            var pathname = string.Join("/", path);
            if (path.Length > 1)
            {
                rpath += string.Join("/", path.Take(path.Length - 1)) + "/";
                rfile = path[path.Length - 1].ToLower();
            }
            else
            {
                rfile = path[0].ToLower();
            }
            if (pathname == "") { pathname = "home"; rfile = pathname; }
            var file = pathname + ".html";

            //load page configuration
            var config = PageInfo.GetPageConfig("content/" + pathname);
            title = config.title.prefix + config.title.body + config.title.suffix;
            description = config.description;

            //open page contents
            if (User.userId > 0)
            {
                //use editor.html
                scaffold = new Scaffold("/Views/Editor/editor.html");

                //load editor resources
                switch (EditorUsed)
                {
                    case EditorType.Monaco:
                        AddScript("/js/utility/monaco/loader.js");
                        scaffold.Data["editor-type"] = "monaco";
                        break;

                    case EditorType.Ace:
                        AddScript("/js/utility/ace/ace.js");
                        scaffold.Data["editor-type"] = "ace";
                        break;
                }
                
                AddScript("/js/views/editor/editor.js");
                AddCSS("/css/views/editor/editor.css");
                if(EditorUsed != EditorType.Monaco)
                {
                    scripts.Append(
                    "<script language=\"javascript\">" +
                        "S.editor.type = " + (int)EditorUsed + ";" +
                    "</script>");
                }
                usePlatform = true;
            }
            else
            {
                //use no-editor.html
                scaffold = new Scaffold("/Views/Editor/no-editor.html");
            }
            
            //load header interface
            LoadHeader(ref scaffold);

            //add page-specific references
            scripts.Append(
                "<script language=\"javascript\">" + 
                    "window.language = '" + User.language + "';" + 
                "</script>\n"
            );
            AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
            AddScript(rpath.ToLower() + rfile + ".js", "page_js");

            //render page content
            var html = Platform.RenderPage("content/" + pathname + ".html", this, User);
            scaffold.Data["content"] = html;

            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
