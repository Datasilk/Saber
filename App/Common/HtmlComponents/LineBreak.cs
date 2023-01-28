﻿using System;
using System.Collections.Generic;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.HtmlComponents
{
    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class LineBreak : IVendorHtmlComponents
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
                new HtmlComponentModel()
                {
                    Key = "#",
                    Name = "Page Content Title",
                    Block = false,
                    Description = "Used to separate groups of content fields by creating a line break & heading element within the Content Fields form.",
                    Render = new Func<View, IRequest, Dictionary<string, string>, Dictionary<string, object>, string, string, List<KeyValuePair<string, string>>>((view, request, args, data, prefix, key) =>
                    {
                        var results = new List<KeyValuePair<string, string>>();
                        results.Add(new KeyValuePair<string, string>(prefix + "#", ""));
                        return results;
                    })
                }
            };
        }
    }
}