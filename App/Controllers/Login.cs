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
                view["theme"] = Theme;
                if(Server.DeveloperKeys.Count > 0)
                {
                    view["apikey"] = Server.DeveloperKeys[0].Key;
                }
                else
                {
                    view.Show("no-api-key");
                }
                
                //AddScript("/editor/js/platform.js");
                AddScript("/editor/js/views/login/new-admin.js");
                UsePlatform = false;
                return base.Render(view.Render());
            }
            else
            {
                return AccessDenied();
            }
        }
    }
}
