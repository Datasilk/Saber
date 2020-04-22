using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Saber.Common.Utility;

namespace Saber.Services
{
    public class Analytics : Service
    {
        public string Render()
        {
            //show website analytics
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/Analytics/analytics.html");

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
