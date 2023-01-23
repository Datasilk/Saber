namespace Saber.Services
{
    public class ImportExport : Service
    {
        public string RenderImport()
        {
            var view = new View("/Views/ImportExport/import.html");
            return view.Render();
        }

        public string RenderExport()
        {
            var view = new View("/Views/ImportExport/export.html");
            return view.Render();
        }
    }
}
