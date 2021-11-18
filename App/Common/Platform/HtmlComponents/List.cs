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
                        { "partial",
                            new HtmlComponentParameter()
                            {
                                Name = "Partial View",
                                DataType = HtmlComponentParameterDataType.PartialView,
                                Description = "The HTML file to use as a partial view",
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
                                Description = "If using multiple partial views, the load order determines the pattern to use when selecting which partial view to use for each item in your list",
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
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        if (!args.ContainsKey("partial") || string.IsNullOrEmpty(data))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                            return new List<KeyValuePair<string, string>>();
                        }
                        var partialFiles = args["partial"].Split("|");
                        var partials = new List<View>();
                        foreach (var file in partialFiles)
                        {
                            partials.Add(new View("/Content/" + file));
                        }
                        View partial = partials[0];

                        //determine load order
                        var order = args.ContainsKey("loadorder") ? args["loadorder"] : "loop";

                        //deserialize the list data
                        try
                        {
                            List<Dictionary<string, string>> records;
                            if(data.IndexOf("data-src=") >= 0)
                            {
                                //get list options
                                var parts = data.Split("|!|");
                                var dataSourceKey = parts[0].Split("=")[1];
                                var startPart = parts.Where(a => a.IndexOf("start=") == 0).FirstOrDefault();
                                var startParts = startPart != null ? startPart.Replace("start=", "").Split("|") : new string[] { };
                                var start = startPart != null ? int.Parse(startParts[0]) : 1;
                                var startQuery = startPart != null ? (startParts.Length > 1 ? startParts[1] : "") : "";
                                var lengthPart = parts.Where(a => a.IndexOf("length=") == 0).FirstOrDefault();
                                var lengthParts = lengthPart != null ? lengthPart.Replace("length=", "").Split("|") : new string[] { };
                                var length = lengthPart != null ? int.Parse(lengthParts[0]) : 10;
                                var lengthQuery = lengthPart != null ? (lengthParts.Length > 1 ? lengthParts[1] : "") : "";
                                var filterPart = parts.Where(a => a.IndexOf("filter=") == 0).FirstOrDefault();
                                var filter = JsonSerializer.Deserialize<List<DataSource.FilterGroup>>(filterPart != null ? filterPart.Replace("filter=", "") : "[]");
                                var sortPart = parts.Where(a => a.IndexOf("sort=") == 0).FirstOrDefault();
                                var sort = JsonSerializer.Deserialize<List<DataSource.OrderBy>>(sortPart != null ? sortPart.Replace("sort=", "") : "[]");

                                //override list options from request parameters
                                if(startQuery != "" && request.Parameters.ContainsKey(startQuery))
                                {
                                    int.TryParse(request.Parameters[startQuery], out start);
                                }
                                if(lengthQuery != "" && request.Parameters.ContainsKey(lengthQuery))
                                {
                                    int.TryParse(request.Parameters[lengthQuery], out length) ;
                                }
                                foreach(var group in filter)
                                {
                                    OverrideFilterGroupValues(request, group);
                                }

                                //get records
                                var datasource = Core.Vendors.DataSources.Where(a => a.Key == dataSourceKey).FirstOrDefault();
                                if(datasource != null)
                                {
                                    records = datasource.Helper.Filter(request, dataSourceKey.Replace(datasource.Helper.Prefix + "-", ""), start, length, request.User.Language ?? "en", filter, sort);
                                }
                                else
                                {
                                    records = new List<Dictionary<string, string>>();
                                }
                            }
                            else
                            {
                                //get items that were manually created by the user
                                records = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(data);
                            }
                            var html = new StringBuilder();
                            var i = -1;
                            var forward = true;
                            if(records == null){
                                results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                                return results;
                            }
                            foreach (var record in records)
                            {
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
                                partial = partials[i];
                                foreach (var kv in record)
                                {
                                    partial[kv.Key] = kv.Value;
                                }
                                html.Append(partial.Render());
                                partial.Clear();
                            }
                            results.Add(new KeyValuePair<string, string>(prefix + key, html.ToString()));
                        }
                        catch (Exception ex)
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + key, ""));
                        }
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
    }
}