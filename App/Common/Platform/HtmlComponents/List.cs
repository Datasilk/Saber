using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.HtmlComponents
{
    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class List : IVendorHtmlComponents
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
                new HtmlComponentModel()
                {
                    Key = "list",
                    Name = "List",
                    Block = false,
                    Icon = "components/list.svg",
                    Description = "Generate a list of content using one or more partial views",
                    Parameters = new Dictionary<string, HtmlComponentParameter>()
                    {
                        { "container",
                            new HtmlComponentParameter()
                            {
                                Name = "Container View",
                                DataType = HtmlComponentParameterDataType.PartialView,
                                Description = "The HTML file to use as a wrapper around your list items",
                                Required = false,
                                List = false,
                                AddItemJs = "S.editor.components.partials.show(event, S.editor.components.accordion.accept)"
                            }
                        },
                        { "partial",
                            new HtmlComponentParameter()
                            {
                                Name = "Partial Views",
                                DataType = HtmlComponentParameterDataType.PartialView,
                                Description = "The HTML file(s) used to render each list item",
                                Required = true,
                                List = true,
                                AddItemJs = "S.editor.components.partials.show(event, S.editor.components.accordion.accept)"
                            }
                        },
                        { "loadorder",
                            new HtmlComponentParameter()
                            {
                                Name = "Load Order",
                                DataType = HtmlComponentParameterDataType.List,
                                Description = "If using multiple partial views, the load order determines the pattern to use when choosing which partial view to render for each item in your list",
                                Required = false,
                                ListOptions = new KeyValuePair<string, string>[]
                                    {
                                        new KeyValuePair<string, string>("Loop", "loop"),
                                        new KeyValuePair<string, string>("Reverse", "reverse"),
                                        new KeyValuePair<string, string>("Bounce", "bounce"),
                                        new KeyValuePair<string, string>("Random", "random"),
                                        new KeyValuePair<string, string>("Random First", "random-first"),
                                    },
                                DefaultValue = "0"
                            }
                        },
                        { "key",
                            new HtmlComponentParameter()
                            {
                                Name = "Key Mustache Variable",
                                DataType = HtmlComponentParameterDataType.Text,
                                Description = "The mustache variable located within your partial view to use as a title for each list item",
                                Required = false
                            }
                        }
                    },
                    Render = new Func<View, IRequest, Dictionary<string, string>, Dictionary<string, object>, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        var mydata = data.ContainsKey(key) ? (string)data[key] : "";
                        if (!args.ContainsKey("partial") || string.IsNullOrEmpty(mydata))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                            return new List<KeyValuePair<string, string>>();
                        }
                        var containerPath = args.ContainsKey("container") ? args["container"] : "";
                        var keyColumn = args.ContainsKey("key") ? args["key"] : "";
                        var partialFiles = (args["partial"]).Split("|");
                        var partials = new List<View>();
                        foreach (var file in partialFiles)
                        {
                            partials.Add(new View("/Content/" + file));
                        }
                        View partial = partials[0];
                        View container = containerPath != "" ? new View("/Content/" + containerPath) : null;

                        //determine load order
                        var order = args.ContainsKey("loadorder") ? args["loadorder"] : "loop";

                        #region "get records ////////////////////////////////////////////////////////////////"
                        //deserialize the list data
                        //try {
                        List<Dictionary<string, string>> records;
                        DataSource.Relationship[] relationships = null;
                        DataSourceInfo datasource = null;
                        Dictionary<string, ListSettings> settings = null;
                        ListSettings mysettings = null;
                        Dictionary<string, int> totals = null;
                        int total = 0;
                        int start = 1;
                        int length = 1000;

                        if(mydata.IndexOf("data-src=") >= 0)
                        {
                            data = data.ToDictionary(a => a.Key, a => a.Value);
                            //get list options
                            var parts = mydata.Split("|!|", StringSplitOptions.RemoveEmptyEntries);
                            var dataSourceKey = parts[0].Split("=")[1];
                            var recordsetPart = parts.Where(a => a.IndexOf("recordset=") == 0).FirstOrDefault();
                            var recordset = recordsetPart != null ? recordsetPart.Replace("recordset=", "") : "";
                            var recordidPart = parts.Where(a => a.IndexOf("recordid=") == 0).FirstOrDefault();
                            var recordid = recordidPart != null ? recordidPart.Replace("recordid=", "") : "";
                            var columnPart = parts.Where(a => a.IndexOf("column=") == 0).FirstOrDefault();
                            var column = columnPart != null ? columnPart.Replace("column=", "") : "";
                            var listsPart = parts.Where(a => a.IndexOf("lists=") == 0).FirstOrDefault();
                            var lists = listsPart != null ? listsPart.Replace("lists=", "") : "{}";

                            if(recordset != "")
                            {
                                //get cached settings list
                                settings = (Dictionary<string, ListSettings>)data[recordset + "-list-settings"];
                            }
                            else
                            {
                                //load settings from content field data
                                settings = JsonSerializer.Deserialize<Dictionary<string, ListSettings>>(lists);
                                data.Add(key + "-list-settings", settings);
                            }
                            mysettings = settings.ContainsKey(dataSourceKey) ? settings[dataSourceKey] : null;

                            if(mysettings != null)
                            {
                                start = mysettings.Position.Start;
                                length = mysettings.Position.Length;

                                //override list options from request parameters
                                foreach(var group in mysettings.Filters)
                                {
                                    OverrideFilterGroupValues(request, group);
                                }
                                if(mysettings.Position.StartQuery != "" && request.Parameters.ContainsKey(mysettings.Position.StartQuery))
                                {
                                    int.TryParse(request.Parameters[mysettings.Position.StartQuery], out var xstart);
                                    if(xstart > 0){mysettings.Position.Start = xstart; }
                                }
                                if(mysettings.Position.LengthQuery != "" && request.Parameters.ContainsKey(mysettings.Position.LengthQuery))
                                {
                                    int.TryParse(request.Parameters[mysettings.Position.LengthQuery], out var xlength);
                                    if(xlength > 0){mysettings.Position.Length = xlength; }
                                }
                            }

                            //get records from data source
                            datasource = Core.Vendors.DataSources.Where(a => a.Key == dataSourceKey).FirstOrDefault();
                            if(datasource != null)
                            {
                                var datasourceId = dataSourceKey.Replace(datasource.Helper.Prefix + "-", "");
                                if(recordset != "")
                                {
                                    var recordsets = (Dictionary<string, List<Dictionary<string, string>>>)data[recordset + "-recordset"];
                                    records = recordsets.ContainsKey(datasourceId) ? recordsets[datasourceId].Where(a => a[column] == recordid).ToList() : new List<Dictionary<string, string>>();
                                    totals = (Dictionary<string, int>)data[recordset + "-totals"];
                                    total = totals.ContainsKey(datasourceId) ? totals[datasourceId] : 0;
                                }
                                else
                                {
                                    relationships = datasource.Helper.Get(datasourceId).Relationships;
                                    if(relationships.Length > 0)
                                    {
                                        var childFilters = new Dictionary<string, List<DataSource.FilterGroup>>();
                                        var childOrderBy = new Dictionary<string, List<DataSource.OrderBy>>();
                                        if(mysettings != null)
                                        {
                                            childFilters.Add(dataSourceKey, mysettings.Filters);
                                            childOrderBy.Add(dataSourceKey, mysettings.OrderBy);
                                        }
                                            
                                        //get record sets for list & sub-lists
                                        var recordsets = datasource.Helper.Filter(request, datasourceId, request.User.Language ?? "en", settings.ToDictionary(a => a.Key, a => a.Value.Position), settings.ToDictionary(a => a.Key, a => a.Value.Filters), settings.ToDictionary(a => a.Key, a => a.Value.OrderBy), relationships.Select(a => a.ChildKey).ToArray());
                                        records = recordsets.ContainsKey(datasourceId) ? recordsets[datasourceId] : new List<Dictionary<string, string>>();

                                        //find settings for each list component
                                        foreach(var relationship in relationships)
                                        {
                                            if (data.ContainsKey(relationship.ListComponent))
                                            {
                                                data.Remove(relationship.ListComponent);
                                            }
                                            data.Add(relationship.ListComponent, "data-src=" + relationship.ChildKey +
                                                "|!|recordset=" + key + "|!|column=" + relationship.ChildColumn);
                                        }
                                            
                                        data.Add(key + "-recordset", recordsets);

                                        //save filtered record set totals
                                        data.Add(key + "-totals", datasource.Helper.FilterTotal(request, datasourceId, request.User.Language ?? "en", settings.ToDictionary(a => a.Key, a => a.Value.Filters), settings.ToDictionary(a => a.Key, a => a.Value.OrderBy), relationships.Select(a => a.ChildKey).ToArray()));
                                    }
                                    else
                                    {
                                        records = datasource.Helper.Filter(request, dataSourceKey.Replace(datasource.Helper.Prefix + "-", ""), mysettings?.Position.Start ?? 1, mysettings?.Position.Length ?? 10, request.User.Language ?? "en", mysettings?.Filters, mysettings?.OrderBy);
                                        total = datasource.Helper.FilterTotal(request, dataSourceKey.Replace(datasource.Helper.Prefix + "-", ""), request.User.Language ?? "en", mysettings?.Filters, mysettings?.OrderBy);
                                    }
                                }
                            }
                            else
                            {
                                records = new List<Dictionary<string, string>>();
                            }
                        }
                        else
                        {
                            //get items that were manually created by the user
                            records = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(mydata);
                            total = records.Count;
                        }
#endregion

                        //get container elements
                        View itemButton = null;
                        View pageButton = null;
                        StringBuilder itemButtons = null;
                        StringBuilder pageButtons = null;
                        if(container != null)
                        {
                            itemButtons = new StringBuilder();
                            pageButtons = new StringBuilder();

                            //item-button
                            var elem = container.Elements.Where(a => a.Name == "item-button").FirstOrDefault();
                            if(elem.Htm != "")
                            {
                                itemButton = new View(new ViewOptions(){Html = elem.Htm });
                            }
                            //page-button
                            elem = container.Elements.Where(a => a.Name == "page-button").FirstOrDefault();
                            if(elem.Htm != null && elem.Htm != "")
                            {
                                pageButton = new View(new ViewOptions(){Html = elem.Htm }); 
                            }
                        }

                        #region "render all records using partial views ////////////////////////////////////////////////////////////////"
                        var html = new StringBuilder();
                        var i = -1;
                        var x = 0;
                        var forward = true;
                        if(records == null){
                            results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                            return results;
                        }
                        foreach (var record in records)
                        {
                            x++;
                            if(mysettings != null && mysettings.Position.Length < x){ break; }
                            switch (order)
                            {
                                case "loop":
                                    i++;
                                    if (i >= partials.Count)
                                    {
                                        i = 0;
                                    }
                                    break;
                                case "reverse":
                                    i--;
                                    if (i < 0)
                                    {
                                        i = partials.Count - 1;
                                    }
                                    break;
                                case "bounce":
                                    i = i + (forward ? 1 : -1);
                                    if (i < 0) { i = 1; forward = true; }
                                    if (i >= partials.Count)
                                    {
                                        i = partials.Count - 2;
                                        forward = false;
                                    }
                                    break;
                                case "random":
                                    var rnd = new Random();
                                    i = rnd.Next(0, partials.Count);
                                    break;
                                case "random-first":
                                    if (forward == true)
                                    {
                                        var rnd2 = new Random();
                                        i = rnd2.Next(0, partials.Count);
                                        forward = false;
                                    }
                                    else
                                    {
                                        i++;
                                        if (i >= partials.Count)
                                        {
                                            i = 0;
                                        }
                                    }
                                    break;
                            }
                            //select partial from array
                            partial = partials[i];

                            //populate all mustache variables
                            foreach (var kv in record)
                            {
                                partial[kv.Key] = kv.Value;
                            }

                            //render all HTML components in the partial view
                            if(datasource != null && relationships != null && relationships.Length > 0)
                            {
                                //modify data for specific list component
                                foreach(var relationship in relationships)
                                {
                                    if (data.ContainsKey(relationship.ListComponent))
                                    {
                                        var d = (string)data[relationship.ListComponent];
                                        if (d.Contains("recordid="))
                                        {
                                            var s = d.Split("recordid=");
                                            s[1] = record["Id"];
                                            d = string.Join("recordid=", s);
                                        }
                                        else
                                        {
                                            d += "|!|recordid=" + record["Id"];
                                        }
                                        data[relationship.ListComponent] = d;
                                    }
                                }
                            }
                            var components = Render.HtmlComponents(partial, request, data);
                            if (components.Count > 0)
                            {
                                foreach (var item in components)
                                {
                                    partial[item.Key] = item.Value;
                                }
                            }

                            //container-related rendering
                            if(itemButton != null)
                            {
                                itemButton.Clear();
                                itemButton["item-number"] = x.ToString();
                                itemButton["item-key"] = keyColumn != "" && record.ContainsKey(keyColumn) ? record[keyColumn] : "";
                                itemButtons.Append(itemButton.Render());
                            }
                            partial.Show(x == 1 ? "is-first-item" : "not-first-item");

                            //render list item as HTML
                            html.Append(partial.Render());
                            partial.Clear();
                        }
#endregion

                        if(container != null)
                        {
                            //render container elements
                            var startQuery = mysettings?.Position.StartQuery ?? "";
                            var lengthQuery = mysettings?.Position.LengthQuery ?? "";
                            var totalPages = Math.Floor((decimal)total / (decimal)length) + 1;
                            var currentPage = (Math.Floor((decimal)start / (decimal)length) + 1);
                            if(startQuery != "")
                            {
                                for(var y = 1; y <= totalPages; y++)
                                {
                                    pageButton.Clear();
                                    pageButton["page-number"] = y.ToString();
                                    pageButton["page-url"] = request.AlterUrl(new Dictionary<string, string>(){
                                        { startQuery, (y * length).ToString() }
                                    });
                                    pageButtons.Append(pageButton.Render());
                                }
                                container["page-buttons"] = pageButtons.ToString();
                            }
                            else
                            {
                                container["page-buttons"] = "<span title=\"You must set the Starting Record > URL Query String Parameter in your List component content field settings to display paging buttons\">Paging Buttons Error</span>";
                            }

                            container["item-buttons"] = itemButtons.ToString();
                            container["current-page"] = currentPage.ToString();
                            container["total-pages"] = totalPages.ToString();
                            if( totalPages != 1){container.Show("is-plural"); }
                            container.Show(start <= 1 ? "hide-back" : "show-back");
                            container.Show(start + length >= total ? "hide-next" : "show-next");
                            container["back-url"] = request.AlterUrl(new Dictionary<string, string>(){
                                        { startQuery, (start - length).ToString() }
                                    });
                            container["next-url"] = request.AlterUrl(new Dictionary<string, string>(){
                                        { startQuery, (start + length).ToString() }
                                    });
                            container["list"] = html.ToString();

                            //render container
                            results.Add(new KeyValuePair<string, string>(prefix + key, container.Render()));
                        }
                        else
                        {
                        results.Add(new KeyValuePair<string, string>(prefix + key, html.ToString()));
                        }
                        //}
                        //catch (Exception ex)
                        //{
                        //    results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                        //}
                        return results;
                    })
                }
            };
        }

        private void OverrideFilterGroupValues(IRequest request, DataSource.FilterGroup group)
        {
            //check filters for request parameter overrides
            foreach(var elem in group.Elements)
            {
                if (elem.QueryName != "" && request.Parameters.ContainsKey(elem.QueryName))
                {
                    elem.Value = request.Parameters[elem.QueryName];
                }
            }
            //check sub groups for request parameter overrides
            foreach (var sub in group.Groups)
            {
                OverrideFilterGroupValues(request, sub);
            }
        }

        public class ListSettings
        {
            public DataSource.PositionSettings Position { get; set; }
            public List<DataSource.FilterGroup> Filters { get; set; }
            public List<DataSource.OrderBy> OrderBy { get; set; }
        }
    }
}