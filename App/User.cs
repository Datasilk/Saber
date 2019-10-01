using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;

namespace Datasilk
{
    public partial class User
    {
        public string language = "en";

        public void SetLanguage(string language)
        {
            this.language = language;
            changed = true;
        }

        partial void VendorInit()
        {
            //check for persistant cookie
            if (userId <= 0 && context.Request.Cookies.ContainsKey("authId"))
            {
                var user = Query.Users.AuthenticateUser(context.Request.Cookies["authId"]);
                if(user != null)
                {
                    //persistant cookie was valid, log in
                    LogIn(user.userId, user.email, user.name, user.datecreated, "", 1, user.photo);
                }
            }
        }

        partial void VendorLogIn()
        {
            //create persistant cookie
            var auth = Query.Users.CreateAuthToken(userId);
            var options = new CookieOptions()
            {
                Expires = DateTime.Now.AddMonths(1)
            };

            context.Response.Cookies.Append("authId", auth, options);
        }

        partial void VendorLogOut()
        {
            context.Response.Cookies.Delete("authId");
        }

        #region "Editor UI"
        public string[] GetOpenTabs()
        {
            //gets a list of open tabs within the Editor UI
            if (context.Session.Get("open-tabs") != null)
            {
                return (string[])Serializer.ReadObject(context.Session.Get("open-tabs").GetString(), typeof(string[]));
            }
            else
            {
                return new string[] { };
            }
        }

        public void SaveOpenTabs(string[] tabs)
        {
            context.Session.Set("open-tabs", Serializer.WriteObject(tabs));
        }

        public void AddOpenTab(string filePath)
        {
            var tabs = GetOpenTabs().ToList();
            if (!tabs.Contains(filePath))
            {
                tabs.Add(filePath);
            }
            SaveOpenTabs(tabs.ToArray());
        }

        public void RemoveOpenTab(string filePath)
        {
            var tabs = GetOpenTabs().ToList();
            if (tabs.Contains(filePath))
            {
                tabs.Remove(filePath);
            }
            SaveOpenTabs(tabs.ToArray());
        }
        #endregion
    }
}
