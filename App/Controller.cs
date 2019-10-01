using Microsoft.AspNetCore.Http;

namespace Saber
{
    public enum EditorType
    {
        Monaco = 0,
        Ace = 1
    }

    public class Controller : Datasilk.Mvc.Controller
    {
        public bool usePlatform = false;
        public string theme = "default";

        public Controller(HttpContext context, Parameters parameters) : base(context, parameters)
        {
            title = "Saber";
            description = "You can do everything you ever wanted";
        }

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (usePlatform == true)
            {
                scripts.Append("<script language=\"javascript\">S.svg.load('/themes/default/icons.svg');</script>");
            }
            var scaffold = new Scaffold("/Views/Shared/layout.html");
            scaffold["title"] = title;
            scaffold["description"] = description;
            scaffold["language"] = User.language;
            scaffold["theme"] = theme;
            scaffold["head-css"] = css.ToString();
            scaffold["favicon"] = favicon;
            scaffold["body"] = body;
            if (usePlatform)
            {
                scaffold.Show("platform-1");
                scaffold.Show("platform-2");
                scaffold.Show("platform-3");
            }

            //add initialization script
            scaffold["scripts"] = scripts.ToString();

            return scaffold.Render();
        }
    }
}