using System;
using System.Collections.Generic;
using System.Linq;

namespace Saber.Services
{
    public class Components : Service
    {
        public string GetParameters(string key)
        {
            if(!CheckSecurity("code-editor") || User.PublicApi) { return AccessDenied(); }
            var parameters = new List<Models.HtmlComponentParams>();
            switch (key)
            {
                case "special-vars":
                    parameters = new List<Models.HtmlComponentParams>()
                    {
                        new Models.HtmlComponentParams()
                        {
                            Key = "var",
                            Name = "Select a special variable to use",
                            DataType = (int)Vendor.HtmlComponentParameterDataType.List,
                            ListOptions = Core.Vendors.SpecialVars.OrderBy(a => a.Value.Name).Select(a => ("<option value=\"" +
                                    a.Value.HtmlHead.Replace("\"", "&qt;") + "{{" + a.Value.Key + "}}" + (a.Value.Block ? "{{/" + a.Value.Key + "}}" : "") + a.Value.HtmlFoot.Replace("\"", "&qt;") +
                                    "\" title=\"" + a.Value.Description.Replace("\"", "&qt;") + "\">" +
                                a.Value.Name + (a.Value.Block == true ? " (block)" : "") + "</option>").Replace("\"", "&q;")).ToArray(),
                            Description = "The relative path to your partial HTML file (e.g. \"partials/menu.html\")"
                        }
                    };
                    break;
                case "partial-view":
                    parameters = new List<Models.HtmlComponentParams>()
                    {
                        new Models.HtmlComponentParams()
                        {
                            Key = "page",
                            Name = "HTML File",
                            DataType = (int)Vendor.HtmlComponentParameterDataType.WebPage,
                            Description = "The relative path to your partial HTML file (e.g. \"partials/menu.html\")"
                        }
                    };
                    break;
                default:
                    //vendor component
                    var component = Core.Vendors.HtmlComponents.ContainsKey(key) ? Core.Vendors.HtmlComponents[key] : null;
                    if(component != null)
                    {
                        foreach (var param in component.Parameters)
                        {
                            parameters.Add(new Models.HtmlComponentParams()
                            {
                                Key = param.Key,
                                Name = param.Value.Name,
                                DataType = (int)param.Value.DataType,
                                List = param.Value.List,
                                DefaultValue = param.Value.DefaultValue,
                                ListOptions = param.Value.ListOptions != null ? param.Value.ListOptions().Select(a => "<option value=\"" + a.Value + "\">" + a.Key.Replace("\"", "&quot;") + "</option>").ToArray() : new string[] { },
                                Description = param.Value.Description.Replace("\"", "&quot;"),
                                Required = param.Value.Required,
                                AddItemJs = param.Value.AddItemJs
                            });
                        }
                        return JsonResponse(new
                        {
                            description = component.Description,
                            block = component.Block,
                            noId = component.NoID,
                            website = component.Website,
                            parameters
                        });
                    }
                    break;
            }
            return JsonResponse(new
            {
                description = "",
                block = false,
                noId = false,
                website = "",
                parameters
            });
        }
    }
}
