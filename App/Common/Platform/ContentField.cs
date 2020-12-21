using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saber.Core.Extensions.Strings;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.ContentField
{
    [ContentField("-")]
    [ReplaceRow]
    public class LineBreak : IVendorContentField
    {
        public string Render(IRequest request, Dictionary<string, string> args, string data, string id, string prefix, string key)
        {
            return (args != null && args.ContainsKey("title") ? "<h4>" + args["title"] + "</h4>" : "") + "<hr/>";
        }
    }

    [ContentField("list")]
    public class List : IVendorContentField
    {
        public string Render(IRequest request, Dictionary<string, string> args, string data, string id, string prefix, string key)
        {
            if (!args.ContainsKey("partial")) { return "You must provide the \"partial\" property for your mustache \"list\" component"; }
            //load provided partial view
            var partial = new View("/Content/" + args["partial"]);
            var html = new StringBuilder();
            var listitems = new View("/Views/ContentFields/list.html");
            listitems["list-items"] = html.ToString();
            listitems["title"] = key.Replace("-", " ").Replace("_", " ").Capitalize();
            listitems["key"] = args.ContainsKey("key") ? args["key"] : "";
            listitems["params"] = string.Join('|', partial.Elements.Where(a => a.Name != "" && a.Name.Substring(0, 1) != "/")
                .Select(a => a.Name + "," + (a.isBlock ? '1' : '0')));
            return listitems.Render();
        }
    }
}
