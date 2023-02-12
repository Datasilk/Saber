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
                var includeWebPages = Parameters.ContainsKey("webpages") && Parameters["webpages"] != "1" ? false : true;
                var includeImages = Parameters.ContainsKey("images") && Parameters["images"] != "1" ? false : true;
                var includeOtherFiles = Parameters.ContainsKey("other") && Parameters["other"] == "1" ? true : false;
                var modified = Parameters.ContainsKey("modified") && Parameters["modified"].Split("/").Length > 2 ? Parameters["modified"].Split("/") : new string[0];
                DateTime? lastModified = Parameters.ContainsKey("modified") && Parameters["modified"].Split("/").Length > 2 ? new DateTime(int.Parse(modified[0]), int.Parse(modified[1]), int.Parse(modified[2])) : null;
                var content = new ByteArrayContent(Common.Platform.Website.Export(includeWebPages, includeImages, includeOtherFiles, lastModified));
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
