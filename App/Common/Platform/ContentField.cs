﻿using System;
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
            var viewlist = new View("/Views/ContentFields/list.html");
            var viewitem = new View("/Views/ContentFields/list-item.html");
            var fieldKey = args.ContainsKey("key") ? args["key"] : ""; ;
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
                        viewitem["title"] = fieldKey != "" ? item[fieldKey] : "List Item #" + i;
                        viewitem["index"] = i.ToString();
                        html.Append(viewitem.Render());
                        viewitem.Clear();
                        i++;
                    }
                    viewlist["list-items"] = html.ToString();
                }
                catch (Exception) { }
            }
            viewlist["title"] = key.Replace("-", " ").Replace("_", " ").Capitalize();
            viewlist["key"] = fieldKey;
            viewlist["params"] = string.Join('|', partial.Elements.Where(a => a.Name != "" && a.Name.Substring(0, 1) != "/")
                .Select(a => a.Name + "," + (a.isBlock ? '1' : '0')));
            return viewlist.Render();
        }
    }
}
