using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Core;

namespace Saber.Common.Platform
{
    /// <summary>
    /// Used to bind HTML mustache variables to HTML components
    /// </summary>
    public class HtmlComponentBinder
    {
        public static List<HtmlComponentModel> HtmlVars = new List<HtmlComponentModel>();
        private static string[] _htmlVarKeys { get; set; }

        public static void Initialize()
        {
            foreach (var type in Vendors.HtmlComponents)
            {
                if (type.Name.Contains("IVendorHtmlComponent")) { continue; }
                var binder = (Vendor.IVendorHtmlComponent)Activator.CreateInstance(type);
                HtmlVars.AddRange(binder.Bind());
            }
        }

        /// <summary>
        /// Get a list of platform-specific html variables that are used to load custom data and plugins
        /// </summary>
        /// <returns></returns>
        public static string[] GetHtmlVariables()
        {
            if(_htmlVarKeys != null) { return _htmlVarKeys; }
            var list = new List<string>();
            list.AddRange(HtmlVars.Select(a => a.Key));
            _htmlVarKeys = list.ToArray();
            return _htmlVarKeys;
        }
    }

    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class ViewDataBinderDefaults : Vendor.IVendorHtmlComponent
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
                new HtmlComponentModel()
                {
                    Key = "user",
                    Name = "User Information",
                    Description = "Display information about a user if logged into their account",
                    Parameters = new Dictionary<string, HtmlComponentParameter>(){},
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
                    Parameters = new Dictionary<string, HtmlComponentParameter>(){},
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
                    Parameters = new Dictionary<string, HtmlComponentParameter>(){},
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
                    Name = "No User",
                    Description = "Display information when a user is not logged into their account",
                    Parameters = new Dictionary<string, HtmlComponentParameter>() { },
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
                    Parameters = new Dictionary<string, HtmlComponentParameter>() { },
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
                    Parameters = new Dictionary<string, HtmlComponentParameter>() { },
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
                    Parameters = new Dictionary<string, HtmlComponentParameter>() { },
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
                    Key = "language-options",
                    Name = "Language Options",
                    Description = "Render HTML <option> elements for all supported languages",
                    Parameters = new Dictionary<string, HtmlComponentParameter>() { },
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var selected = request.Parameters.ContainsKey("lang") ? request.Parameters["lang"] : request.User.Language;
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "language-options",
                            "<option value=\"en\">English</option>" +
                            string.Join("\n", Query.Languages.GetList().Select(a => "<option value=\"" + a.langId + "\"" +
                                (selected == a.langId ? " selected" : "") + ">" + a.language + "</option>").ToArray())
                            ));
                        return results;
                    })
                }
            };
        }
    }
}
