namespace Saber
{
    public class Page : Datasilk.Page
    {
        public Page(global::Core DatasilkCore) : base(DatasilkCore) {
            title = "Saber";
            description = "You can do everything you ever wanted";
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (scripts.IndexOf("S.svg.load") < 0)
            {
                scripts += "<script language=\"javascript\">S.svg.load('/themes/default/icons.svg');</script>";
            }
            return base.Render(path, body, metadata);
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