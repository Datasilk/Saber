using Microsoft.AspNetCore.Http;

namespace Saber.Pages
{
    public class Login: Page
    {
        public Login(HttpContext context) : base(context)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if(User.userId > 0)
            {
                //redirect to dashboard
                return base.Render(path, Redirect("/dashboard/"));
            }

            //check for database reset
            var scaffold = new Scaffold("/Pages/Login/login.html", server.Scaffold);

            if(server.environment == Server.enumEnvironment.development && server.hasAdmin == false)
            {
                //load new administrator form
                scaffold = new Scaffold("/Pages/Login/new-admin.html", server.Scaffold);
                scaffold.Data["title"] = "Create an administrator account";
                scripts.Append("<script src=\"/js/pages/login/new-admin.js\"></script>");
            }
            else if (User.resetPass == true)
            {
                //load new password form (for admin only)
                scaffold = new Scaffold("/Pages/Login/new-pass.html", server.Scaffold);
                scaffold.Data["title"] = "Create an administrator password";
                scripts.Append("<script src=\"/js/pages/login/new-pass.js\"></script>");
            }
            else
            {
                //load login form (default)
                scripts.Append("<script src=\"/js/pages/login/login.js\"></script>");
            }

            //load login page
            usePlatform = true;
            return base.Render(path, scaffold.Render());
        }
    }
}
