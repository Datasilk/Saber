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
                            List<Dictionary<string, string>> items;
                            if(data.IndexOf("data-src=") == 0)
                            {
                                //get items from custom data source via a vendor plugin
                                var parts = data.Split("|!|");
                                var datakey = parts[0].Split("=")[1];
                                var filter = JsonSerializer.Deserialize<Dictionary<string, object>>(parts.Length > 1 ? parts[1] : "{\"start\":\"1\",\"length\":\"10\"}");
                                var start = filter.ContainsKey("start") && !string.IsNullOrEmpty(filter["start"].ToString()) ? int.Parse(filter["start"].ToString()) : 1;
                                var length = filter.ContainsKey("length") && !string.IsNullOrEmpty(filter["length"].ToString()) ? int.Parse(filter["length"].ToString()) : 1;
                                var datasource = Core.Vendors.DataSources.Where(a => a.Key == datakey).FirstOrDefault();
                                if(datasource != null)
                                {
                                    items = datasource.Helper.Filter(request, datakey.Replace(datasource.Helper.Prefix + "-", ""), start, length, request.User.Language ?? "en", filter);
                                }
                                else
                                {
                                    items = new List<Dictionary<string, string>>();
                                }
                            }
                            else
                            {
                                //get items that were manually created by the user
                                items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(data);
                            }
                            var html = new StringBuilder();
                            var i = -1;
                            var forward = true;
                            foreach (var item in items)
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
                                foreach (var kv in item)
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
                        }
                        return results;
                    })
                }
            };
        }
    }
}