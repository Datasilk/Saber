namespace Saber.Controllers
{
    public class Login: Controller
    {
        public override string Render(string body = "")
        {
            if (Server.HasAdmin == false)
            {
                //load new administrator form
                Theme = "dark";
                UsePlatform = true;
                var view = new View("/Views/Login/new-admin.html");
                view["title"] = "Create an administrator account";
                Scripts.Append("<script src=\"/editor/js/views/login/new-admin.js\"></script>");
                return base.Render(view.Render());
            }
            return "";
        }
    }
}
