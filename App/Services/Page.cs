using Saber.Core;

namespace Saber.Services
{
    public class Page : Service
    {
        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer, or scripts)</returns>
        public string Render(string path, string language = "en")
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            try
            {
                var content = Common.Platform.Render.Page(path, this, PageInfo.GetPageConfig(path), language);
                //remove any scripts & css from response
                Scripts = new System.Text.StringBuilder();
                Css = new System.Text.StringBuilder();
                return Response(content);
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
