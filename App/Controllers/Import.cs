namespace Saber.Controllers
{
    public class Import : Controller
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity("import")) { return AccessDenied(); }
            if (Parameters.Files.Count == 0)
            {
                return Error("Please specify a file to import");
            }
            if (Parameters.Files.Count > 0 && Parameters.Files["zip"].ContentType != "application/x-zip-compressed")
            {
                return Error("Import file must be a compressed zip file.");
            }
            //create backup of website
            var copyTo = App.MapPath("Content/backups/");
            if (!Directory.Exists(copyTo))
            {
                Directory.CreateDirectory(copyTo);
            }
            File.WriteAllBytes(copyTo + "latest.zip", Common.Platform.Website.Export());

            //open uploaded zip file
            Common.Platform.Website.Import(Parameters.Files["zip"]);

            return "latest.zip";
        }

        
    }
}
