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
            var view = new View("/Views/Security/details.html");
            var viewScope = new View("/Views/Security/scope.html");
            var viewKey = new View("/Views/Security/key.html");
            var html = new StringBuilder();
            var scopes = new StringBuilder();

            //show platform-specific keys
            viewScope["label"] = "Saber Editor";
            foreach(var key in Core.Security.Keys)
            {
                viewKey["key"] = key.Value;
                viewKey["label"] = key.Label;
                viewKey["description"] = key.Description;
                html.Append(viewKey.Render());
                viewKey.Clear();
            }
            viewScope["keys"] = html.ToString();
            scopes.Append(viewScope.Render());
            viewScope.Clear();

            //add vendor-specific keys
            foreach(var vendor in Vendors.Keys)
            {
                html.Clear();
                viewScope["label"] = vendor.Vendor;
                foreach (var key in vendor.Keys)
                {
                    viewKey["key"] = key.Value;
                    viewKey["label"] = key.Label;
                    viewKey["description"] = key.Description;
                    html.Append(viewKey.Render());
                    viewKey.Clear();
                }
                viewScope["keys"] = html.ToString();
                scopes.Append(viewScope.Render());
                viewScope.Clear();
            }

            view["scopes"] = scopes.ToString();
            return view.Render();
        }
    }
}
