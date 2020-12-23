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
                var htmlVars = Common.Vendors.HtmlComponentKeys;

                //add custom component for generating Partial Views
                viewComponent["icon"] = "/editor/components/partial-view.svg";
                viewComponent["key"] = "partial-view";
                viewComponent["name"] = "Partial View";
                viewComponent["description"] = "Render a partial HTML file inside of your web page.";
                viewComponent["data-params"] = JsonSerializer.Serialize(new List<Models.HtmlComponentParams>()
                {
                    new Models.HtmlComponentParams()
                    {
                        Key = "page",
                        Name = "HTML File",
                        DataType = (int)Vendor.HtmlComponentParameterDataType.WebPage,
                        Description = "The relative path to your partial HTML file (e.g. \"partials/menu.html\")"
                    }
                }, new JsonSerializerOptions()
                {
                    WriteIndented = false
                }).Replace("\"", "&quot;");
                html.Append(viewComponent.Render());

                //add custom component for generating Special Variables
                viewComponent["icon"] = "/editor/special-vars.svg";
                viewComponent["key"] = "special-vars";
                viewComponent["name"] = "Special Variable";
                viewComponent["description"] = "Generate special variables that contain dynamic info about your website.";
                viewComponent["data-params"] = JsonSerializer.Serialize(new List<Models.HtmlComponentParams>()
                {
                    new Models.HtmlComponentParams()
                    {
                        Key = "var",
                        Name = "Select a special variable to use",
                        DataType = (int)Vendor.HtmlComponentParameterDataType.List,
                        ListOptions = Common.Vendors.SpecialVars.OrderBy(a => a.Value.Name).Select(a => ("<option value=\"" +
                                a.Value.HtmlHead.Replace("\"", "&qt;") + "{{" + a.Value.Key + "}}" + (a.Value.Block ? "{{/" + a.Value.Key + "}}" : "") + a.Value.HtmlFoot.Replace("\"", "&qt;") +
                                "\" title=\"" + a.Value.Description.Replace("\"", "&qt;") + "\">" +
                            a.Value.Name + (a.Value.Block == true ? " (block)" : "") + "</option>").Replace("\"", "&q;")).ToArray(),
                        Description = "The relative path to your partial HTML file (e.g. \"partials/menu.html\")"
                    }
                }, new JsonSerializerOptions()
                {
                    WriteIndented = false
                }).Replace("\"", "&quot;");
                html.Append(viewComponent.Render());

                foreach (var component in Common.Vendors.HtmlComponents.Values.OrderBy(a => a.Key))
                {
                    if(string.IsNullOrEmpty(component.Icon) || string.IsNullOrEmpty(component.Name)) { continue; }
                    viewComponent.Clear();
                    viewComponent["icon"] = "/editor/" + component.Icon.ToLower();
                    viewComponent["key"] = component.Key;
                    viewComponent["name"] = component.Name;
                    viewComponent["description"] = component.Description;
                    var parameters = new List<Models.HtmlComponentParams>();
                    foreach(var param in component.Parameters)
                    {
                        parameters.Add(new Models.HtmlComponentParams()
                        {
                            Key = param.Key,
                            Name = param.Value.Name,
                            DataType = (int)param.Value.DataType,
                            DefaultValue = param.Value.DefaultValue,
                            ListOptions = param.Value.ListOptions?.Select(a => "<option value=\"" + a.Key + "\">" + a.Value.Replace("\"", "&quot;") + "</option>").ToArray(),
                            Description = param.Value.Description.Replace("\"", "&quot;")
                        }); ;
                    }
                    viewComponent["data-params"] = JsonSerializer.Serialize(parameters, new JsonSerializerOptions()
                    {
                        WriteIndented = false
                    }).Replace("\"", "&quot;");
                    html.Append(viewComponent.Render());
                }
                if(html.Length > 0)
                {
                    view.Show("components");
                    view["components-list"] = html.ToString();

                }

                //add custom content field List item view
                var listitem = new View("/Views/ContentFields/list-item.html");
                listitem["title"] = "##title##";
                listitem["index"] = "##index##";
                view["custom-field-list-item"] = listitem.Render();
            }
            AddScript("/editor/js/platform.js");
            AddScript("/editor/js/editor.js");
            AddCSS("/editor/css/views/editor/editor.css");
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

            return base.Render(view.Render());
        }
    }
}
