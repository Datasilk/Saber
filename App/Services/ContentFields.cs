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
        public string Render(string path, string language, string container, Dictionary<string, string> data = null)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            var paths = PageInfo.GetRelativePath(path);
            var fields = data != null && data.Keys.Count > 0 ? data : Core.ContentFields.GetPageContent(path, language);
            var htmlVars = Common.Vendors.HtmlComponentKeys;
            var view = new View(string.Join("/", paths) + (path.Contains(".html") ? "" : ".html"));
            var section = new View("/Views/ContentFields/section.html");
            var fieldText = new View("/Views/ContentFields/text.html");
            var fieldBlock = new View("/Views/ContentFields/block.html");
            var fieldImage = new View("/Views/ContentFields/image.html");
            var fieldVendor = new View("/Views/ContentFields/vendor.html");

            var result = processView("", view, language, container, fields, section, fieldBlock, fieldText, fieldImage, fieldVendor, htmlVars);

            if (string.IsNullOrWhiteSpace(result))
            {
                var nofields = new View("/Views/ContentFields/nofields.html");
                nofields["filename"] = paths[paths.Length - 1];
                return Response(nofields.Render());
            }
            return Response( result);
        }

        private string processView(string title, View view, string language, string container, Dictionary<string, string> fields, View section, View fieldBlock, View fieldText, View fieldImage, View fieldVendor, string[] vars)
        {
            var html = new StringBuilder();
            var sectionTitle = "";
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
                        fieldBlock["title"] = fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize();
                        fieldBlock["id"] = fieldId;
                        if (fieldValue == "1") { fieldBlock.Show("checked"); }
                        html.Append(fieldBlock.Render());
                    }
                    else
                    {
                        var found = false;
                        //find vendor content field
                        var vendor = Common.Vendors.ContentFields.Where(a => elemName.IndexOf(a.Key) == 0).FirstOrDefault();
                        if(vendor.Value != null)
                        {
                            found = true;
                            //hard-code line break component
                            if(elem.Name == "-" && elem.Vars.ContainsKey("title"))
                            {
                                sectionTitle = elem.Vars["title"].ToLower();
                            }

                            if(vendor.Value.ReplaceRow == true)
                            {
                                html.Append(vendor.Value.ContentField.Render(this, elem.Vars ?? new Dictionary<string, string>(), fieldValue, fieldId, prefix, elemName, language, container));
                            }
                            else
                            {
                                fieldVendor.Clear();
                                string fieldTitleId = fieldTitle.Length > 1 ? fieldTitle.Replace(vendor.Key.Capitalize().Replace("-", " ").Replace("_", " "), "") : fieldTitle;

                                var fieldTitleKey = fieldTitle.ToLower();
                                if (!string.IsNullOrEmpty(fieldTitleId))
                                {
                                    fieldTitleKey = fieldTitleKey.Replace(fieldTitleId, "");
                                }
                                fieldTitleKey = fieldTitleKey.Capitalize();

                                fieldVendor["title"] = (fieldTitleKey != "" ? fieldTitleKey + ": " : "") + fieldTitleId.Trim().Capitalize();
                                fieldVendor["id"] = fieldId;
                                fieldVendor["value"] = fieldValueHtml;
                                fieldVendor["content"] = vendor.Value.ContentField.Render(this, elem.Vars ?? new Dictionary<string, string>(), fieldValue, fieldId, prefix, elemName, language, container);
                                html.Append(fieldVendor.Render());
                            }
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
                                                var isPhoto = false;
                                                switch (attrName)
                                                {
                                                    case "style":
                                                        //TODO: style parsing support to check if field exists in
                                                        //background or background-image CSS property
                                                        if(htmElem.IndexOf("background-image:url(") == htmElem.Length - 21 ||
                                                            htmElem.IndexOf("background-image: url(") == htmElem.Length - 22)
                                                        {
                                                            isPhoto = true;
                                                        }

                                                        break;
                                                    case "src":
                                                        if (tagName == "img")
                                                        {
                                                            isPhoto = true;
                                                        }
                                                        break;
                                                }
                                                if (isPhoto)
                                                {
                                                    found = true;
                                                    //load image selection field
                                                    fieldImage.Clear();
                                                    fieldImage["title"] = sectionTitle != "" ? fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize() : fieldTitle.Capitalize();
                                                    fieldImage["id"] = fieldId;
                                                    fieldImage["value"] = fieldValue;
                                                    html.Append(fieldImage.Render());
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
                            fieldText["title"] = sectionTitle != "" ? fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize() : fieldTitle.Capitalize();
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
            if(language == "") { language = "en"; }
            if (paths[1] == "partials")
            {
                var validated = new Dictionary<string, string>();
                ValidateField(string.Join("/", paths), fields, validated);
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
            }
            else
            {
                var config = PageInfo.GetPageConfig(path);
                var validated = new Dictionary<string, string>();
                ValidateField(string.Join("/", paths) + ".html", fields, validated);
                ValidateField("/Content/partials/" + config.header, fields, validated);
                ValidateField("/Content/partials/" + config.footer, fields, validated);
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
