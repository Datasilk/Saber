using System;
using System.Collections.Generic;
using System.Linq;
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
            return view.Render();
        }

        public string Details(string userId)
        {
            if (!CheckSecurity("manage-users")) { return AccessDenied(); }
            var view = new View("/Views/Users/details.html");
            var viewScope = new View("/Views/Security/scope.html");
            var viewKey = new View("/Views/Security/key.html");

            return view.Render();
        }
    }
}
