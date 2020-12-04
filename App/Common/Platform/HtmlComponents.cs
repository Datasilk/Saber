using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform
{
    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class HtmlComponents : IVendorHtmlComponent
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
                new HtmlComponentModel()
                {
                    Key = "user",
                    Name = "User Logged In",
                    Block = true,
                    Description = "Display a block of HTML if the user is logged into their account",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "user", "True"));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "username",
                    Name = "User Name",
                    Description = "Display the user's name",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "username", request.User.Name));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "userid",
                    Name = "User ID",
                    Description = "Display the user's ID",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "userid", request.User.UserId.ToString()));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "no-user",
                    Name = "User Not Logged In",
                    Block = true,
                    Description = "Display a block of HTML when the user is not logged into their account",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if (request.User.UserId == 0)
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "no-user", "True"));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
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
                        var paths = request.Path.Split('/');
                        results.Add(new KeyValuePair<string, string>(prefix + "page-id", string.Join('_', paths)));
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
                            "<option value=\"en\">English</option>" +
                            string.Join("\n", Query.Languages.GetList().Select(a => "<option value=\"" + a.langId + "\"" +
                                (selected == a.langId ? " selected" : "") + ">" + a.language + "</option>").ToArray())
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
