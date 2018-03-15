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

            if(S.User.userId > 0)
            {
                //load editor
                scaffold.Data["editor"] = "1";
                scaffold.Data["editor-foot"] = "1";

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
                
                AddScript("/js/pages/editor/editor.js");
                AddCSS("/css/pages/editor/editor.css");
                if(EditorUsed != EditorType.Monaco)
                {
                    scripts +=
                    "<script language=\"javascript\">" +
                        "S.editor.type = " + (int)EditorUsed + ";" +
                    "</script>";
                }
                usePlatform = true;
            }

            //add page-specific references
            scripts += "<script language=\"javascript\">" + 
                "window.language = '" + UserInfo.language + "';" + 
                "</script>\n";
            AddCSS(rpath.ToLower() + rfile + ".css", "page_css");
            AddScript(rpath.ToLower() + rfile + ".js", "page_js");

            //render page content
            var html = Editor.RenderPage("content/" + pathname + ".html");
            scaffold.Data["content"] = html;

            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
