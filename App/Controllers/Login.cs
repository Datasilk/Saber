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
                AddScript("/editor/js/views/login/new-admin.js");
                return base.Render(view.Render());
            }
            return "";
        }
    }
}
