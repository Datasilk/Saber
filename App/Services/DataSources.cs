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

        public string Columns(string key)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if(datasource != null)
            {
                var columns = new List<Vendor.DataSource.Column>() { 
                    new Vendor.DataSource.Column()
                    {
                        Name = "id",
                        DataType = Vendor.DataSource.DataType.Number
                    },
                    new Vendor.DataSource.Column()
                    {
                        Name = "datecreated",
                        DataType = Vendor.DataSource.DataType.DateTime
                    },
                    new Vendor.DataSource.Column()
                    {
                        Name = "datemodified",
                        DataType = Vendor.DataSource.DataType.DateTime
                    }
                };
                
                columns.AddRange(datasource.Helper.Get(key.Replace(datasource.Helper.Prefix + "-", "")).Columns);
                return JsonResponse(columns);
            }
            return Error("Could not find data source");
        }

        public string Relationships(string key)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if (datasource != null)
            {
                return JsonResponse(datasource.Helper.Get(key.Replace(datasource.Helper.Prefix + "-", "")).Relationships);
            }
            return Error("Could not find data source");
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
            foreach (var datasource in datasources)
            {
                htmlitem.Clear();
                listItem.Clear();
                listItem["title"] = datasource.Vendor;
                listItem["description"] = datasource.Description;
                foreach (var item in datasource.List())
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
            if (source != null)
            {
                source.Helper.Create(this, datasource.Replace(source.Helper.Prefix + "-", ""), columns);
            }
            return Success();
        }

        #region "Filters"
        public string RenderFilter(string key, string column)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if (datasource == null)
            {
                return Error("Could not find data source \"" + key + "\"");
            }
            return Common.Platform.DataSources.RenderFilter(this, datasource.Helper.Get(key.Replace(datasource.Helper.Prefix + "-", "")), new Vendor.DataSource.FilterElement()
            {
                Column = column
            });
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

        public string RenderFilterGroups(string key, List<Vendor.DataSource.FilterGroup> groups, int depth = 0)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            try
            {
                var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
                if (datasource == null)
                {
                    return Error("Could not find data source \"" + key + "\"");
                }
                return Common.Platform.DataSources.RenderFilterGroups(this, datasource, groups, depth);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        #endregion

        #region "Order By"
        public string RenderOrderByList(string key, List<Vendor.DataSource.OrderBy> orderby = null)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if (datasource == null)
            {
                return Error("Could not find data source \"" + key + "\"");
            }
            return Common.Platform.DataSources.RenderOrderByList(datasource, orderby);
        }
        public string RenderOrderBy(string key, string column, int direction = 0)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var datasource = Core.Vendors.DataSources.Where(a => a.Key == key).FirstOrDefault();
            if (datasource == null)
            {
                return Error("Could not find data source \"" + key + "\"");
            }
            var col = new Vendor.DataSource.OrderBy()
            {
                Column = column,
                Direction = direction == 0 ? Vendor.DataSource.OrderByDirection.Ascending : Vendor.DataSource.OrderByDirection.Descending
            };
            return Common.Platform.DataSources.RenderOrderBy(col);
        }
        #endregion
    }
}
