using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Saber.Services
{
    public class Analytics : Service
    {
        public string Render(int timeScale = 2, DateTime? startDate = null)
        {
            //show website analytics
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/Analytics/analytics.html");

            if (!startDate.HasValue)
            {
                startDate = DateTime.Now;
                var hours = -1;
                switch (timeScale)
                {
                    case 1: hours = -24; break;
                    case 2: hours = -24 * 7; break;
                    case 3: hours = -24 * 30; break;
                    case 4: hours = -24 * 365; break;
                }
                startDate = startDate.Value.AddHours(hours);
            }
            var html = new StringBuilder();
            var analytics = Query.Logs.GetUrlAnalytics((Query.Logs.TimeScale)timeScale, startDate);
            if(analytics != null && analytics.Count > 0)
            {
                var urlItem = new View("/Views/Analytics/url-item.html");
                foreach(var item in analytics.GroupBy(a => new { a.url, a.urlId })
                    .Select(g => new { g.Key.url, g.Key.urlId, total = g.Sum(a => a.total) }))
                {
                    urlItem["url"] = item.url;
                    urlItem["id"] = item.urlId.ToString();
                    urlItem["total"] = item.total.ToString();
                    html.Append(urlItem.Render());
                }
                view["url-list"] = html.ToString();
            }
            



            //render view
            return JsonSerializer.Serialize(
                new Datasilk.Core.Web.Response()
                {
                    selector = ".sections > .website-analytics .analytics-contents",
                    html = RenderView(view),
                    css = Css.ToString(),
                    javascript = Scripts.ToString()
                }
            );
        }
    }
}
