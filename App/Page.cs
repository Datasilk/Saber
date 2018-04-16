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
        private User _userInfo;

        public Page(global::Core DatasilkCore) : base(DatasilkCore) {
            title = "Saber";
            description = "You can do everything you ever wanted";
        }

        public User UserInfo
        {
            get {
                if (_userInfo == null) { _userInfo = new User(S); }
                return _userInfo;
            }
        }

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
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
            scaffold.Data["language"] = UserInfo.language;
            scaffold.Data["head-css"] = headCss;
            scaffold.Data["theme"] = theme;
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
                var child = scaffold.Child("header");
                child.Data["user"] = "1";
                child.Data["username"] = S.User.name;
                
            }
            else
            {
                scaffold.Child("header").Data["no-user"] = "1";
            }
        }
    }
}