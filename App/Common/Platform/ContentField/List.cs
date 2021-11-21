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
            var fieldKey = args.ContainsKey("key") ? args["key"] : "";
            viewlist["title"] = key.Replace("-", " ").Replace("_", " ").Capitalize();
            viewlist["key"] = key;
            viewlist["partial"] = partials[0];
            viewlist["lang"] = lang;
            viewlist["container"] = container;
            viewlist["renderapi"] = args.ContainsKey("renderapi") ? "'" + args["renderapi"] + "'" : "null";

            //get list items
            try
            {
                var html = new StringBuilder();
                if (data.IndexOf("data-src=") >= 0)
                {
                    //use data source
                    var parts = data.Split("|!|");
                    var dataSourceKey = parts[0].Split("=")[1];
                    viewlist["lists-key"] = dataSourceKey;
                    var listsPart = parts.Where(a => a.IndexOf("lists=") == 0).FirstOrDefault();
                    var lists = listsPart != null ? listsPart.Replace("lists=", "") : "{}";
                    var settings = JsonSerializer.Deserialize<Dictionary<string, HtmlComponents.List.ListSettings>>(lists);
                    var mysettings = settings.ContainsKey(dataSourceKey) ? settings[dataSourceKey] : null;
                    var datasource = Core.Vendors.DataSources.Where(a => a.Key == dataSourceKey).FirstOrDefault();
                    var locked = parts.Contains("locked");
                    var canadd = parts.Contains("add");
                    if (!canadd) { viewlist.Show("hide-add-list-item"); }
                    if (datasource != null)
                    {
                        //render data source filter form
                        var datasourceId = dataSourceKey.Replace(datasource.Helper.Prefix + "-", "");
                        viewlist.Show("has-datasource");
                        viewlist["filter-contents"] = DataSource.RenderFilters(request, datasource, mysettings?.Filters);
                        viewlist.Show(locked ? "locked" : "not-locked");
                        viewlist["datasource"] = (datasource.Helper.Vendor != "" ? datasource.Helper.Vendor + " - " : "") + datasource.Name;
                        viewlist["orderby-contents"] = DataSource.RenderOrderByList(datasource, mysettings?.OrderBy);
                        viewlist["position-contents"] = DataSource.RenderPositionSettings(datasource, mysettings?.Position);
                        var relationships = datasource.Helper.Get(datasourceId).Relationships;
                        if(relationships.Length == 0) 
                        { 
                            viewlist.Show("no-lists");
                        }
                        else
                        {
                            viewlist["lists"] = "<option value=\"" + dataSourceKey + "\">" + key.Replace("list-", "").Replace("-", " ").Capitalize() + "</option>" + 
                                string.Join('\n', relationships.Select(a => "<option value=\"" + 
                                Core.Vendors.DataSources.Where(b => b.Key == a.ChildKey).FirstOrDefault()?.Helper.Prefix + 
                                "-" + a.Child.Key + "\">" + a.ListComponent.Replace("list-", "").Replace("-", " ").Capitalize() + "</option>"
                            ).ToArray());
                        }
                    }
                }
                else
                {
                    //use custom list items
                    viewlist.Show("no-lists");
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
                    viewlist["position-contents"] = DataSource.RenderPositionSettings(null, new DataSource.PositionSettings()
                    {
                        Start = 1,
                        StartQuery = "",
                        Length = 10,
                        LengthQuery = ""
                    });
                }
            }
            catch (Exception ex) 
            { 
            }
            return viewlist.Render();
        }
    }
}
