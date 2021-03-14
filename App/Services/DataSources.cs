using System.Linq;

namespace Saber.Services
{
    public class DataSources: Service
    {
        public string List()
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            //get list of data sources available
            return JsonResponse(Common.Vendors.DataSources.Select(a => new string[] {a.Key, (string.IsNullOrEmpty(a.Helper.Vendor) ? "" : a.Helper.Vendor + " - ") + a.Name 
            }).ToArray());
        }

        public string RenderFilters(string key)
        {
            if (User.PublicApi || !CheckSecurity("edit-content")) { return AccessDenied(); }
            //get HTML partial for data source filters form
            var datasource = Common.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if(datasource == null)
            {
                return Error("Could not find data source \"" + key + "\"");
            }
            var filter = datasource.Helper.RenderFilters(key.Replace(datasource.Helper.Prefix + "-", "") , this);
            return filter.OnInit + "|" + filter.HTML;
        }
    }
}
