using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.HtmlComponents
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
                    KeyIsPrefix = true,
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
                        ListData myData = null;
                        if (data.ContainsKey(key))
                        {
                            if(data[key].GetType() == typeof(ListData))
                            {
                                myData = (ListData)data[key];
                            }
                            else
                            {
                                //convert data to object
                                var mydata = (string)data[key];
                                var parts = mydata.Split("|!|", StringSplitOptions.RemoveEmptyEntries);
                                myData = new ListData()
                                {
                                    DataSource = parts.Where(a => a.Contains("data-src=")).FirstOrDefault()?.Split("=")[1] ?? "",
                                };
                                if (!string.IsNullOrEmpty(myData.DataSource))
                                {
                                    //get other settings
                                    myData.Settings = JsonSerializer.Deserialize<Dictionary<string, ListSettings>>(parts.Where(a => a.Contains("lists=")).FirstOrDefault()?.Split("=")[1] ?? "{}") ?? new Dictionary<string, ListSettings>();
                                }
                            }
                        }
                        if (!args.ContainsKey("partial") || myData == null)
                        {
                            //render blank list
                            results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                            return new List<KeyValuePair<string, string>>();
                        }

                        var containerPath = args.ContainsKey("container") ? args["container"] : "";
                        var keyColumn = args.ContainsKey("key") ? args["key"] : "";
                        var partialFiles = (args["partial"].Contains("|") ? args["partial"].Split("|") : args["partial"].Split(",")).Select(a => a.Trim()).ToArray();
                        var partials = new List<View>();
                        foreach (var file in partialFiles)
                        {
                            partials.Add(new View("/Content/" + file));
                        }
                        View partial = partials[0];
                        View container = containerPath != "" ? new View("/Content/" + containerPath) : null;

                        //determine load order
                        var order = args.ContainsKey("loadorder") ? args["loadorder"] : "loop";

                        #region "get records"
                        //deserialize the list data
                        //try {
                        List<Dictionary<string, string>> records = null;
                        DataSource.Relationship[] relationships = null;
                        DataSourceInfo datasource = null;
                        ListSettings mysettings = null;
                        Dictionary<string, int> totals = null;
                        int total = 0;
                        int start = 1;
                        int length = 1000;
                        var hasDataSource = !string.IsNullOrEmpty(myData.DataSource);

                        if(hasDataSource)
                        {
                            //data = data.ToDictionary(a => a.Key, a => a.Value);
                            mysettings = myData.Settings.ContainsKey(myData.DataSource) ? myData.Settings[myData.DataSource] : new ListSettings();

                            if(mysettings != null)
                            {
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
                                start = mysettings.Position.Start;
                                length = mysettings.Position.Length;
                            }

                            //get records from data source
                            datasource = Core.Vendors.DataSources.Where(a => a.Key == myData.DataSource).FirstOrDefault();
                            if(datasource != null)
                            {
                                var datasourceId = myData.DataSource.Replace(datasource.Helper.Prefix + "-", "");
                                if(myData.ParentRecordSet != null)
                                {
                                    if (myData.Relationship.Type == DataSource.RelationshipType.SingleSelection)
                                    {
                                        //get single record for single selection list
                                        var parentRecord = myData.Record;
                                        var listVal = parentRecord[myData.Relationship.ListComponent];
                                        var selectionId = listVal.Contains("selected=") ? listVal.Split("selected=")[1] : "";
                                        if (!string.IsNullOrEmpty(selectionId))
                                        {
                                            records = myData.RecordSets[myData.Relationship.ChildKey.Replace(datasource.Helper.Prefix + "-", "")].Where(a => a["Id"] == selectionId).ToList() ?? new List<Dictionary<string, string>>();
                                        }
                                    }
                                    else
                                    {
                                        records = myData.RecordSets.ContainsKey(datasourceId) ? myData.RecordSets[datasourceId].Where(a => a[myData.Relationship.ChildColumn] == myData.Record["Id"]).ToList() : new List<Dictionary<string, string>>();
                                    }
                                    
                                    totals = myData.Totals;
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
                                            childFilters.Add(myData.DataSource, mysettings.Filters);
                                            childOrderBy.Add(myData.DataSource, mysettings.OrderBy);
                                        }

                                        foreach(var relationship in relationships)
                                        {
                                            if(relationship.Type == DataSource.RelationshipType.SingleSelection)
                                            {
                                                //add filters for single selection relationships
                                                var filterGroup = new DataSource.FilterGroup()
                                                {
                                                    Elements = new List<DataSource.FilterElement>()
                                                    {
                                                        new DataSource.FilterElement()
                                                        {
                                                            Column = relationship.ListComponent,
                                                            Value = "#single-selection",
                                                            Match = DataSource.FilterMatchType.Equals,
                                                        }
                                                    }

                                                };
                                                if (myData.Settings.ContainsKey(relationship.ChildKey))
                                                {
                                                    myData.Settings[relationship.ChildKey].Filters.Add(filterGroup);
                                                }
                                                else
                                                {
                                                    myData.Settings.Add(relationship.ChildKey, new ListSettings()
                                                    {
                                                        Filters = new List<DataSource.FilterGroup>()
                                                        {
                                                            filterGroup
                                                        },
                                                        Position = new DataSource.PositionSettings(),
                                                        OrderBy = new List<DataSource.OrderBy>()
                                                    });
                                                }
                                            }
                                        }
                                            
                                        //get record sets for list & sub-lists
                                        var recordsets = datasource.Helper.Filter(request, datasourceId, request.User.Language ?? "en", 
                                            myData.Settings.ToDictionary(a => a.Key, a => a.Value.Position), 
                                            myData.Settings.ToDictionary(a => a.Key, a => a.Value.Filters), 
                                            myData.Settings.ToDictionary(a => a.Key, a => a.Value.OrderBy), 
                                            relationships.Select(a => a.ChildKey).ToArray());
                                        records = recordsets.ContainsKey(datasourceId) ? recordsets[datasourceId] : new List<Dictionary<string, string>>();

                                        //find settings for each list component
                                        totals = datasource.Helper.FilterTotal(request, datasourceId, request.User.Language ?? "en", myData.Settings.ToDictionary(a => a.Key, a => a.Value.Filters), relationships.Select(a => a.ChildKey).ToArray());

                                        foreach(var relationship in relationships)
                                        {
                                            if (data.ContainsKey(relationship.ListComponent))
                                            {
                                                data.Remove(relationship.ListComponent);
                                            }
                                            var listData = new ListData(){
                                                DataSource = relationship.ChildKey,
                                                ParentKey = key,
                                                ParentRecordSet = recordsets[relationship.Key],
                                                RecordSets = recordsets,
                                                Relationship = relationship,
                                                Totals = totals,
                                                Settings = myData.Settings
                                            };
                                            data.Add(relationship.ListComponent, listData);
                                            //"data-src=" + relationship.ChildKey +
                                            //    "|!|recordset=" + key + "|!|" + (
                                            //    relationship.Type == DataSource.RelationshipType.SingleSelection ? 
                                            //    ("selection=" + relationship.Key + "|!|list=" + relationship.ListComponent)  : 
                                            //    "column=" + relationship.ChildColumn));
                                       }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            records = datasource.Helper.Filter(request, myData.DataSource.Replace(datasource.Helper.Prefix + "-", ""), mysettings?.Position.Start ?? 1, mysettings?.Position.Length ?? 10, request.User.Language ?? "en", mysettings?.Filters, mysettings?.OrderBy);
                                            total = datasource.Helper.FilterTotal(request, myData.DataSource.Replace(datasource.Helper.Prefix + "-", ""), request.User.Language ?? "en", mysettings?.Filters);
                                        }catch(Exception){ }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //get items that were manually created by the user
                            records = JsonSerializer.Deserialize<List<Dictionary<string, string>>>((string)data[key]) ?? null;
                            if(records != null){ total = records.Count; }
                        }
                        if(records == null){
                            //no records, render empty component
                            results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                            return results;
                        }
