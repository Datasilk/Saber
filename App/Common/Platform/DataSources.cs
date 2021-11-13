using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saber.Core;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.Platform
{
    public static class DataSources
    {
        public static string RenderFilters(IRequest request, DataSourceInfo datasource, List<Vendor.DataSource.FilterGroup> filters)
        {
            var view = new View("/Views/DataSources/filters.html");
            if(filters == null || filters.Count == 0)
            {
                view.Show("no-content");
            }
            else
            {
                view["content"] = RenderFilterGroups(request, datasource, filters);
                view.Show("has-content");
            }
            return view.Render();
        }

        public static string RenderFilterGroups(IRequest request, DataSourceInfo datasource, List<Vendor.DataSource.FilterGroup> filters)
        {
            if(filters == null || filters.Count == 0) { return ""; }
            var viewGroup = new View("/Views/DataSources/filter-group.html");
            var viewText = new View("/Views/DataSources/Filters/text.html");
            var viewNumber = new View("/Views/DataSources/Filters/number.html");
            var viewBool = new View("/Views/DataSources/Filters/bool.html");
            var viewDateTime = new View("/Views/DataSources/Filters/datetime.html");
            var info = datasource.Helper.Get(datasource.Key);
            var groupsHtml = new StringBuilder();
            var html = new StringBuilder();
            foreach (var group in filters)
            {
                viewGroup.Clear();
                if (group.Elements.Count > 0)
                {
                    viewGroup.Show("has-filters");
                }
                if (group.Groups.Count > 0)
                {
                    viewGroup.Show("has-filter-groups");
                }
                foreach (var filter in group.Elements)
                {
                    var col = info.Columns.Where(a => a.Name == filter.Column).FirstOrDefault();
                    if (col == null) { continue; }
                    var name = col.Name.Replace("_", " ").Capitalize();
                    var value = !string.IsNullOrEmpty(filter.QueryName) && request.Parameters.ContainsKey(filter.QueryName) ?
                        request.Parameters[filter.QueryName] : filter.Value;
                    switch (col.DataType)
                    {
                        case Vendor.DataSource.DataType.Text:
                            viewText["label"] = name;
                            viewText["value"] = value;
                            html.Append(viewText.Render());
                            break;
                        case Vendor.DataSource.DataType.Float:
                        case Vendor.DataSource.DataType.Number:
                            viewNumber["label"] = name;
                            viewNumber["value"] = value;
                            html.Append(viewNumber.Render());
                            break;
                        case Vendor.DataSource.DataType.Boolean:
                            viewBool["label"] = name;
                            viewBool["id"] = col.Name;
                            viewBool["checked"] = value == "1" ? "checked=\"checked\"" : "";
                            html.Append(viewBool.Render());
                            break;
                        case Vendor.DataSource.DataType.DateTime:
                            viewDateTime["label"] = name;
                            viewDateTime["value"] = value;
                            html.Append(viewDateTime.Render());
                            break;
                    }
                }
                groupsHtml.Append(html + RenderFilterGroups(request, datasource, group.Groups));
            }
            return groupsHtml.ToString();
        }
    }
}
