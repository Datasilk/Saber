using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saber.Common.Platform
{
    /// <summary>
    /// Used to bind scaffold html variables to platform data
    /// </summary>
    public partial class ScaffoldDataBinder
    {
        public static ScaffoldDataBinder Binder { get; set; } = new ScaffoldDataBinder();
        public Dictionary<string, Func<Datasilk.Web.Request, string, string>> HtmlVars = new Dictionary<string, Func<Datasilk.Web.Request, string, string>>();

        public ScaffoldDataBinder()
        {
            Bind();
        }

        partial void Bind();

        public Dictionary<string, string> GetMethodArgs(string args)
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
    }
}
