using Saber.Core;

namespace Saber.Services
{
    public class Page : Service
    {
        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public string Render(string path, string language = "en")
        {
            if (User.PublicApi) { return AccessDenied(); }
            try
            {
                return Response(Common.Platform.Render.Page(path, this, PageInfo.GetPageConfig(path), language));
            }
            catch (ServiceErrorException ex)
            {
                return Error(ex.Message);
            }
            catch (ServiceDeniedException)
            {
                return AccessDenied();
            }
        }
    }
}
