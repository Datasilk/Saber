using System.Text;

namespace Saber.Services
{
    public class ErrorLogs : Service
    {
        public string Render(int start = 0, int length = 50, string search = "")
        {
            //display all application settings
            if (IsPublicApiRequest || !CheckSecurity("website-settings")) { return AccessDenied(); }
            var view = new View("/Views/ErrorLogs/errorlogs.html");
            var viewItem = new View("/Views/ErrorLogs/log-item.html");
            var html = new StringBuilder();
            var errors = Query.Logs.Errors.GetList(start, length, search);
            foreach(var err in errors)
            {
                viewItem.Clear();
                viewItem["area"] = err.area;
                viewItem["message"] = err.message.Replace("\n", "<br/>");
                viewItem["stacktrace"] = err.stacktrace.Replace("\n", "<br/>");
                viewItem["data"] = err.data.Replace("\n", "<br/>");
                viewItem["url"] = string.IsNullOrEmpty(err.url) ? "No URL provided" : err.url;
                viewItem["datetime"] = err.datecreated.ToString("yyyy/MM/dd hh:mm tt");
                html.Append(viewItem.Render());
            }
            view["contents"] = html.ToString();

            //render view
            return JsonResponse(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".errorlogs-contents",
                    html = view.Render(),
                    css = Css.ToString(),
                    javascript = Scripts.ToString()
                }
            );
        }
    }
}
