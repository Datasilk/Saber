using System.Text;

namespace Saber.Services
{
    public class Users : Service
    {
        public string List(int start = 1, int length = 25, string search = "", int orderby = 2)
        {
            if (!CheckSecurity("manage-users")) { return AccessDenied(); }
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
            if (!CheckSecurity("manage-users")) { return AccessDenied(); }
            var view = new View("/Views/Users/details.html");
            var user = Query.Users.GetDetails(userId);
            if(userId != 1)
            {
                view["group-list"] = AssignedGroups(userId);
                view.Show("can-assign");
            }
            else
            {
                view["group-list"] = Cache.LoadFile(App.MapPath("/Views/Users/admin-group.html"));
            }
            
            view.Bind(new { user });
            return view.Render();
        }

        public string AssignGroup(int groupId, int userId)
        {
            if (!CheckSecurity("manage-users")) { return AccessDenied(); }
            Query.Security.Users.Add(groupId, userId);
            return Success();
        }

        public string RemoveGroup(int groupId, int userId)
        {
            if (!CheckSecurity("manage-users")) { return AccessDenied(); }
            Query.Security.Users.Remove(groupId, userId);
            return Success();
        }

        public string AssignedGroups(int userId)
        {
            if (!CheckSecurity("manage-users")) { return AccessDenied(); }
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
    }
}
