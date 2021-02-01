using System;
using System.Linq;

namespace Saber.Common.Platform
{
    public static class ContentFields
    {


        public static Core.ContentFields.FieldType GetFieldType(View view, int index)
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
                var vendor = Vendors.ContentFields.Where(a => elemName.IndexOf(a.Key) == 0).FirstOrDefault();
                if (vendor.Value != null)
                {
                    //hard-code line break component
                    if (elem.Name == "-" && elem.Vars.ContainsKey("title"))
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
    }
}
