using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.Platform
{
    public static class ContentFields
    {
        /// <summary>
        /// Generate an HTML form for all content fields within a partial view.
        /// </summary>
        /// <param name="request">The currente IRequest object</param>
        /// <param name="title">Title of the form</param>
        /// <param name="view">Partial view to collect mustache variables from</param>
        /// <param name="language">Selected language used to pass into all vendor HTML Components found in the partial view</param>
        /// <param name="container">CSS selector of the HTML container that this form will be injected into. This field is passed into all vendor HTML Components found in the partial view.</param>
        /// <param name="fields">The values associated with each mustache variable in the partial view.</param>
        /// <returns>An HTML string representing the content fields form</returns>
        public static string RenderForm(Core.IRequest request, string title, View view, string language, string container, Dictionary<string, string> fields, string[] excludedFields = null, Dictionary<string, Core.ContentFields.FieldType> fieldTypes = null)
        {
            var section = new View("/Views/ContentFields/section.html");
            var fieldText = new View("/Views/ContentFields/text.html");
            var fieldNumber = new View("/Views/ContentFields/number.html");
            var fieldBlock = new View("/Views/ContentFields/block.html");
            var fieldImage = new View("/Views/ContentFields/image.html");
            var fieldVendor = new View("/Views/ContentFields/vendor.html");
            var vars = Core.Vendors.HtmlComponentKeys;
            var html = new StringBuilder();
            var sections = new StringBuilder();
            var sectionTitle = "";
            var keys = new List<string>();
            var fieldElementInfo = new List<Models.ContentFieldElementInfo>();

            if(excludedFields == null) { excludedFields = new string[] { }; }

            for (var x = 0; x < view.Elements.Count; x++)
            {
                var elem = view.Elements[x];
                fieldElementInfo.Add(new Models.ContentFieldElementInfo(){
                    Type = Core.ContentFields.FieldType.text 
                });
                var elemInfo = fieldElementInfo[^1];
                if (elem.Name == "" || elem.Name.Substring(0, 1) == "/") { continue; }

                //get element name with no partial file prefixes
                var elemName = elem.Name;
                var prefix = "";
                foreach (var partial in view.Partials)
                {
                    elemName = elemName.Replace(partial.Prefix, "");
                }

                //validate element
                if(excludedFields.Any(a => a == elemName)){ continue; }
                if (keys.Contains(elemName)) { continue; }
                if(!Core.Vendors.HtmlComponents.Any(a => a.Value.Key == elemName))
                {
                    //allow multiple content fields for HTML Components but not for mustache variables
                    keys.Add(elemName);
                }

                //do not display content fields
                if (Core.Vendors.HtmlComponents.Any(a => a.Value.Key == elemName && a.Value.ContentField == false)) { continue; }

                //get partial view prefix from element name
                prefix = elem.Name.Replace(elemName, "");
                if (prefix == elemName) { prefix = ""; }

                //clean field title & field Id
                var fieldTitle = elemName.Capitalize().Replace("-", " ").Replace("_", " ");
                var fieldId = GetFieldId(elem.Name);

                //get existing content for field
                var fieldValue = "";
                if (fields.ContainsKey(elem.Name))
                {
                    fieldValue = fields[elem.Name];
                }
                var fieldValueHtml = fieldValue.Replace("\"", "&quot;");
                var fieldType = fieldTypes != null && fieldTypes.ContainsKey(elem.Name) ? fieldTypes[elem.Name] : GetFieldType(view, x, fieldElementInfo);
                elemInfo.Type = fieldType;

                switch (fieldType)
                {
                    case Core.ContentFields.FieldType.block:
                        //load block field
                        fieldBlock.Clear();
                        fieldBlock["title"] = sectionTitle != "" ? fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize() : fieldTitle.Capitalize();
                        fieldBlock["id"] = fieldId;
                        if (fieldValue == "1") { fieldBlock.Show("checked"); }
                        html.Append(fieldBlock.Render());
                        break;
                    case Core.ContentFields.FieldType.image:
                        //image field
                        fieldImage.Clear();
                        fieldImage["title"] = sectionTitle != "" ? fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize() : fieldTitle.Capitalize();
                        fieldImage["id"] = fieldId;
                        fieldImage["value"] = fieldValue;
                        html.Append(fieldImage.Render());
                        break;
                    case Core.ContentFields.FieldType.text:
                        //text field
                        fieldText.Clear();
                        fieldText["title"] = sectionTitle != "" ? fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize() : fieldTitle.Capitalize();
                        fieldText["id"] = fieldId;
                        fieldText["value"] = fieldValue;
                        html.Append(fieldText.Render());
                        break;
                    case Core.ContentFields.FieldType.number:
                        //number field
                        fieldNumber.Clear();
                        fieldNumber["title"] = sectionTitle != "" ? fieldTitle.ToLower().Replace(sectionTitle, "").Trim().Capitalize() : fieldTitle.Capitalize();
                        fieldNumber["id"] = fieldId;
                        fieldNumber["value"] = fieldValue;
                        html.Append(fieldNumber.Render());
                        break;
                    case Core.ContentFields.FieldType.vendor:
                    case Core.ContentFields.FieldType.linebreak:
                    case Core.ContentFields.FieldType.list:
                        //vendor HTML component
                        var vendor = Core.Vendors.ContentFields.Where(a => elemName.IndexOf(a.Key) == 0).FirstOrDefault();
                        if (fieldType == Core.ContentFields.FieldType.linebreak && elem.Var.Length > 0)
                        {
                            if(html.Length > 0)
                            {
                                section["fields"] = html.ToString();
                                html.Clear();
                                sections.Append(section.Render());
                            }
                            sectionTitle = elem.Var.ToLower();
                            section.Clear();
                            section["title"] = sectionTitle.Capitalize();
                            if(sectionTitle != "") { section.Show("has-title"); }
                        }

                        if (vendor.Value.ReplaceRow == true)
                        {
                            html.Append(vendor.Value.ContentField.Render(request, elem.Vars ?? new Dictionary<string, string>() { ["var"] = elem.Var }, fieldValue, fieldId, prefix, elemName, language, container));
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
                            fieldVendor["content"] = vendor.Value.ContentField.Render(request, elem.Vars ?? new Dictionary<string, string>() { ["var"] = elem.Var}, fieldValue, fieldId, prefix, elemName, language, container);
                            html.Append(fieldVendor.Render());
                        }
                        break;
                    default: continue;
                }
                html.Append("\n\n<hr/>\n\n");
            }
            section["fields"] = html.ToString();
            html.Clear();
            sections.Append(section.Render());
            return sections.Length == 0 ? "" : sections.ToString();
        }
        
        public static Core.ContentFields.FieldType GetFieldType(View view, int index, List<Models.ContentFieldElementInfo> elemsInfo)
        {
            var elem = view.Elements[index];
            if (elem.isBlock)
            {
                return Core.ContentFields.FieldType.block;
            }
            else
            {
                var elemName = elem.Name;
                foreach (var partial in view.Partials)
                {
                    elemName = elemName.Replace(partial.Prefix, "");
                }
                //find vendor content field
                var vendor = Core.Vendors.ContentFields.Where(a => elemName.IndexOf(a.Key) == 0).FirstOrDefault();
                if (vendor.Value != null)
                {
                    //hard-code line break component
                    if (elem.Name == "#" && elem.Var.Length > 0)
                    {
                        return Core.ContentFields.FieldType.linebreak;
                    }else if(elem.Name.IndexOf("list") == 0)
                    {
                        return Core.ContentFields.FieldType.list;
                    }
                    else
                    {
                        return Core.ContentFields.FieldType.vendor;
                    }
                }
                
                //check to see if content field is inside an HTML element
                if (index > 0)
                {
                    var prev = view.Elements[index - 1];
                    var prevElemInfo = elemsInfo[index - 1];
                    var inQuotes = false;
                    var quotes = 0;

                    if (prevElemInfo.Type == Core.ContentFields.FieldType.image)
                    {
                        if(prev.Html.IndexOf("\"") < 0)
                        {
                            return Core.ContentFields.FieldType.image;
                        }
                        quotes = 0;
                    }

                    for (var i = prev.Html.Length - 1; i >= 0; i--)
                    {
                        if (prev.Html[i] == '"') { quotes++; }
                        if (prev.Html[i] == '=' && quotes == 1) { inQuotes = true; }
                        if (prev.Html[i] == '>') { break; }
                        if (prev.Html[i] == '<')
                        {
                            //found html element
                            if (inQuotes == true)
                            {
                                //content field exists inside an HTML element attribute value
                                try
                                {
                                    var htmElem = prev.Html.Substring(i + 1);
                                    var tagParts = htmElem.Split(" ");
                                    var tagName = tagParts[0].ToLower();
                                    var attrName = tagParts[^1].Split("=")[0];
                                    switch (attrName)
                                    {
                                        case "style":
                                            //style parsing support to check if field exists in
                                            //background or background-image CSS property
                                            if (htmElem.IndexOf("background-image:url(") == htmElem.Length - 21 ||
                                                htmElem.IndexOf("background-image: url(") == htmElem.Length - 22)
                                            {
                                                return Core.ContentFields.FieldType.image;
                                            }

                                            break;
                                        case "src":
                                            if (tagName == "img")
                                            {
                                                return Core.ContentFields.FieldType.image;
                                            }
                                            break;
                                    }
                                }
                                catch (Exception) { }
                            }
                            break;
                        }
                    }
                }

                //last resort, field is text
                return Core.ContentFields.FieldType.text;
            }
        }

        public static string GetFieldId(string elemName)
        {
            return "field_" + elemName
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
        }
    }
}
