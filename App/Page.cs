using Microsoft.AspNetCore.Http;

namespace Saber
{
    public enum EditorType
    {
        Monaco = 0,
        Ace = 1
    }

    public class Page : Datasilk.Page
    {
        public bool usePlatform = false;
        public string theme = "default";

        //constructor
        public Page(HttpContext context) : base(context) {
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
            var scaffold = new Scaffold("/Views/Shared/layout.html", Server.Scaffold);
            scaffold.Data["title"] = title;
            scaffold.Data["description"] = description;
            scaffold.Data["language"] = User.language;
            scaffold.Data["head-css"] = headCss.ToString();
            scaffold.Data["theme"] = theme;
            scaffold.Data["favicon"] = favicon;
            scaffold.Data["body"] = body;
            scaffold.Data["platform-1"] = usePlatform == true ? "1" : "";
            scaffold.Data["platform-2"] = usePlatform == true ? "1" : "";
            scaffold.Data["platform-3"] = usePlatform == true ? "1" : "";

            //add initialization script
            scaffold.Data["scripts"] = scripts.ToString();

            return scaffold.Render();
        }
    }
}