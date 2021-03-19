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
            switch (key)
            {
                case "special-vars":
                    return JsonResponse(new List<Models.HtmlComponentParams>()
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
                    });
                case "partial-view":
                    return JsonResponse(new List<Models.HtmlComponentParams>()
                    {
                        new Models.HtmlComponentParams()
                        {
                            Key = "page",
                            Name = "HTML File",
                            DataType = (int)Vendor.HtmlComponentParameterDataType.WebPage,
                            Description = "The relative path to your partial HTML file (e.g. \"partials/menu.html\")"
                        }
                    });
                default:
                    //vendor component
                    var component = Core.Vendors.HtmlComponents.ContainsKey(key) ? Core.Vendors.HtmlComponents[key] : null;
                    if(component != null)
                    {
                        var parameters = new List<Models.HtmlComponentParams>();
                        foreach (var param in component.Parameters)
                        {
                            parameters.Add(new Models.HtmlComponentParams()
                            {
                                Key = param.Key,
                                Name = param.Value.Name,
                                DataType = (int)param.Value.DataType,
                                List = param.Value.List,
                                DefaultValue = param.Value.DefaultValue,
                                ListOptions = param.Value.ListOptions?.Select(a => "<option value=\"" + a.Value + "\">" + a.Key.Replace("\"", "&quot;") + "</option>").ToArray(),
                                Description = param.Value.Description.Replace("\"", "&quot;"),
                                Required = param.Value.Required,
                                AddItemJs = param.Value.AddItemJs
                            });
                        }
                        return JsonResponse(parameters);
                    }
                    break;
            }
            return Error();
        }
    }
}
