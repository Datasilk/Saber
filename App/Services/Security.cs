using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saber.Services
{
    public class Security : Service
    {
        public string Groups()
        {
            if (!CheckSecurity("manage-security")) { return AccessDenied(); }
            var view = new View("/Views/Security/security.html");
            var listitem = new View("/Views/Security/group-item.html");
            var html = new StringBuilder();
            var groups = Query.Security.Groups.GetList();
            foreach(var group in groups)
            {
                listitem.Bind(new { group });
                html.Append(listitem.Render());
                listitem.Clear();
            }
            view["groups"] = html.ToString();
            return view.Render();
        }

        public string CreateGroup(string name)
        {
            if (!CheckSecurity("manage-security")) { return AccessDenied(); }
            if (Query.Security.Groups.Exists(name)) { return Error("Group \"" + name + "\" already exists"); }
            Query.Security.Groups.Create(name);
            return Success();
        }

        public string GroupDetails(int groupId)
        {
            if (!CheckSecurity("manage-security")) { return AccessDenied(); }

            return Error();
        }
    }
}
