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
                    if(data.IndexOf("data-src=") == 0)
                    {
                        var parts = data.Split("|!|");
                        var datasrc = parts[0].Replace("data-src=", "");
                        var locked = parts.Contains("locked");
                        var canadd = parts.Contains("add");
                        var filterpart = parts.Where(a => a.IndexOf("filter=") == 0).FirstOrDefault();
                        var filter = new Dictionary<string, string>();
                        if(filterpart != null)
                        {
                            filter = filterpart.Replace("filter=", "").Split("|").ToDictionary
                                (a => a.Split("=", 2)[0], a => a.Split("=", 2)[1]);
                        }
                        var datasource = Core.Vendors.DataSources.Where(a => a.Key == datasrc).FirstOrDefault();
                        if(datasource != null)
                        {
                            //render data source filter form
                            datasrc = datasrc.Replace(datasource.Helper.Prefix + "-", "");
                            var filterform = datasource.Helper.RenderFilters(request, datasrc, filter);
                            viewlist.Show("filter");
                            viewlist["filter-contents"] = filterform.HTML;
                            viewlist["filter-oninit"] = "data-init=\"" + filterform.OnInit + "\"";
                            viewlist.Show(locked ? "locked" : "not-locked");
                            viewlist["datasource"] = datasource.Name;
                        }
                        if (!canadd) { viewlist.Show("hide-add-list-item"); }
                    }
                    else
                    {
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
                        viewlist.Show("list-items");
                        viewlist["list-title"] = "List Items";
                        viewlist["list-contents"] = "<ul class=\"list\">" + html.ToString() + "</ul>";
                    }
                }
                catch (Exception ex) 
                { 
                }
            }
            return viewlist.Render();
        }
    }
}
