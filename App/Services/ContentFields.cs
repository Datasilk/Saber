using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Saber.Core;
using Saber.Core.Extensions.Strings;
using System.Linq;

namespace Saber.Services
{
    public class ContentFields : Service
    {
        public string Render(string path, string language)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            var paths = PageInfo.GetRelativePath(path);
            var fields = Core.ContentFields.GetPageContent(path, language);
            var html = new StringBuilder();
            var view = new View(string.Join("/", paths) + ".html");
            var fieldText = new View("/Views/ContentFields/text.html");
            var fieldBlock = new View("/Views/ContentFields/block.html");
           for(var x = 0; x < view.Elements.Count; x++)
            {
                var elem = view.Elements[x];
                if (elem.Name != "" && elem.Name.Substring(0,1) != "/")
                {
                    var val = "";
                    if (fields.ContainsKey(elem.Name))
                    {
                        //get existing content for field
                        val = fields[elem.Name];

                    }
                    if(view.Elements.Any(a => a.Name == "/" + elem.Name))
                    {
                        //load block field
                        fieldBlock.Clear();
                        fieldBlock["title"] = elem.Name.Capitalize().Replace("-", " ").Replace("_", " ");
                        fieldBlock["id"] = "field_" + elem.Name.Replace("-", "").Replace("_", "");
                        if(val == "1") { fieldBlock.Show("checked"); }
                        html.Append(fieldBlock.Render());
                    }
                    else
                    {
                        var found = false;
                        //find vendor content field

                        
                        if(found == false)
                        {
                            //check to see if content field is inside an HTML element
                            if (x > 0)
                            {
                                var prev = view.Elements[x - 1];
                                var inQuotes = false;
                                var quotes = 0;
                                for (var i = prev.Htm.Length - 1; x >= 0; x--)
                                {
                                    if (prev.Htm[i] == '"') { quotes++; }
                                    if (prev.Htm[i] == '=' && quotes == 1) { inQuotes = true; }
                                    if (prev.Htm[i] == '>') { break; }
                                    if (prev.Htm[i] == '<')
                                    {
                                        //found html element
                                        if(inQuotes == true)
                                        {
                                            //content field exists inside an HTML element attribute value
                                            try
                                            {
                                                var htmElem = prev.Htm.Substring(i + 1);
                                                var tagParts = htmElem.Split(" ");
                                                var tagName = tagParts[0].ToLower();
                                                var attrName = tagParts[^1].Split("=")[0];

                                                switch (attrName)
                                                {
                                                    case "style":
                                                        //TODO: style parsing support to check if field exists in
                                                        //background or background-image CSS property
                                                        break;
                                                    case "href":
                                                        if(tagName == "img")
                                                        {
                                                            found = true;
                                                            //load image selection field
                                                        }
                                                        break;
                                                }
                                            }
                                            catch (Exception ex) 
                                            {
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }


                        //load text field
                        if (found == false) 
                        { 
                            fieldText.Clear();
                            fieldText["title"] = elem.Name.Capitalize().Replace("-", " ").Replace("_", " ");
                            fieldText["id"] = "field_" + elem.Name.Replace("-", "").Replace("_", "");
                            fieldText["default"] = val;
                            html.Append(fieldText.Render());
                        }
                    }
                }
            }
            if (html.Length == 0)
            {
                var nofields = new View("/Views/ContentFields/nofields.html");
                nofields["filename"] = paths[paths.Length - 1];
                return nofields.Render();
            }
            return html.ToString();
        }

        public string Save(string path, string language, Dictionary<string, string> fields)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }

            var data = new Dictionary<string, string>();
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var view = new View(string.Join("/", paths) + ".html");
            foreach (var elem in view.Elements)
            {
                if (elem.Name != "")
                {
                    var name = elem.Name.Replace("-", "").Replace("_", "");
                    if (fields.ContainsKey(name))
                    {
                        if (fields[name] != "")
                        {
                            data.Add(elem.Name, fields[name]);
                        }
                    }
                }
            }
            try
            {
                //save fields as json
                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(App.MapPath(Core.ContentFields.ContentFile(path, language)), json);
                //reset view cache for page
                Website.ResetCache(path, language);
            }
            catch (Exception)
            {
                return Error();
            }

            return Success();
        }
    }
}
