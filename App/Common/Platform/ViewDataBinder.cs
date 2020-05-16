using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
                           where typeof(ViewDataBinder).IsAssignableFrom(assemblyType)
                           select assemblyType).ToArray();

            foreach (var type in binders)
            {
                var binder = (ViewDataBinder)Activator.CreateInstance(type);
                binder.Bind();
            }
        }

        public virtual void Bind() { }

        protected Dictionary<string, string> GetMethodArgs(string args)
        {
            try
            {
                return (Dictionary<string, string>)JsonConvert.DeserializeObject("{" + args + "}", typeof(Dictionary<string, string>));
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
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
                "user", "username", "userid", "no-user"
            };
            list.AddRange(HtmlVars.Select(a => a.Key));
            return list.ToArray();
        }
    }

    public class ViewDataBinderModel
    {
        /// <summary>
        /// parameters: page request, arguments, variable prefix (e.g. "header-")
        /// return: List of Key/Value pairs to be injected into template
        /// </summary>
        public Func<Request, string, string, List<KeyValuePair<string, string>>> Callback { get; set; }

        //human-readable name of html variable
        public string Name { get; set; }

        //human-readable description of html variable purpose
        public string Description { get; set; }

        //parameter list with human-readable information about each required & optional parameter
        public Dictionary<string, ScaffoldDataBinderParameter> Parameters { get; set; } = new Dictionary<string, ScaffoldDataBinderParameter>();
    }

    public class ScaffoldDataBinderParameter
    {
        public bool Required { get; set; } = false;
        public string DefaultValue { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class ScaffoldDataBinderDefaults : ViewDataBinder
    {
        public override void Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            HtmlVars.Add("user", new ViewDataBinderModel()
            {
                Name = "User Information",
                Description = "Display information about a user if logged into their account",
                Parameters = new Dictionary<string, ScaffoldDataBinderParameter>(){},
                Callback = new Func<Request, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
                {
                    var results = new List<KeyValuePair<string, string>>();
                    //check if user is logged in
                    if(request.User.userId > 0)
                    {
                        results.Add(new KeyValuePair<string, string>(prefix + "user", "True"));
                        results.Add(new KeyValuePair<string, string>(prefix + "username", request.User.name));
                        results.Add(new KeyValuePair<string, string>(prefix + "userid", request.User.userId.ToString()));
                    }
                    return results;
                })
            });

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            HtmlVars.Add("no-user", new ViewDataBinderModel()
            {
                Name = "No User",
                Description = "Display information when a user is not logged into their account",
                Parameters = new Dictionary<string, ScaffoldDataBinderParameter>() { },
                Callback = new Func<Request, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
                {
                    var results = new List<KeyValuePair<string, string>>();
                    //check if user is logged in
                    if (request.User.userId == 0)
                    {
                        results.Add(new KeyValuePair<string, string>(prefix + "no-user", "True"));
                    }
                    return results;
                })
            });

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            HtmlVars.Add("page-url", new ViewDataBinderModel()
            {
                Name = "Page URL",
                Description = "Return the canonical URL based on the page request",
                Parameters = new Dictionary<string, ScaffoldDataBinderParameter>() { },
                Callback = new Func<Datasilk.Core.Web.Request, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
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
                    results.Add(new KeyValuePair<string, string>("page-url", url));
                    return results;
                })
            });

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            HtmlVars.Add("page-id", new ViewDataBinderModel()
            {
                Name = "Page ID",
                Description = "Return an ID based on the page URL",
                Parameters = new Dictionary<string, ScaffoldDataBinderParameter>() { },
                Callback = new Func<Datasilk.Core.Web.Request, string, string, List<KeyValuePair<string, string>>>((request, data, prefix) =>
                {
                    var results = new List<KeyValuePair<string, string>>();
                    var paths = request.Path.Split('/');
                    results.Add(new KeyValuePair<string, string>("page-id", string.Join('_', paths)));
                    return results;
                })
            });
        }
    }
}