#endregion

                        //get container elements
                        View pageButton = null;
                        View itemButton = null;
                        StringBuilder itemButtons = new StringBuilder();
                        StringBuilder pageButtons = new StringBuilder();
                        if(container != null)
                        {
                            //item-button-template
                            var elemIndex = container.Elements.FindIndex(a => a.Name == "item-button-template");
                            if(elemIndex >= 0)
                            {
                                //get item button template block
                                itemButton = new View(new ViewOptions(){Html = container.GetBlock(elemIndex) });
                            }
                            //page-button-template
                            elemIndex = container.Elements.FindIndex(a => a.Name == "page-button-template");
                            if(elemIndex >= 0)
                            {
                                //get page button template block
                                pageButton = new View(new ViewOptions(){Html = container.GetBlock(elemIndex) });
                            }
                        }

                        #region "render all records using partial views ////////////////////////////////////////////////////////////////"
                        var html = new StringBuilder();
                        var i = -1;
                        var x = 0;
                        var forward = true;
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
                                foreach(var relationship in relationships)
                                {
                                    if (data.ContainsKey(relationship.ListComponent))
                                    {
                                        //set record 
                                        var d = (ListData)data[relationship.ListComponent];
                                        d.Record = record;
                                    }
                                }
                            }

                            //render all components found within current list item partial
                            var components = Platform.Render.HtmlComponents(partial, request, data);
                            if (components.Count > 0)
                            {
                                foreach (var item in components)
                                {
                                    partial[item.Key] = item.Value;
                                }
                            }

                            //render each item button (if applicable)
                            if(itemButton != null)
                            {
                                itemButton.Clear();
                                itemButton["item-number"] = x.ToString();
                                itemButton["item-label"] = keyColumn != "" && record.ContainsKey(keyColumn) ? record[keyColumn] : "";
                                if(x == 1){itemButton.Show("selected"); }
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
                            if(!string.IsNullOrEmpty(startQuery))
                            {
                                //back button
                                var startIndex = start - length;
                                container["back-number"] = startIndex.ToString();
                                container["back-url"] = request.AlterUrl(new Dictionary<string, string>(){
                                    { startQuery, startIndex.ToString() }
                                });
                                if(startIndex <= 0)
                                {
                                    container.Show("back-disabled");
                                }

                                //next button
                                var nextIndex = start + length;
                                container["next-number"] = nextIndex.ToString();
                                container["next-url"] = request.AlterUrl(new Dictionary<string, string>(){
                                    { startQuery, nextIndex.ToString() }
                                });
                                if(nextIndex > total)
                                {
                                    container.Show("next-disabled");
                                }

                                //page buttons
                                if(pageButton != null)
                                {
                                    for(var p = 1; p <= totalPages; p++)
                                    {
                                        pageButton.Clear();
                                        pageButton["page-number"] = p.ToString();
                                        if(p == currentPage){pageButton.Show("selected"); }
                                        pageButtons.Append(pageButton.Render());
                                    }
                                }
                            }
                            else
                            {
                                //error displaying paging buttons
                                container["back-button"] = container["next-buttons"] = "<span title=\"You must set the \"Starting Record\" URL Query String Parameter & the \"Records Per Page\" URL Query String Parameter in your List component content field settings to display paging buttons\">Paging Buttons Error</span>";
                            }

                            container["item-buttons"] = itemButtons.ToString();
                            container["page-buttons"] = pageButtons.ToString();
                            container["current-page"] = currentPage.ToString();
                            container["total-pages"] = totalPages.ToString();
                            container["total-results"] = total.ToString();
                            container["starting-result"] = start.ToString();
                            container["ending-result"] = (start + length).ToString();
                            container["displayed-results"] = x.ToString();
                            if( totalPages != 1) { container.Show("is-plural"); }
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

        #region "Helper Functions"

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

        #endregion

        #region "Helper Classes"

        public class ListSettings
        {
            [JsonPropertyName("p")]
            public DataSource.PositionSettings Position { get; set; } = new DataSource.PositionSettings();
            [JsonPropertyName("f")]
            public List<DataSource.FilterGroup> Filters { get; set; } = new List<DataSource.FilterGroup>();
            [JsonPropertyName("o")]
            public List<DataSource.OrderBy> OrderBy { get; set; } = new List<DataSource.OrderBy>();
        }

        private class ListData
        {
            public string DataSource { get; set; } = "";
            public string ParentKey { get; set; } = "";
            public List<Dictionary<string, string>> ParentRecordSet { get; set; } = null;
            public Dictionary<string, List<Dictionary<string, string>>> RecordSets { get; set; } = null;
            public DataSource.Relationship Relationship { get; set; } = null;
            public Dictionary<string, string> Record { get; set; } = null;
            public Dictionary<string, ListSettings> Settings { get; set; } = null;
            public Dictionary<string, int> Totals { get; set; } = null;
        }
        #endregion
    }
}