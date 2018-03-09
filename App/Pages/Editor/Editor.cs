using System.Linq;
using System.Net;

namespace Saber.Pages
{
    public class Editor : Page
    {
        public Editor(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            var scaffold = new Scaffold("/Pages/Editor/editor.html");
            
            //load header interface
            LoadHeader(ref scaffold);

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


            //open page contents
            var Editor = new Services.Editor(S);
            var content = new Scaffold(rpath + rfile + ".html", S.Server.Scaffold);

            if(S.User.userId > 0)
            {
                //load editor
                if(content.HTML == "")
                {
                    content.HTML = "<p>Write content using HTML & CSS</p>";
                }
                scaffold.Data["editor"] = "1";
                scaffold.Data["editor-foot"] = "1";

                //load editor resources
                AddScript("/js/utility/ace/ace.js");
                AddScript("/js/pages/editor/editor.js");
                AddCSS("/css/pages/editor/editor.css");
                scripts += 
                    "<script language=\"javascript\">" + 
                        "S.editor.explorer.openResources('content/" + rpath.Replace("/Content/pages/", "") + 
                        "', ['" + rfile + ".html', '" + rfile + ".less', '" + rfile + ".js']" + 
                        ");" + 
                    "</script>";
                usePlatform = true;
            }
            else
            {
                if (content.HTML == "")
                {
                    content.HTML = "<p>This page does not exist. Please log into your account to write content for this page.</p>";
                }
            }

            //add page-specific references
            AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
            AddScript(rpath.ToLower() + rfile + ".js", "page_js");

            //render page content
            var html = Editor.RenderPage("content/" + pathname + ".html");
            if(html == "") { html = content.HTML; }
            scaffold.Data["content"] = html;

            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
