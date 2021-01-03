using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform
{
    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class HtmlComponents : IVendorHtmlComponents
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
                new HtmlComponentModel()
                {
                    Key = "-",
                    Name = "Line Break",
                    Block = false,
                    Description = "Used to separate groups of content fields by creating a line break within the Content Fields form.",
                    //Parameters = new Dictionary<string, HtmlComponentParameter>()
                    //{
                    //    {"title", 
                    //        new HtmlComponentParameter()
                    //        {
                    //            Name = "Title",
                    //            DataType = HtmlComponentParameterDataType.Text,
                    //            Description = "Display a title above your line break",
                    //            Required = false
                    //        } 
                    //    }
                    //},
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "-", ""));
                        return results;
                    })
                },
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "list",
                    Name = "List",
                    Block = false,
                    Icon = "components/list.svg",
                    Description = "Generate a list of partial views",
                    Parameters = new Dictionary<string, HtmlComponentParameter>()
                    {
                        {"partial",
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
                        {"loadorder",
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
                        {"key",
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
                        if(!args.ContainsKey("partial") || string.IsNullOrEmpty(data))
                        {
                            return new List<KeyValuePair<string, string>>();
                        }
                        var partialFiles = args["partial"].Split("|");
                        var partials = new List<View>();
                        foreach(var file in partialFiles)
                        {
                            partials.Add(new View("/Content/" + file));
                        }
                        View partial = partials[0];

                        //determine load order
                        var order = args.ContainsKey("loadorder") ? args["loadorder"] : "loop";

                        //deserialize the list data
                        try
                        {
                            var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(data);
                            var html = new StringBuilder();
                            var i = -1;
                            var forward = true;
                            foreach(var item in items)
                            {
                                switch (order)
                                {
                                    case "loop":
                                        i++;
                                        if(i >= partials.Count)
                                        {
                                            i = 0;
                                        }
                                        break;
                                    case "reverse":
                                        i--;
                                        if(i < 0)
                                        {
                                            i = partials.Count - 1;
                                        }
                                        break;
                                    case "bounce":
                                        i = i + (forward ? 1 : -1);
                                        if(i < 0){ i = 1; forward = true; }
                                        if(i >= partials.Count)
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
                                        if(forward == true)
                                        {
                                            var rnd2 = new Random();
                                            i = rnd2.Next(0, partials.Count);
                                            forward = false;
                                        }
                                        else
                                        {
                                            i++;
                                            if(i >= partials.Count)
                                            {
                                                i = 0;
                                            }
                                        }
                                        break;
                                }
                                partial = partials[i];
                                foreach(var kv in item)
                                {
                                    partial[kv.Key] = kv.Value;
                                }
                                html.Append(partial.Render());
                                partial.Clear();
                            }
                            results.Add(new KeyValuePair<string, string>(prefix + key, html.ToString()));
                        }
                        catch(Exception) { }
                        return results;
                    })
                },
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "user",
                    Name = "User Logged In",
                    Block = true,
                    Description = "Display a block of HTML if the user is logged into their account",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "user", "True"));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "username",
                    Name = "User Name",
                    Description = "Display the user's name",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "username", request.User.Name));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "userid",
                    Name = "User ID",
                    Description = "Display the user's ID",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if(request.User.UserId > 0 && !request.Parameters.ContainsKey("live"))
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "userid", request.User.UserId.ToString()));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "no-user",
                    Name = "User Not Logged In",
                    Block = true,
                    Description = "Display a block of HTML when the user is not logged into their account",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        //check if user is logged in
                        if (request.User.UserId == 0)
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "no-user", "True"));
                        }
                        return results;
                    })
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                , new HtmlComponentModel()
                {
                    Key = "page-url",
                    Name = "Page URL",
                    Description = "Return the canonical URL based on the page request",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                        {
                            var results = new List<KeyValuePair<string, string>>();
                            var req = request.Context.Request;
                            var url = string.Concat(
                                req.Scheme,
                                "://",
                                req.Host.ToUriComponent(),
                                req.PathBase.ToUriComponent(),
                                req.Path.ToUriComponent(),
                                req.QueryString.ToUriComponent()
                            );
                            results.Add(new KeyValuePair<string, string>(prefix + "page-url", url));
                            return results;
                        })
                    }
                , 
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "page-id",
                    Name = "Page ID",
                    Description = "Return an ID based on the page URL",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        if(request.Path == "")
                        {
                            results.Add(new KeyValuePair<string, string>(prefix + "page-id", "home"));
                        }
                        else if (request.Path.Contains("api/") && request.Parameters.ContainsKey("path"))
                        {
                            var paths = request.Parameters["path"].Replace("content/", "").Replace(".html", "").Split("/");
                            results.Add(new KeyValuePair<string, string>(prefix + "page-id", string.Join('_', paths)));
                        }
                        else
                        {
                            var paths = request.Path.Split('/');
                            results.Add(new KeyValuePair<string, string>(prefix + "page-id", string.Join('_', paths)));
                        }
                        return results;
                    })
                },

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "year",
                    Name = "Current Year",
                    Description = "Display the current year",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "year", DateTime.Now.Year.ToString()));
                        return results;
                    })
                },

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                new HtmlComponentModel()
                {
                    Key = "languages.options",
                    Name = "Language List Form",
                    Description = "Render an HTML form so your users can select their preferred language.",
                    Render = new Func<View, IRequest, Dictionary<string, string>, string, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var selected = request.Parameters.ContainsKey("lang") ? request.Parameters["lang"] : request.User.Language;
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "languages.options",
                            string.Join("\n", App.Languages.Select(a => "<option value=\"" + a.Key + "\"" +
                                (selected == a.Key ? " selected" : "") + ">" + a.Value + "</option>").ToArray())
                            ));
                        return results;
                    }),
                    HtmlHead = "<form id=\"changelang\" method=\"post\"><select name=\"lang\" onchange=\"changelang.submit()\">",
                    HtmlFoot = "</select></form>"
                }
            };
        }
    }
}
