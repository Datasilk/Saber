using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Core;

namespace Saber.Common.Platform
{
    /// <summary>
    /// Used to bind view html variables to platform data
    /// </summary>
    public class ViewDataBinder
    {
        public static Dictionary<string, ViewDataBinderModel> HtmlVars = new Dictionary<string, ViewDataBinderModel>();

        public static void Initialize()
        {
            //find all derived types of ViewDataBinder, then execute the Bind() method of each derived type
            //so that all Vendor-specific html variables will be added to the HtmlVars dictionary
            var binders = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                           from assemblyType in domainAssembly.GetTypes()
                           where typeof(Vendor.IViewDataBinder).IsAssignableFrom(assemblyType)
                           select assemblyType).ToArray();

            foreach (var type in binders)
            {
                if (type.Name.Contains("IViewDataBinder")) { continue; }
                var binder = (Vendor.IViewDataBinder)Activator.CreateInstance(type);
                binder.Bind();
            }
        }

        /// <summary>
        /// Get a list of platform-specific html variables that are used to load custom data and plugins
        /// </summary>
        /// <returns></returns>
        public static string[] GetHtmlVariables()
        {
            var list = new List<string>()
            {
                "user", "username", "userid", "no-user", "year"
            };
            list.AddRange(HtmlVars.Select(a => a.Key));
            return list.ToArray();
        }
    }

    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class ScaffoldDataBinderDefaults : Vendor.IViewDataBinder
    {
        public List<ViewDataBinderModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<ViewDataBinderModel>(){
                new ViewDataBinderModel()
                {
                    Key = "user",
                    Name = "User Information",
                    Description = "Display information about a user if logged into their account",
                    Parameters = new Dictionary<string, ViewDataBinderParameter>(){},
                    Callback = new Func<IRequest, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "user", "True"));
                            results.Add(new KeyValuePair<string, string>(prefix + "username", request.User.Name));
                            results.Add(new KeyValuePair<string, string>(prefix + "userid", request.User.UserId.ToString()));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new ViewDataBinderModel()
                {
                    Key = "no-user",
                    Name = "No User",
                    Description = "Display information when a user is not logged into their account",
                    Parameters = new Dictionary<string, ViewDataBinderParameter>() { },
                    Callback = new Func<IRequest, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
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
                , new ViewDataBinderModel()
                {
                    Key = "page-url",
                    Name = "Page URL",
                    Description = "Return the canonical URL based on the page request",
                    Parameters = new Dictionary<string, ViewDataBinderParameter>() { },
                    Callback = new Func<IRequest, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
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
                new ViewDataBinderModel()
                {
                    Key = "page-id",
                    Name = "Page ID",
                    Description = "Return an ID based on the page URL",
                    Parameters = new Dictionary<string, ViewDataBinderParameter>() { },
                    Callback = new Func<IRequest, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        var paths = request.Path.Split('/');
                        results.Add(new KeyValuePair<string, string>(prefix + "page-id", string.Join('_', paths)));
                        return results;
                    })
                },

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new ViewDataBinderModel()
                {
                    Key = "year",
                    Name = "Current Year",
                    Description = "Display the current year",
                    Parameters = new Dictionary<string, ViewDataBinderParameter>() { },
                    Callback = new Func<IRequest, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "year", DateTime.Now.Year.ToString()));
                        return results;
                    })
                },

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new ViewDataBinderModel()
                {
                    Key = "language-options",
                    Name = "Language Options",
                    Description = "Render HTML <option> elements for all supported languages",
                    Parameters = new Dictionary<string, ViewDataBinderParameter>() { },
                    Callback = new Func<IRequest, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
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
