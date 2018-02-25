using System.Linq;

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
            var file = string.Join("/", path) + ".html";
            if (path.Length > 1)
            {
                rpath += string.Join("/", path.Take(path.Length - 1)) + "/";
                rfile = path[path.Length - 1].ToLower() + ".html";
            }
            else
            {
                rfile = path[0].ToLower() + ".html";
            }
            

            //open page contents
            var content = new Scaffold(rpath + rfile, S.Server.Scaffold);

            if(S.User.userId > 0)
            {
                //load editor
                if(content.HTML == "")
                {
                    content.HTML = "<p>Write content using HTML & CSS</p>";
                }
                scaffold.Data["editor"] = "1";
                scaffold.Data["tabId"] = "content_" + file.Replace("/", "_").Replace(".", "_");
                scaffold.Data["tab-path"] = rpath + rfile;
                scaffold.Data["tab-title"] = rfile;
                scaffold.Data["tab-content"] = content.HTML;

                //load editor resources
                AddScript("/js/utility/ace/ace.js");
                AddScript("/js/pages/editor/editor.js");
                AddCSS("/css/pages/editor/editor.css");
                scripts += "<script language=\"javascript\">S.editor.explorer.open('content/" + file + "', '" + file.Replace("/", "_").Replace(".","_") + "');</script>";
            }
            else
            {
                if (content.HTML == "")
                {
                    content.HTML = "<p>This page does not exist. Please log into your account to write content for this page.</p>";
                }
                scaffold.Data["no-editor"] = "1";
                scaffold.Data["content"] = content.Render();
            }
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
