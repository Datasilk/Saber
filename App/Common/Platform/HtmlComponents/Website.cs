using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.HtmlComponents
{
    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class Website : IVendorHtmlComponents
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "page-url",
                    Name = "Page URL",
                    Description = "Return the canonical URL based on the page request",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                        {
                            var results = new List<KeyValuePair<string, string>>();
                            var req = request.Context.Request;
                            var url = string.Concat(
                                req.Scheme,
                                "://",
                                req.Host.ToUriComponent(),
                                req.PathBase.ToUriComponent(),
                                req.Path.ToUriComponent(),
                                req.QueryString.ToUriComponent()
                            );
                            results.Add(new KeyValuePair<string, string>(prefix + "page-url", url));
                            return results;
                        })
                    }
                , 
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "page-id",
                    Name = "Page ID",
                    Description = "Return an ID based on the page URL",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        if(request.Path == "")
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "page-id", "home"));
                        }
                        else if (request.Path.Contains("api/") && request.Parameters.ContainsKey("path"))
                        {
                            var paths = request.Parameters["path"].Replace("content/", "").Replace(".html", "").Split("/");
                            results.Add(new KeyValuePair<string, string>(prefix + "page-id", string.Join('_', paths)));
                        }
                        else
                        {
                            var paths = request.Path.Split('/');
                            results.Add(new KeyValuePair<string, string>(prefix + "page-id", string.Join('_', paths)));
                        }
                        return results;
                    })
                },

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "year",
                    Name = "Current Year",
                    Description = "Display the current year",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "year", DateTime.Now.Year.ToString()));
                        return results;
                    })
                },

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "languages.options",
                    Name = "Language List Form",
                    Description = "Render an HTML form so your users can select their preferred language.",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var selected = request.Parameters.ContainsKey("lang") ? request.Parameters["lang"] : request.User.Language;
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "languages.options",
                            string.Join("\n", App.Languages.Select(a => "<option value=\"" + a.Key + "\"" +
                                (selected == a.Key ? " selected" : "") + ">" + a.Value + "</option>").ToArray())
                            ));
                        return results;
                    }),
                    HtmlHead = "<form id=\"changelang\" method=\"post\"><select name=\"lang\" onchange=\"changelang.submit()\">",
                    HtmlFoot = "</select></form>"
                }
            };
        }
    }
}
