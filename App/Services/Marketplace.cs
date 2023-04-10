using Saber.Core;

namespace Saber.Services
{
    public class Marketplace : Service
    {
        public string Toolbar()
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            return Cache.LoadFile("Views/Market/toolbar.html");
        }

        public string InstallTemplate(int templateId, string token)
        {
            if (!CheckSecurity("install-template")) { return AccessDenied(); }
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string> {
                    {"auth-token", token },
                    { "templateId", templateId.ToString() }
                };
                using (var content = new FormUrlEncodedContent(parameters))
                {
                    var response = client.PostAsync(Server.SaberCmsHost + "api/DownloadTemplate/Request", content).Result;
                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        using (var ms = new MemoryStream())
                        {
                            response.Content.CopyToAsync(ms);
                            ms.Position = 0;
                            if (ms.Length == 0) { return Error("template not found"); }
                            //unzip template and import
                            try
                            {
                                Common.Platform.Website.Import(ms, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, null, "Install Template");
                                return Error("Error installing template: " + ex.Message);
                            }

                        }
                    }
                    else
                    {
                        return Error("Error installing template: Status Code " + (int)response.StatusCode + " (" + response.StatusCode + ") when trying to download template zip file");
                    }
                    
                }
            }
            return Success();
        }
    }
}
