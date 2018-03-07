namespace Saber
{
    public class Page : Datasilk.Page
    {
        public bool usePlatform = false;
        public string language = "en";

        public Page(global::Core DatasilkCore) : base(DatasilkCore) {
            title = "Saber";
            description = "You can do everything you ever wanted";
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (scripts.IndexOf("S.svg.load") < 0 && usePlatform == true)
            {
                scripts += "<script language=\"javascript\">S.svg.load('/themes/default/icons.svg');</script>";
            }
            var scaffold = new Scaffold("/layout.html", S.Server.Scaffold);
            scaffold.Data["title"] = title;
            scaffold.Data["description"] = description;
            scaffold.Data["language"] = language;
            scaffold.Data["head-css"] = headCss;
            scaffold.Data["favicon"] = favicon;
            scaffold.Data["body"] = body;
            scaffold.Data["platform-1"] = usePlatform == true ? "1" : "";
            scaffold.Data["platform-2"] = usePlatform == true ? "1" : "";
            scaffold.Data["platform-3"] = usePlatform == true ? "1" : "";

            //add initialization script
            scaffold.Data["scripts"] = scripts;

            return scaffold.Render();
        }

        public void LoadHeader(ref Scaffold scaffold)
        {
            if(S.User.userId > 0)
            {
                scaffold.Child("header").Data["user"] = "1";
            }
            else
            {
                scaffold.Child("header").Data["no-user"] = "1";
            }
        }
    }
}