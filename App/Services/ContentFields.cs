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
            var htmlVars = Common.Platform.HtmlComponentBinder.GetHtmlVariables();
            var view = new View(string.Join("/", paths) + ".html");
            var viewHeader = new View("/Content/partials/" + config.header.file);
            var viewFooter = new View("/Content/partials/" + config.footer.file);
            var section = new View("/Views/ContentFields/section.html");
            var fieldText = new View("/Views/ContentFields/text.html");
            var fieldBlock = new View("/Views/ContentFields/block.html");
            var fieldImage = new View("/Views/ContentFields/image.html");
            var fieldVendor = new View("/Views/ContentFields/vendor.html");

            var result = processView("Body", view, fields, section, fieldBlock, fieldText, fieldImage, fieldVendor, htmlVars);
            var resultHead = processView(PageInfo.NameFromFile(config.header.file), viewHeader, fields, section, fieldBlock, fieldText, fieldImage, fieldVendor, htmlVars);
            var resultFoot = processView(PageInfo.NameFromFile(config.footer.file), viewFooter, fields, section, fieldBlock, fieldText, fieldImage, fieldVendor, htmlVars);

            if (string.IsNullOrWhiteSpace(result) &&
                string.IsNullOrWhiteSpace(resultHead) &&
                string.IsNullOrWhiteSpace(resultFoot))
            {
                var nofields = new View("/Views/ContentFields/nofields.html");
                nofields["filename"] = paths[paths.Length - 1];
                return nofields.Render();
            }
            return Response(resultHead + result + resultFoot);
        }

        private string processView(string title, View view, Dictionary<string, string> fields, View section, View fieldBlock, View fieldText, View fieldImage, View fieldVendor, string[] vars)
        {
            var html = new StringBuilder();
            for (var x = 0; x < view.Elements.Count; x++)
            {
                var elem = view.Elements[x];

                if (elem.Name != "" && elem.Name.Substring(0, 1) != "/")
                {
                    //get element name with no partial file prefixes
                    var elemName = elem.Name;
                    var prefix = "";
                    foreach (var partial in view.Partials)
                    {
                        elemName = elemName.Replace(partial.Prefix, "");
                    }
                    //get partial view prefix from element name
                    prefix = elem.Name.Replace(elemName, "");
                    if(prefix == elemName) { prefix = ""; }

                    //clean field title & field Id
                    var fieldTitle = elemName.Capitalize().Replace("-", " ").Replace("_", " ");
                    var fieldId = "field_" + elem.Name
                        .Replace("!", "_1_")
                        .Replace("@", "_2_")
                        .Replace("#", "_3_")
                        .Replace("$", "_4_")
                        .Replace("%", "_5_")
                        .Replace("^", "_6_")
                        .Replace("&", "_7_")
                        .Replace("*", "_8_")
                        .Replace("(", "_9_")
                        .Replace(")", "_0_")
                        .Replace("+", "_p_")
                        .Replace("[", "_a_")
                        .Replace("{", "_b_")
                        .Replace("]", "_c_")
                        .Replace("}", "_d_")
                        .Replace("=", "_e_")
                        .Replace("|", "_f_")
                        .Replace("\\", "_g_")
                        .Replace(";", "_h_")
                        .Replace(":", "_i_")
                        .Replace("'", "_j_")
                        .Replace(",", "_k_")
                        .Replace("<", "_l_")
                        .Replace(".", "_m_")
                        .Replace(">", "_n_")
                        .Replace("/", "_o_")
                        .Replace("?", "_p_")
                        .Replace("\"", "_q_")
                        .Replace("`", "_r_")
                        .Replace(" ", "_s_")
                        .Replace("~", "_t_");

                    //get existing content for field
                    var fieldValue = "";
                    if (fields.ContainsKey(elem.Name))
                    {
                        fieldValue = fields[elem.Name];
                    }
                    var fieldValueHtml = fieldValue.Replace("\"", "&quot;");

                    //determine which content field layout to load
                    if (view.Elements.Any(a => a.Name == "/" + elem.Name) && !vars.Any(a => a == elemName))
                    {
                        //load block field
                        fieldBlock.Clear();
                        fieldBlock["title"] = fieldTitle;
                        fieldBlock["id"] = fieldId;
                        if (fieldValue == "1") { fieldBlock.Show("checked"); }
                        html.Append(fieldBlock.Render());
                    }
                    else
                    {
                        var found = false;
                        //find vendor content field
                        var vendor = Vendors.ContentFields.Where(a => elemName.IndexOf(a.Key) == 0).FirstOrDefault();
                        if(vendor.Value != null)
                        {
                            found = true;
                            fieldVendor.Clear();
                            var fieldTitleId = fieldTitle.Replace(vendor.Key.Capitalize().Replace("-", " ").Replace("_", " "), "");
                            var fieldTitleKey = fieldTitle.Replace(fieldTitleId, "");
                            fieldVendor["title"] = fieldTitleKey + ": " + fieldTitleId.Capitalize();
                            fieldVendor["id"] = fieldId;
                            fieldVendor["value"] = fieldValueHtml;
                            fieldVendor["content"] = vendor.Value.Render(this, elem.Vars, fieldValue, fieldId, prefix, elemName);
                            html.Append(fieldVendor.Render());
                        }

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
                                                    case "src":
                                                        if (tagName == "img")
                                                        {
                                                            found = true;
                                                            //load image selection field
                                                            fieldImage.Clear();
                                                            fieldImage["title"] = fieldTitle;
                                                            fieldImage["id"] = fieldId;
                                                            fieldImage["value"] = fieldValue;
                                                            html.Append(fieldImage.Render());
                                                        }
                                                        break;
                                                }
                                            }
                                            catch (Exception) {}
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
                            fieldText["title"] = fieldTitle;
                            fieldText["id"] = fieldId;
                            fieldText["value"] = fieldValue;
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
                    var name = elem.Name
                        .Replace("!", "_1_")
                        .Replace("@", "_2_")
                        .Replace("#", "_3_")
                        .Replace("$", "_4_")
                        .Replace("%", "_5_")
                        .Replace("^", "_6_")
                        .Replace("&", "_7_")
                        .Replace("*", "_8_")
                        .Replace("(", "_9_")
                        .Replace(")", "_0_")
                        .Replace("+", "_p_")
                        .Replace("[", "_a_")
                        .Replace("{", "_b_")
                        .Replace("]", "_c_")
                        .Replace("}", "_d_")
                        .Replace("=", "_e_")
                        .Replace("|", "_f_")
                        .Replace("\\", "_g_")
                        .Replace(";", "_h_")
                        .Replace(":", "_i_")
                        .Replace("'", "_j_")
                        .Replace(",", "_k_")
                        .Replace("<", "_l_")
                        .Replace(".", "_m_")
                        .Replace(">", "_n_")
                        .Replace("/", "_o_")
                        .Replace("?", "_p_")
                        .Replace("\"", "_q_")
                        .Replace("`", "_r_")
                        .Replace(" ", "_s_")
                        .Replace("~", "_t_");
                    if (fields.ContainsKey(name) && fields[name] != "" && !results.ContainsKey(name))
                    {
                        results.Add(elem.Name, fields[name]);
                    }
                }
            }
        }
    }
}
