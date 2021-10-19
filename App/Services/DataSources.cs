using System.Text;
using System.Linq;

namespace Saber.Services
{
    public class DataSources: Service
    {
        public string List()
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            //get list of data sources available
            return JsonResponse(Core.Vendors.DataSources.Select(a => new string[] {a.Key, (string.IsNullOrEmpty(a.Helper.Vendor) ? "" : a.Helper.Vendor + " - ") + a.Name 
            }).ToArray());
        }

        public string RenderFilters(string key)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            //get HTML partial for data source filters form
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if(datasource == null)
            {
                return Error("Could not find data source \"" + key + "\"");
            }
            var filter = datasource.Helper.RenderFilters(key.Replace(datasource.Helper.Prefix + "-", "") , this);
            return filter.OnInit + "|" + filter.HTML;
        }

        public string RenderList()
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasources = Core.Vendors.DataSources.Select(a => a.Helper).Distinct();
            var html = new StringBuilder();
            var htmlitem = new StringBuilder();
            var view = new View("/Views/DataSources/datasources.html");
            var listItem = new View("/Views/DataSources/list-item.html");
            var datasourceItem = new View("/Views/DataSources/datasource-item.html");
            foreach(var datasource in datasources)
            {
                htmlitem.Clear();
                listItem.Clear();
                listItem["title"] = datasource.Vendor;
                listItem["description"] = datasource.Description;
                foreach(var item in datasource.List())
                {
                    datasourceItem.Clear();
                    datasourceItem["name"] = item.Value;
                    datasourceItem["columns"] = string.Join(", ", datasource.Get(item.Key).Columns);
                    htmlitem.Append(datasourceItem.Render());
                }
                listItem["data-sources"] = htmlitem.ToString();
                html.Append(listItem.Render());
            }
            view["data-sources"] = html.ToString();
            return view.Render();
        }
    }
}
