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
            var config = PageInfo.GetPageConfig(path);
            var htmlVars = Common.Platform.ViewDataBinder.GetHtmlVariables();
            var view = new View(string.Join("/", paths) + ".html");
            var viewHeader = new View("/Content/partials/" + config.header.file);
            var viewFooter = new View("/Content/partials/" + config.footer.file);
            var section = new View("/Views/ContentFields/section.html");
            var fieldText = new View("/Views/ContentFields/text.html");
            var fieldBlock = new View("/Views/ContentFields/block.html");

            var result = processView("Body", view, fields, section, fieldBlock, fieldText, htmlVars);
            var resultHead = processView(PageInfo.NameFromFile(config.header.file), viewHeader, fields, section, fieldBlock, fieldText, htmlVars);
            var resultFoot = processView(PageInfo.NameFromFile(config.footer.file), viewFooter, fields, section, fieldBlock, fieldText, htmlVars);

            if (string.IsNullOrWhiteSpace(result) &&
                string.IsNullOrWhiteSpace(resultHead) &&
                string.IsNullOrWhiteSpace(resultFoot))
            {
                var nofields = new View("/Views/ContentFields/nofields.html");
                nofields["filename"] = paths[paths.Length - 1];
                return nofields.Render();
            }
            return resultHead + result + resultFoot;
        }

        private string processView(string title, View view, Dictionary<string, string> fields, View section, View fieldBlock, View fieldText, string[] vars)
        {
            var html = new StringBuilder();
            for (var x = 0; x < view.Elements.Count; x++)
            {
                var elem = view.Elements[x];
                
                if (elem.Name != "" && elem.Name.Substring(0, 1) != "/")
                {
                    //get element name with no partial file prefixes
                    var elemName = elem.Name;
                    foreach (var partial in view.Partials)
                    {
                        elemName = elemName.Replace(partial.Prefix, "");
                    }
                    var val = "";
                    if (fields.ContainsKey(elem.Name))
                    {
                        //get existing content for field
                        val = fields[elem.Name];

                    }
                    if (view.Elements.Any(a => a.Name == "/" + elem.Name) && !vars.Any(a => a == elemName))
                    {
                        //load block field
                        fieldBlock.Clear();
                        fieldBlock["title"] = elemName.Capitalize().Replace("-", " ").Replace("_", " ");
                        fieldBlock["id"] = "field_" + elem.Name.Replace("-", "").Replace("_", "");
                        if (val == "1") { fieldBlock.Show("checked"); }
                        html.Append(fieldBlock.Render());
                    }
                    else
                    {
                        var found = false;
                        //find vendor content field


                        if (found == false)
                        {
                            //check to see if content field is inside an HTML element
                            if (x > 0)
                            {
                                var prev = view.Elements[x - 1];
                                var inQuotes = false;
                                var quotes = 0;
                                for (var i = prev.Htm.Length - 1; i >= 0; i--)
                                {
                                    if (prev.Htm[i] == '"') { quotes++; }
                                    if (prev.Htm[i] == '=' && quotes == 1) { inQuotes = true; }
                                    if (prev.Htm[i] == '>') { break; }
                                    if (prev.Htm[i] == '<')
                                    {
                                        //found html element
                                        if (inQuotes == true)
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
                                                        if (tagName == "img")
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
                        if (found == false && !vars.Any(a => a == elemName))
                        {
                            fieldText.Clear();
                            fieldText["title"] = elemName.Capitalize().Replace("-", " ").Replace("_", " ");
                            fieldText["id"] = "field_" + elem.Name.Replace("-", "").Replace("_", "");
                            fieldText["default"] = val;
                            html.Append(fieldText.Render());
                        }
                    }
                }
            }
            section.Clear();
            section["title"] = title;
            section["fields"] = html.ToString();
            return html.Length == 0 ? "" : section.Render();
        }

        public string Save(string path, string language, Dictionary<string, string> fields)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }

            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var config = PageInfo.GetPageConfig(path);
            var validated = new Dictionary<string, string>();
            ValidateField(string.Join("/", paths) + ".html", fields, validated);
            ValidateField("/Content/partials/" + config.header.file, fields, validated);
            ValidateField("/Content/partials/" + config.footer.file, fields, validated);
            try
            {
                //save fields as json
                var json = JsonSerializer.Serialize(validated);
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

        private void ValidateField(string path, Dictionary<string, string> fields, Dictionary<string, string> results)
        {
            var view = new View(path);
            foreach (var elem in view.Elements)
            {
                if (elem.Name != "")
                {
                    var name = elem.Name.Replace("-", "").Replace("_", "");
                    if (fields.ContainsKey(name) && fields[name] != "" && !results.ContainsKey(name))
                    {
                        results.Add(name, fields[name]);
                    }
                }
            }
        }
    }
}
