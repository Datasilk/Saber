using System.Text;
using System.Linq;

namespace Saber.Services
{
    public class Security : Service
    {
        public string Groups()
        {
            if (User.PublicApi || !CheckSecurity("manage-security")) { return AccessDenied(); }
            var view = new View("/Views/Security/groups.html");
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
            if (User.PublicApi || !CheckSecurity("manage-security")) { return AccessDenied(); }
            if (Query.Security.Groups.Exists(name)) { return Error("Group \"" + name + "\" already exists"); }
            Query.Security.Groups.Create(name);
            return Success();
        }

        public string GroupDetails(int groupId)
        {
            if (User.PublicApi || !CheckSecurity("manage-security")) { return AccessDenied(); }
            var view = new View("/Views/Security/details.html");
            var viewScope = new View("/Views/Security/scope.html");
            var viewKey = new View("/Views/Security/key.html");
            var html = new StringBuilder();
            var scopes = new StringBuilder();
            var groupkeys = Query.Security.Keys.GetList(groupId);

            //show platform-specific keys
            viewScope["label"] = "Saber Editor";
            foreach(var key in Core.Security.Keys)
            {
                viewKey["key"] = key.Value;
                viewKey["label"] = key.Label;
                viewKey["description"] = key.Description;
                if(groupkeys.Any(a => a.key == key.Value && a.value == true))
                {
                    viewKey.Show("checked");
                }
                html.Append(viewKey.Render());
                viewKey.Clear();
            }
            viewScope["keys"] = html.ToString();
            scopes.Append(viewScope.Render());
            viewScope.Clear();

            //add vendor-specific keys
            foreach(var vendor in Core.Vendors.Keys)
            {
                html.Clear();
                viewScope["label"] = vendor.Vendor;
                foreach (var key in vendor.Keys)
                {
                    viewKey["key"] = key.Value;
                    viewKey["label"] = key.Label;
                    viewKey["description"] = key.Description;
                    if (groupkeys.Any(a => a.key == key.Value && a.value == true))
                    {
                        viewKey.Show("checked");
                    }
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

        public string SaveKey(int groupId, string key, bool value)
        {
            if (User.PublicApi || !CheckSecurity("manage-security")) { return AccessDenied(); }
            var isplatform = true;
            var seckey = Core.Security.Keys.Where(a => a.Value == key).FirstOrDefault();
            if(seckey == null)
            {
                isplatform = false;
                seckey = Core.Vendors.Keys.Where(a => a.Keys.Any(b => b.Value == key)).FirstOrDefault()
                    ?.Keys.Where(a => a.Value == key).FirstOrDefault();
            }
            if(seckey == null) { return Error("could not find security key"); }
            Query.Security.Keys.Create(groupId, key, value, isplatform);
            return Success();
        }

        public string DeleteGroup(int groupId)
        {
            if (User.PublicApi || !CheckSecurity("manage-security")) { return AccessDenied(); }
            Query.Security.Groups.Delete(groupId);
            return Success();
        }
    }
}
