namespace Saber.Pages.DashboardPages
{
    public class Timeline: Page
    {
        public Timeline(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            //load timeline
            var scaffold = new Scaffold("/Pages/Dashboard/Timeline/timeline.html", S.Server.Scaffold);
            return scaffold.Render();
        }
    }
}
