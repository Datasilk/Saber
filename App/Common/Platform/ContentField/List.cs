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
                if(data.IndexOf("data-src=") >= 0)
                {
                    var parts = data.Split("|!|");
                    var dataSourceKey = parts[0].Split("=")[1];
                    var startPart = parts.Where(a => a.IndexOf("start=") == 0).FirstOrDefault();
                    var startParts = startPart != null ? startPart.Replace("start=", "").Split("|") : new string[] { };
                    var start = startPart != null ? int.Parse(startParts[0]) : 0;
                    var startQuery = startPart != null ? (startParts.Length > 1 ? startParts[1] : "") : "";
                    var lengthPart = parts.Where(a => a.IndexOf("length=") == 0).FirstOrDefault();
                    var lengthParts = lengthPart != null ? lengthPart.Replace("length=", "").Split("|") : new string[] { };
                    var length = lengthPart != null ? int.Parse(lengthParts[0]) : 10;
                    var lengthQuery = lengthPart != null ? (lengthParts.Length > 1 ? lengthParts[1] : "") : "";
                    var filterPart = parts.Where(a => a.IndexOf("filter=") == 0).FirstOrDefault();
                    var filters = JsonSerializer.Deserialize<List<DataSource.FilterGroup>>(filterPart != null ? filterPart.Replace("filter=", "") : "[]");
                    var sortPart = parts.Where(a => a.IndexOf("sort=") == 0).FirstOrDefault();
                    var orderby = JsonSerializer.Deserialize<List<DataSource.OrderBy>>(sortPart != null ? sortPart.Replace("sort=", "") : "[]");
                    var datasource = Core.Vendors.DataSources.Where(a => a.Key == dataSourceKey).FirstOrDefault();
                    var locked = parts.Contains("locked");
                    var canadd = parts.Contains("add");
                    if (!canadd) { viewlist.Show("hide-add-list-item"); }
                    if (datasource != null)
                    {
                        //render data source filter form
                        viewlist.Show("has-datasource");
                        viewlist["filter-contents"] = DataSource.RenderFilters(request, datasource, filters);
                        viewlist.Show(locked ? "locked" : "not-locked");
                        viewlist["datasource"] = (datasource.Helper.Vendor != "" ? datasource.Helper.Vendor + " - " : "") + datasource.Name;
                        viewlist["orderby-contents"] = DataSource.RenderOrderByList(datasource, orderby);
                        viewlist["position-contents"] = DataSource.RenderPositionSettings(datasource, new DataSource.PositionSettings()
                        {
                            Start = start,
                            StartQuery = startQuery,
                            Length = length,
                            LengthQuery = lengthQuery
                        });
                    }
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
                        viewlist["item-count"] = items.Count.ToString();
                    }
                    else
                    {
                        viewlist["item-count"] = "0";
                    }
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
