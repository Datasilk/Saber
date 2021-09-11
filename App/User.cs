using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Saber.Core;

namespace Saber
{
    public class User : IUser
    {
        private bool changed = false;
        private HttpContext Context;
        private Session Session;

        public int UserId { get; set; } = 0;
        public string Email { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Photo { get; set; }
        public bool IsAdmin { get; set; } //if true, has full permissions
        public bool PublicApi { get; set; }
        public DateTime DateCreated { get; set; }
        public string Language { get; set; }
        public List<KeyValuePair<string, bool>> Keys { get; set; } = new List<KeyValuePair<string, bool>>();
        public int[] Groups { get; set; } = new int[] { };
        public bool ResetPass { get; set; }

        public static User Get(HttpContext context, Session session)
        {
            User user;
            var userstr = session.Get("user");
            if (!string.IsNullOrEmpty(userstr))
            {
                user = JsonSerializer.Deserialize<User>(session.Get("user"));
            }
            else
            {
                user = (User)new User().SetContext(context);
            }
            user.Init(context, session);
            return user;
        }

        public IUser SetContext(HttpContext context)
        {
            Context = context;
            return this;
        }

        public IUser SetSession(Session session)
        {
            Session = session;
            return this;
        }

        public void Init(HttpContext context, Session session)
        {
            //generate visitor id
            Context = context;
            Session = session;

            //check for persistant cookie
            if (UserId <= 0 && context.Request.Cookies.ContainsKey("authId"))
            {
                var user = Query.Users.Authenticate(context.Request.Cookies["authId"]);
                if (user != null)
                {
                    //persistant cookie was valid, log in
                    LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin);
                }
            }
        }

        public void Save(bool changed = false)
        {
            if (this.changed == true && changed == false)
            {
                Session.Set("user", JsonSerializer.Serialize(this));
                this.changed = false;
            }
            if (changed == true)
            {
                this.changed = true;
            }
        }

        public void LogIn(int userId, string email, string name, DateTime datecreated, bool photo = false, bool isAdmin = false, bool publicApi = false)
        {
            UserId = userId;
            Email = email;
            Name = name;
            Photo = photo;
            IsAdmin = isAdmin;
            DateCreated = datecreated;
            PublicApi = publicApi;

            if (!publicApi)
            {
                var keys = Query.Security.Keys.GetByUserId(userId);
                foreach (var key in keys)
                {
                    Keys.Add(new KeyValuePair<string, bool>(key.key, key.value));
                }
                var groups = Query.Security.Users.GetGroups(userId);
                if (groups != null && groups.Count > 0)
                {
                    Groups = groups.Select(a => a.groupId).ToArray();
                }

                //create persistant cookie
                var auth = Query.Users.CreateAuthToken(UserId);
                var options = new CookieOptions()
                {
                    Expires = DateTime.Now.AddMonths(1)
                };

                Context.Response.Cookies.Append("authId", auth, options);
            }

            changed = true;
        }

        public void LogOut()
        {
            UserId = 0;
            Email = "";
            Name = "";
            Photo = false;
            IsAdmin = false;
            changed = true;
            Context.Response.Cookies.Delete("authId");
            Save();
        }

        public void SetLanguage(string language)
        {
            Language = language;
            changed = true;
        }

        #region "Editor UI"
        public string[] GetOpenTabs()
        {
            //gets a list of open tabs within the Editor UI
            var opentabs = Session.Get("open-tabs");
            if (!string.IsNullOrEmpty(opentabs))
            {
                return JsonSerializer.Deserialize<string[]>(Session.Get("open-tabs"));
            }
            else
            {
                return new string[] { };
            }
        }

        public void SaveOpenTabs(string[] tabs)
        {
            Session.Set("open-tabs", JsonSerializer.Serialize(tabs));
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

        #region "Helpers"

        protected static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return string.Join("", chars);
        }

        protected static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        #endregion
    }
}
