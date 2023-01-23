using System.Net.Http.Headers;

namespace Saber.Controllers
{
    public class Export : Controller
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity("export")) { return AccessDenied(); }
            try
            {
                var filename = "SaberExport.zip";
                var content = new ByteArrayContent(Common.Platform.Website.Export());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                content.Headers.ContentDisposition.FileName = filename;
                Context.Response.ContentLength = content.Headers.ContentLength;
                Context.Response.ContentType = "application/zip";
                Context.Response.StatusCode = 200;
                Context.Response.Headers.Add("Content-Disposition", "attachment; filename=" + filename);
                content.CopyToAsync(Context.Response.Body);
            }
            catch (Exception ex)
            {
                return Error(ex.Message + "\n" + ex.StackTrace);
            }
            return "";
        }

        
    }
}
