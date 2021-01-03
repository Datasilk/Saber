using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Saber.Core.Extensions.Strings;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.ContentField
{
    [ContentField("-")]
    [ReplaceRow]
    public class LineBreak : IVendorContentField
    {
        public string Render(IRequest request, Dictionary<string, string> args, string data, string id, string prefix, string key, string lang, string container)
        {
            return (args != null && args.ContainsKey("title") ? "<h4>" + args["title"] + "</h4>" : "") + "<hr/>";
        }
    }

    [ContentField("list")]
    public class List : IVendorContentField
    {
        public string Render(IRequest request, Dictionary<string, string> args, string data, string id, string prefix, string key, string lang, string container)
        {
            if (!args.ContainsKey("partial")) { return "You must provide the \"partial\" property for your mustache \"list\" component"; }
            //load provided partial view
            var partials = args["partial"].Split("|");
            var viewlist = new View("/Views/ContentFields/list.html");
            var viewitem = new View("/Views/ContentFields/list-item.html");
            var fieldKey = args.ContainsKey("key") ? args["key"] : ""; ;
            viewlist["title"] = key.Replace("-", " ").Replace("_", " ").Capitalize();
            viewlist["key"] = fieldKey;
            viewlist["partial"] = partials[0];
            viewlist["lang"] = lang;
            viewlist["container"] = container;

            //get list items
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var html = new StringBuilder();
                    var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(data);
                    var i = 1;
                    foreach (var item in items)
                    {
                        viewitem["label"] = fieldKey != "" ? item[fieldKey] : "List Item #" + i;
                        viewitem["index"] = i.ToString();
                        viewitem["onclick"] = "S.editor.fields.custom.list.edit(event, '" + viewlist["title"] +
                            "', '" + viewlist["key"] +
                            "', '" + viewlist["partial"] + "', '" + lang + "', '" + container + "')";
                        html.Append(viewitem.Render());
                        viewitem.Clear();
                        i++;
                    }
                    viewlist["list-items"] = html.ToString();
                }
                catch (Exception) { }
            }
            return viewlist.Render();
        }
    }
}
