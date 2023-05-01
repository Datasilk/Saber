namespace Saber.Controllers
{
    public class Activate: Controller
    {
        public override string Render(string body = "")
        {
            if (Server.HasAdmin == true)
            {
                if(Parameters.ContainsKey("key") && Query.Users.Activate(Parameters["key"]))
                {
                    //user activated
                    return Redirect("/login?activated");
                }
                else
                {
                    return Error("Activation key invalid or expired");
                }
            }
            else
            {
                return AccessDenied();
            }
        }
    }
}
