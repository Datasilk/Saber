using System;
using System.Collections.Generic;
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
            return JsonResponse(Core.Vendors.DataSources.Select(a => new string[] {
                a.Key, 
                (string.IsNullOrEmpty(a.Helper.Vendor) ? "" : a.Helper.Vendor + " - ") + a.Name
            }).ToArray());
        }

        public string RenderFilters(string key, List<Vendor.DataSource.FilterGroup> filters)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if(datasource == null)
            {
                return Error("Could not find data source \"" + key + "\"");
            }
            return Common.Platform.DataSources.RenderFilters(this, datasource, filters);
        }

        public string RenderFilterGroup(string key, List<Vendor.DataSource.FilterGroup> groups)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            try
            {
                var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
                if (datasource == null)
                {
                    return Error("Could not find data source \"" + key + "\"");
                }
                return Common.Platform.DataSources.RenderFilterGroups(this, datasource, groups);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
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
                    datasourceItem["columns"] = string.Join(", ", datasource.Get(item.Key).Columns.Select(a => a.Name));
                    htmlitem.Append(datasourceItem.Render());
                }
                listItem["data-sources"] = htmlitem.ToString();
                html.Append(listItem.Render());
            }
            view["data-sources"] = html.ToString();
            return view.Render();
        }

        public string AddRecord(string datasource, Dictionary<string, string> columns)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-datasources")) { return AccessDenied(); }
            var source = Core.Vendors.DataSources.Where(a => a.Key == datasource).FirstOrDefault();
            if(source != null)
            {
                source.Helper.Create(this, datasource.Replace(source.Helper.Prefix + "-", ""), columns);
            }
            return Success();
        }
    }
}
