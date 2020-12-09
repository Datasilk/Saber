using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
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
                    UsePlatform = true;

                    //use editor.html
                    view = new View("/Views/Editor/editor.html");

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
                        viewComponent["icon"] = "/editor/partial-view.svg";
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
                                ListOptions = Common.Vendors.SpecialVars.OrderBy(a => a.Key).Select(a => ("<option value=\"" +
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
                            viewComponent["icon"] = "/editor/images/" + component.Icon.ToLower();
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
                try
                {
                    if (File.Exists(App.MapPath(rpath + rfile + ".html")))
                    {
                        //page exists
                        view["content"] = Common.Platform.Render.Page("content/" + pathname + ".html", this, config, lang);
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
                            view["content"] = Common.Platform.Render.Page("content/pages/404.html", this, config, lang);
                            AddCSS("/content/pages/404.css", "page_css");
                            AddScript("/content/pages/404.js", "page_js");
                        }
                        else
                        {
                            view["content"] = Common.Platform.Render.Page("content/pages/404.html", this, config, lang);
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
                }
                catch (ServiceDeniedException)
                {
                    view["content"] = Common.Platform.Render.Page("content/access-denied.html", this, config, lang);
                }
                catch (ServiceErrorException)
                {
                    view["content"] = Common.Platform.Render.Page("content/error.html", this, config, lang);
                }
                

                //log page request
                var url = string.Join("/", PathParts) + (Context.Request.QueryString.HasValue ? "?" + Context.Request.QueryString.Value : "");
                Query.Logs.LogUrl(url, Context.Connection.RemoteIpAddress.ToInt());

                return base.Render(view.Render());
            }
            else
            {
                //don't load layout, which includes CSS & Javascript files
                try
                {
                    return "<span style=\"display:none;\"></span>\n" + //CORS fix: add empty span at top of page to trick CORB validation
                        Common.Platform.Render.Page("content/" + pathname + ".html", this, config, lang);
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
