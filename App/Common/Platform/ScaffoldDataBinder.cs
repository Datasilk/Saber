using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Saber.Common.Platform
{
    /// <summary>
    /// Used to bind scaffold html variables to platform data
    /// </summary>
    public partial class ScaffoldDataBinder
    {
        public static Dictionary<string, ScaffoldDataBinderModel> HtmlVars = new Dictionary<string, ScaffoldDataBinderModel>();
        
        partial void Bind();
        
        public static void Initialize()
        {
            var binder = new ScaffoldDataBinder();
            binder.Bind();
        }

        private Dictionary<string, string> GetMethodArgs(string args)
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

    public class ScaffoldDataBinderModel
    {
        public Func<Datasilk.Web.Request, string, string> Callback { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public Dictionary<string, ScaffoldDataBinderParameter> Parameters { get; set; } = new Dictionary<string, ScaffoldDataBinderParameter>();
    }

    public class ScaffoldDataBinderParameter
    {
        public bool Required { get; set; } = false;
        public string DefaultValue { get; set; }
        public string Description { get; set; }
    }
}
