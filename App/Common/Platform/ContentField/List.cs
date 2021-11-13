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
            viewlist["renderapi"] = args.ContainsKey("renderapi") ? "'" + args["renderapi"] + "'" : "null";

            //get list items
            try
            {
                var html = new StringBuilder();
                if(data.IndexOf("data-src=") == 0)
                {
                    var parts = data.Split("|!|");
                    var dataSourceKey = parts[0].Split("=")[1];
                    var startPart = parts.Where(a => a.IndexOf("start=") == 0).FirstOrDefault();
                    var start = startPart != null ? int.Parse(startPart.Replace("start=", "")) : 0;
                    var lengthPart = parts.Where(a => a.IndexOf("length=") == 0).FirstOrDefault();
                    var length = lengthPart != null ? int.Parse(lengthPart.Replace("length=", "")) : 10;
                    var filterPart = parts.Where(a => a.IndexOf("filter=") == 0).FirstOrDefault();
                    var filters = JsonSerializer.Deserialize<List<DataSource.FilterGroup>>(filterPart != null ? filterPart.Replace("filter=", "") : "[]");
                    var sortPart = parts.Where(a => a.IndexOf("sort=") == 0).FirstOrDefault();
                    var sort = JsonSerializer.Deserialize<List<DataSource.OrderBy>>(sortPart != null ? sortPart.Replace("sort=", "") : "[]");
                    var datasource = Core.Vendors.DataSources.Where(a => a.Key == dataSourceKey).FirstOrDefault();
                    var locked = parts.Contains("locked");
                    var canadd = parts.Contains("add");
                    if(datasource != null)
                    {
                        //render data source filter form
                        viewlist.Show("filter");
                        viewlist["filter-contents"] = DataSource.RenderFilters(request, datasource, filters);
                        viewlist.Show(locked ? "locked" : "not-locked");
                        viewlist["datasource"] = (datasource.Helper.Vendor != "" ? datasource.Helper.Vendor + " - " : "") + datasource.Name;
                    }
                    if (!canadd) { viewlist.Show("hide-add-list-item"); }
                }
                else
                {
                    if (!string.IsNullOrEmpty(data))
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
                    }
                    
                    viewlist.Show("list-items");
                    viewlist.Show("no-datasource");
                    viewlist["list-contents"] = "<ul class=\"list\">" + html.ToString() + "</ul>";
                    viewlist.Show("not-locked");
                }
            }
            catch (Exception ex) 
            { 
            }
            return viewlist.Render();
        }
    }
}
