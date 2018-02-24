using System;
using System.Text;

namespace Saber.Pages
{
    public class Home : Page
    {
        public Home(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            //html.Append(Redirect("/login"));
            var scaffold = new Scaffold("/Pages/Home/home.html", S.Server.Scaffold);

            //load header since it was included in home.html
            LoadHeader(ref scaffold);

            //finally, render base page layout with home page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
