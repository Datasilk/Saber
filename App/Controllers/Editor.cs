using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using Datasilk.Core.Web;
using Saber.Core;

namespace Saber.Controllers
{
    public class Editor : Controller
    {
        public override string Render(string body = "")
        {
            //get selected language
            var lang = User.Language ?? "en";

            //get relative paths
            var pathname = Parameters.ContainsKey("path") ? Parameters["path"] : "home";

            //load page configuration
            var config = PageInfo.GetPageConfig("content/" + pathname);
            var webconfig = Common.Platform.Website.Settings.Load();


            //use editor.html
            var view = new View("/Views/Editor/editor.html");

            //load editor resources
            if (CheckSecurity("code-editor"))
            {
                view.Show("code-editor");
                view.Show("manage-datasources");
                view.Show("data-sources");
                view.Show("file-browser");

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

                //load components list
                var viewComponent = new View("/Views/Components/list-item.html");
                var html = new StringBuilder();
                var htmlVars = Core.Vendors.HtmlComponentKeys;

                //add custom component for generating Partial Views
                viewComponent["icon"] = "/editor/components/partial-view.svg";
                viewComponent["key"] = "partial-view";
                viewComponent["name"] = "Partial View";
                viewComponent["description"] = "Render a partial HTML file inside of your web page.";
                html.Append(viewComponent.Render());

                //add custom component for generating Special Variables
                viewComponent["icon"] = "/editor/special-vars.svg";
                viewComponent["key"] = "special-vars";
                viewComponent["name"] = "Special Variable";
                viewComponent["description"] = "Generate special variables that contain dynamic info about your website.";
                html.Append(viewComponent.Render());

                foreach (var component in Core.Vendors.HtmlComponents.Values.OrderBy(a => a.Key))
                {
                    if(string.IsNullOrEmpty(component.Icon) || string.IsNullOrEmpty(component.Name)) { continue; }
                    viewComponent.Clear();
                    viewComponent["icon"] = "/editor/" + component.Icon.ToLower();
                    viewComponent["key"] = component.Key;
                    viewComponent["name"] = component.Name;
                    viewComponent["description"] = component.Description;

                    html.Append(viewComponent.Render());
                }
                if(html.Length > 0)
                {
                    view.Show("components");
                    view["components-list"] = html.ToString();

                }

                //add custom content field List item partial view as a text/html script 
                var listitem = new View("/Views/ContentFields/list-item.html");
                listitem["label"] = "##label##";
                listitem["index"] = "##index##";
                listitem["onclick"] = "##onclick##";
                view["custom-field-list-item"] = listitem.Render();
            }
            AddScript("/editor/js/platform.js");
            AddScript("/editor/js/editor.js");
            AddScript("/editor/js/vendors-editor.js");
            AddCSS("/editor/css/views/editor/editor.css");
            AddCSS("/editor/css/vendors-editor.css");
            if (EditorUsed != EditorType.Monaco)
            {
                Scripts.Append(
                "<script language=\"javascript\">" +
                    "S.editor.type = " + (int)EditorUsed + ";" +
                "</script>");
            }

            if (CheckSecurity("code-editor"))
            {
                Scripts.Append(
            "<script language=\"javascript\">" +
                "S.editor.useCodeEditor = true;" +
                "S.editor.components.load();" +
            "</script>");
            }
                    

            //check security permissions in order to show certain features
            var websiteSecurity = false;
            if (CheckSecurity("upload"))
            {
                view.Show("page-resources");
            }
            if (CheckSecurity("edit-content"))
            {
                view.Show("page-content");
            }
            if (CheckSecurity("page-settings"))
            {
                view.Show("page-settings");
            }
            if (CheckSecurity("website-analytics"))
            {
                view.Show("website-analytics");
                websiteSecurity = true;
            }
            if (CheckSecurity("website-settings"))
            {
                view.Show("website-settings");
                websiteSecurity = true;
            }
            if (CheckSecurity("manage-users"))
            {
                view.Show("manage-users");
                websiteSecurity = true;
            }
            if (CheckSecurity("manage-security"))
            {
                view.Show("manage-security");
                websiteSecurity = true;
            }
            if (websiteSecurity)
            {
                view.Show("website-management");
            }

            //add page-specific references
            Scripts.Append(
                "<script language=\"javascript\">" +
                    "window.language = '" + User.Language + "';" +
                    "S.editor.init();" +
                "</script>\n"
            );

            //allow CORS wildcard
            Context.Response.Headers.Add(
                    new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("Access-Control-Allow-Origin", "*"));

            return base.Render(view.Render());
        }
    }
}
