using System;
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
                }
            };
        }
    }
}