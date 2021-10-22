using System;
using System.Text;

namespace Saber.Services
{
    public class Users : Service
    {
        public string List(int start = 1, int length = 25, string search = "", int orderby = 2)
        {
            if (IsPublicApiRequest || !CheckSecurity("manage-users")) { return AccessDenied(); }
            var view = new View("/Views/Users/users.html");
            var list = new View("/Views/Users/list.html");
            var listitem = new View("/Views/Users/list-item.html");
            var users = Query.Users.GetList(start, length, search);
            var lists = new StringBuilder();
            var html = new StringBuilder();
            var showAdmins = false;
            foreach(var user in users)
            {
                if(showAdmins == false && user.userId != 1 && user.security == 0)
                {
                    if (html.Length > 0)
                    {
                        //show admins
                        list["users-type"] = "Administrators";
                        list["users"] = html.ToString();
                        lists.Append(list.Render());
                        html.Clear();
                        list.Clear();
                    }
                    showAdmins = true;
                }
                listitem.Clear();
                listitem.Bind(new { user });
                html.Append(listitem.Render());
            }
            if(html.Length > 0 && showAdmins == false)
            {
                list["users-type"] = "Administrators";
                list["users"] = html.ToString();
                lists.Append(list.Render());
            }
            else if(html.Length > 0)
            {
                list["users-type"] = "Members";
                list["users"] = html.ToString();
                lists.Append(list.Render());
            }
            view["lists"] = lists.ToString();

            //get list of security groups that can be assigned to users
            var groups = Query.Security.Groups.GetList();
            html.Clear();
            foreach(var group in groups)
            {
                html.Append("<option value=\"" + group.groupId + "\">" + group.name + "</option>");
            }
            view["group-list"] = html.ToString();

            return view.Render();
        }

        public string Details(int userId)
        {
            if (IsPublicApiRequest || !CheckSecurity("manage-users")) { return AccessDenied(); }
            var view = new View("/Views/Users/details.html");
            var user = Query.Users.GetDetails(userId);
            if(user.enabled == true)
            {
                view.Show("isenabled");
            }
            if (!user.isadmin)
            {
                //non-admins only
                view["group-list"] = AssignedGroups(userId);
                view.Show("can-assign");
            }
            else
            {
                //administrators only
                view["group-list"] = Cache.LoadFile(App.MapPath("/Views/Users/admin-group.html"));
            }
            if (User.IsAdmin)
            {
                //allow admin to assign users as admins
                view.Show("can-assign-admin");
                view.Show("can-enable"); 
                if (user.isadmin) { view["is-admin"] = "checked=\"checked\""; }
                if (user.enabled) { view["is-enabled"] = "checked=\"checked\""; }

            }
            
            view.Bind(new { user });
            return view.Render();
        }

        public string Update(int userId, string email, string name, bool enabled, bool isadmin)
        {
            if (IsPublicApiRequest || !CheckSecurity("manage-users")) { return AccessDenied(); }
            var user = Query.Users.GetDetails(userId);
            if(user.email != email)
            {
                //TODO: send user an email to their new email address to verify their account
            }
            if(user.enabled != enabled)
            {
                if(enabled == true)
                {
                    Query.Users.Enable(userId);
                }
                else
                {
                    Query.Users.Disable(userId);
                }
            }
            if (User.IsAdmin && user.isadmin != isadmin)
            {
                Query.Users.UpdateAdmin(userId, isadmin);
            }
            if(user.name != name)
            { 
                Query.Users.UpdateName(userId, name);
            }
            return Success();
        }

        public string AssignGroup(int groupId, int userId)
        {
            if (IsPublicApiRequest || !CheckSecurity("manage-users")) { return AccessDenied(); }
            Query.Security.Users.Add(groupId, userId);
            return Success();
        }

        public string RemoveGroup(int groupId, int userId)
        {
            if (IsPublicApiRequest || !CheckSecurity("manage-users")) { return AccessDenied(); }
            Query.Security.Users.Remove(groupId, userId);
            return Success();
        }

        public string AssignedGroups(int userId)
        {
            if (IsPublicApiRequest || !CheckSecurity("manage-users")) { return AccessDenied(); }
            var groups = Query.Security.Users.GetGroups(userId);
            var view = new View("/Views/Users/group-item.html");
            var html = new StringBuilder();
            if(groups.Count == 0)
            {
                html.Append(Cache.LoadFile(App.MapPath("/Views/Users/no-groups.html")));
            }
            else
            {
                foreach (var group in groups)
                {
                    view.Bind(new { group });
                    html.Append(view.Render());
                    view.Clear();
                }
            }
            return html.ToString();
        }

        public string RenderSettings()
        {
            if (!CheckSecurity("users-settings")) { return AccessDenied(); }
            var view = new View("/Views/Users/users-settings.html");
            var groups = Query.Security.Groups.GetList();
            var html = new StringBuilder();
            var settings = Common.Platform.Website.Settings.Load();
            foreach(var group in groups)
            {
                html.Append("<option value=\"" + group.groupId + "\"" + 
                    (settings.Users.groupId == group.groupId ? " selected=\"selected\"" : "") + ">" + group.name + "</option>\n");
            }
            view["security-group-options"] = html.ToString();
            view["max-signups"] = settings.Users.maxSignups.HasValue && settings.Users.maxSignups.Value > -1 ?  
                settings.Users.maxSignups.Value.ToString() : "";
            return view.Render();
        }

        public string UpdateSettings(string groupId, string maxSignups, string maxSignupsMinutes)
        {
            if (!CheckSecurity("users-settings-update")) { return AccessDenied(); }
            try
            {
                //open website settings json
                var settings = Common.Platform.Website.Settings.Load();
                settings.Users.groupId = string.IsNullOrEmpty(groupId) ? 0 : int.Parse(groupId);
                settings.Users.maxSignups = string.IsNullOrEmpty(maxSignups) ? -1 : int.Parse(maxSignups);
                settings.Users.maxSignupsMinutes = string.IsNullOrEmpty(maxSignupsMinutes) ? -1 : int.Parse(maxSignupsMinutes);
                Common.Platform.Website.Settings.Save(settings);
            }
            catch (Exception)
            {
                return Error();
            }
            return Success();
        }

        #region "Helpers"

        private string EncryptPassword(string email, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(email + Server.Salt + password, Server.BcryptWorkfactor);

        }

        private bool DecryptPassword(string email, string password, string encrypted)
        {
            return BCrypt.Net.BCrypt.Verify(email + Server.Salt + password, encrypted);
        }

        #endregion
    }
}
