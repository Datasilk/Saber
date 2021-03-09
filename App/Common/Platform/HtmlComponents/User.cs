using System;
using System.Collections.Generic;
using System.Linq;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.HtmlComponents
{
    /// <summary>
    /// Define Saber-specific html variables
    /// </summary>
    public class User : IVendorHtmlComponents
    {
        public List<HtmlComponentModel> Bind()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return new List<HtmlComponentModel>(){
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
            };
        }
    }
}