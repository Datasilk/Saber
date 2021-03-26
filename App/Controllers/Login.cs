namespace Saber.Controllers
{
    public class Login: Controller
    {
        public override string Render(string body = "")
        {
            if (Server.HasAdmin == false)
            {
                //load new administrator form
                var view = new View("/Views/Login/new-admin.html");
                view["title"] = "Create an administrator account";
                AddScript("/editor/js/platform.js");
                AddScript("/editor/js/views/login/new-admin.js");
                return base.Render(view.Render());
            }
            else
            {
                //login form for 3rd-party authentication
                var view = new View("/Views/Login/login.html");
                AddScript("/editor/js/platform.js");
                AddScript("/editor/js/views/login/login.js");
                AddScript("<script>var clientId = '" + Parameters["client_id"] + ";</script>");
                return view.Render();
            }
        }
    }
}
