using System.Text;
using System.Text.Json;
using Saber.Core.Extensions.Strings;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.ContentField
{

    [ContentField("list")]
    public class List : IVendorContentField
    {
        public string Render(IRequest request, Dictionary<string, string> args, string data, string id, string prefix, string key, string lang, string container)
        {
            if (!args.ContainsKey("partial")) { return "You must provide the \"partial\" property for your mustache \"list\" component"; }
            //load provided partial view
            var partials = (args["partial"].Contains("|") ? args["partial"].Split("|") : 
                args["partial"].Split(",")).Select(a => a.Trim()).ToArray();
            var viewlist = new View("/Views/ContentFields/list.html");
            var viewitem = new View("/Views/ContentFields/list-item.html");
            var fieldKey = args.ContainsKey("key") ? args["key"] : "";
            var listItemsClick = args.ContainsKey("list-click") ? args["list-click"] :
                "S.editor.fields.custom.list.tab('list-items', event);";

            //bind view data
            viewlist["title"] = key.Replace("-", " ").Replace("_", " ").Capitalize();
            viewlist["field-key"] = fieldKey;
            viewlist["partial"] = partials[0];
            viewlist["lang"] = lang;
            viewlist["container"] = container;
            viewlist["render-api"] = args.ContainsKey("render-api") ? "'" + args["render-api"] + "'" : "null";
            viewlist["list-items-click"] = listItemsClick;

            //get list items
            try
            {
                var html = new StringBuilder();
                var foundDataSrc = false;
                if (data.IndexOf("data-src=") >= 0)
                {
                    //use data source
                    var parts = data.Split("|!|");
                    var dataSourceKey = parts[0].Split("=")[1];
                    viewlist["lists-key"] = dataSourceKey;
                    var listsPart = parts.Where(a => a.IndexOf("lists=") == 0).FirstOrDefault();
                    var lists = listsPart != null ? listsPart.Replace("lists=", "") : "{}";
                    var settings = JsonSerializer.Deserialize<Dictionary<string, HtmlComponents.List.ListSettings>>(lists);
                    var mysettings = settings != null && settings.ContainsKey(dataSourceKey) ? settings[dataSourceKey] : null;
                    var datasource = Core.Vendors.DataSources.Where(a => a.Key == dataSourceKey).FirstOrDefault();
                    var locked = parts.Contains("locked");
                    var canadd = parts.Contains("add");
                    var canfilter = !args.ContainsKey("hide-filter");
                    var isSingle = parts.Contains("single");
                    var isMultiselect = parts.Contains("multi");
                    if (!canadd) { viewlist.Show("hide-add-list-item"); }
                    if (datasource != null)
                    {
                        foundDataSrc = true;
                        viewlist["datasource"] = (datasource.Helper.Vendor != "" ? datasource.Helper.Vendor + " - " : "") + 
                            datasource.Name;viewlist.Show(locked ? "locked" : "not-locked");
                        viewlist.Show(locked ? "locked" : "not-locked");
                        if (!args.ContainsKey("list-click") && !isMultiselect) { viewlist.Show("no-list-items"); }
                        var datasourceId = dataSourceKey.Replace(datasource.Helper.Prefix + "-", "");
                        if (isSingle || isMultiselect)
                        {
                            //render select options for single selection
                            viewlist.Show("single-selection");
                            if (isMultiselect){ viewlist.Show("multi-selection"); }
                            viewlist.Show("no-lists");
                            viewlist.Show("no-filter");
                            viewlist.Show("no-sort");
                            viewlist.Show("no-position");
                            var colkey = fieldKey;
                            var selected = parts.Where(a => a.IndexOf("selected=") == 0).FirstOrDefault()?.Replace("selected=", "") ?? "";
                            if (string.IsNullOrEmpty(fieldKey))
                            {
                                //get first string column from data source
                                var info = datasource.Helper.Get(datasourceId);
                                colkey = info.Columns.Where(a => a.DataType == DataSource.DataType.Text).FirstOrDefault()?.Name ?? "";
                            }
                            if (colkey == "")
                            {
                                viewlist["list-items-options"] = "<option value=\"\">[Err: No text column found for list!]</option>";
                            }
                            else
                            {
                                var results = datasource.Helper.Filter(request, datasourceId, 1, 1000).ToList();
                                var ids = selected.Split(",");
                                if (isMultiselect)
                                {

                                    //load list based on selected items
                                    var items = results.Where(a =>
                                    {
                                        var id = a.ContainsKey("Id") ? a["Id"] : a[colkey];
                                        return ids.Contains(id);
                                    }).ToList();

                                    //add hidden list item to use when adding list items via javascript
                                    viewitem.Clear();
                                    viewitem["attrs"] = "class=\"template\" style=\"display:none\"";
                                    html.Append(viewitem.Render());

                                    var i = 0;
                                    foreach (var item in items)
                                    {
                                        viewitem.Clear();
                                        viewitem["label"] = colkey != "" ? item[colkey] : "List Item #" + i;
                                        viewitem["index"] = item["Id"].ToString();
                                        viewitem["onclick"] = "";
                                        html.Append(viewitem.Render());
                                    }
                                    viewlist["list-contents"] = "<ul class=\"list\">" + html.ToString() + "</ul>";
                                    viewlist["item-count"] = items.Count.ToString();

                                    //remove items from list that are already selected
                                    results = results.Where(a =>
                                    {
                                        var id = a.ContainsKey("Id") ? a["Id"] : a[colkey];
                                        return !ids.Contains(id);
                                    }).ToList();
                                }
                                viewlist["list-items-options"] = string.Join("\n", results.Select(a =>
                                {
                                    var id = a.ContainsKey("Id") ? a["Id"] : a[colkey];
                                    return "<option value=\"" + id + "\"" + (ids.Contains(id) ? " selected" : "") + ">" + 
                                    a[colkey] + "</option>";
                                }));
                            }
                        }
                        else
                        {
                            //render data source filter form
                            if (canfilter)
                            {
                                viewlist["filter-contents"] = DataSource.RenderFilters(request, datasource, mysettings?.Filters);
                                viewlist["orderby-contents"] = DataSource.RenderOrderByList(datasource, mysettings?.OrderBy);
                                viewlist["position-contents"] = DataSource.RenderPositionSettings(datasource, mysettings?.Position);
                            }
                            else
                            {
                                viewlist.Show("no-filter");
                                viewlist.Show("no-sort");
                                viewlist.Show("no-position");
                            }
                            
                            var relationships = datasource.Helper.Get(datasourceId).Relationships;
                            if (relationships.Length == 0)
                            {
                                viewlist.Show("no-lists");
                            }
                            else
                            {
                                viewlist["lists"] = "<option value=\"" + dataSourceKey + "\">" + 
                                    key.Replace("list-", "").Replace("-", " ").Capitalize() + "</option>" +
                                    string.Join('\n', relationships.Select(a => "<option value=\"" +
                                    Core.Vendors.DataSources.Where(b => b.Key == a.ChildKey).FirstOrDefault()?.Helper.Prefix +
                                    "-" + a.Child.Key + "\">" + a.ListComponent.Replace("list-", "").Replace("-", " ").Capitalize() + 
                                    "</option>").ToArray());
                            }
                        }
                    }
                }
                if (foundDataSrc == false)
                {
                    //use custom list items
                    viewlist.Show("no-lists");
                    if (!string.IsNullOrEmpty(data) && data.IndexOf("data-src=") < 0)
                    {
                        var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(data);
                        var i = 1;
                        foreach (var item in items)
                        {
                            viewitem.Clear();
                            viewitem["label"] = fieldKey != "" && item.ContainsKey(fieldKey) ? item[fieldKey] : "List Item #" + i;
                            viewitem["index"] = i.ToString();
                            viewitem["onclick"] = "S.editor.fields.custom.list.edit(event, '" + viewlist["title"] +
                                "', '" + fieldKey +
                                "', '" + viewlist["partial"].Split(",")[0].Trim() + "', '" + lang + "', '" + container + "')";
                            html.Append(viewitem.Render());
                            i++;
                        }
                        viewlist["item-count"] = items.Count.ToString();
                    }
                    else
                    {
                        viewlist["item-count"] = "0";
                    }
                    viewlist.Show("no-datasource");
                    viewlist.Show("no-filter");
                    viewlist.Show("no-sort");
                    viewlist.Show("no-position");
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
