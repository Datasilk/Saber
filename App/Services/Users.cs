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
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/Users/list.html");
            var listitem = new View("/Views/Users/list-item.html");
            var users = Query.Users.GetList(start, length, search);


            return view.Render();
        }

        public string Details(string userId)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/Users/details.html");
            var viewScope = new View("/Views/Security/scope.html");
            var viewKey = new View("/Views/Security/key.html");

            return view.Render();
        }
    }
}
