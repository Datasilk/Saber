﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saber.Core;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.Platform
{
    public static class DataSources
    {
        #region "Filters"
        public static string RenderFilters(IRequest request, DataSourceInfo datasource, List<Vendor.DataSource.FilterGroup> filters = null)
        {
            var view = new View("/Views/DataSources/filters.html");
            view["datasource"] = datasource.Key;
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

        public static string RenderFilterGroups(IRequest request, DataSourceInfo datasource,List<Vendor.DataSource.FilterGroup> filters, int depth = 0)
        {
            if(filters == null){ filters = new List<Vendor.DataSource.FilterGroup>(); }
            var viewGroup = new View("/Views/DataSources/filter-group.html");
            var info = datasource.Helper.Get(datasource.Key.Replace(datasource.Helper.Prefix + "-", ""));
            var groupsHtml = new StringBuilder();
            var html = new StringBuilder();
            foreach (var group in filters)
            {
                viewGroup.Clear();
                html.Clear();
                viewGroup["datasource"] = datasource.Key;
                viewGroup[group.Match == Vendor.DataSource.GroupMatchType.Any ? "match-any" : "match-all"] = " selected=\"selected\"";
                if (depth > 0) { viewGroup.Show("sub"); }
                foreach (var filter in group.Elements)
                {
                    html.Append(RenderFilter(request, info, filter));
                }
                if (html.Length > 0)
                {
                    viewGroup["filters"] = html.ToString();
                    viewGroup.Show("has-filters");
                }
                if(group.Groups != null && group.Groups.Count > 0)
                {
                    viewGroup["filter-groups"] = RenderFilterGroups(request, datasource, group.Groups, depth + 1);
                }

                groupsHtml.Append(viewGroup.Render());
            }
            return groupsHtml.ToString();
        }

        public static string RenderFilter(IRequest request, Vendor.DataSource datasource, Vendor.DataSource.FilterElement filter)
        {
            var col = datasource.Columns.Where(a => a.Name == filter.Column).FirstOrDefault();
            if(col == null)
            {
                switch (filter.Column)
                {
                    case "id":
                        col = new Vendor.DataSource.Column()
                        {
                            Name = filter.Column,
                            DataType = Vendor.DataSource.DataType.Number
                        };
                        break;
                    case "datecreated": case "datemodified":
                        col = new Vendor.DataSource.Column()
                        {
                            Name = filter.Column,
                            DataType = Vendor.DataSource.DataType.DateTime
                        };
                        break;
                }
            }
            if (col == null) { return ""; }
            var name = col.Name.Replace("_", " ").Capitalize();
            var value = !string.IsNullOrEmpty(filter.QueryName) && request.Parameters.ContainsKey(filter.QueryName) ?
                request.Parameters[filter.QueryName] : filter.Value;
            switch (col.DataType)
            {
                case Vendor.DataSource.DataType.Text:
                    var viewText = new View("/Views/DataSources/Filters/text.html");
                    viewText["column"] = col.Name;
                    viewText["label"] = name;
                    viewText["value"] = value;
                    viewText["queryname"] = filter.QueryName;
                    viewText[GetFilterMatch(filter)] = " selected=\"selected\"";
                    return viewText.Render();

                case Vendor.DataSource.DataType.Float:
                case Vendor.DataSource.DataType.Number:
                    var viewNumber = new View("/Views/DataSources/Filters/number.html");
                    viewNumber["column"] = col.Name;
                    viewNumber["label"] = name;
                    viewNumber["value"] = value;
                    viewNumber["queryname"] = filter.QueryName;
                    viewNumber[GetFilterMatch(filter)] = " selected=\"selected\"";
                    return viewNumber.Render();

                case Vendor.DataSource.DataType.Boolean:
                    var viewBool = new View("/Views/DataSources/Filters/bool.html");
                    viewBool["column"] = col.Name;
                    viewBool["label"] = name;
                    viewBool["id"] = col.Name;
                    viewBool["checked"] = value == "1" ? "checked=\"checked\"" : "";
                    viewBool["queryname"] = filter.QueryName;
                    return viewBool.Render();

                case Vendor.DataSource.DataType.DateTime:
                    var viewDateTime = new View("/Views/DataSources/Filters/datetime.html");
                    viewDateTime["column"] = col.Name;
                    viewDateTime["label"] = name;
                    viewDateTime["value"] = value;
                    viewDateTime["queryname"] = filter.QueryName;
                    viewDateTime[GetFilterMatch(filter)] = " selected=\"selected\"";
                    return viewDateTime.Render();
            }
            return "";
        }

        private static string GetFilterMatch(Vendor.DataSource.FilterElement filter)
        {
            switch (filter.Match)
            {
                case Vendor.DataSource.FilterMatchType.Contains:
                    return "contains";
                case Vendor.DataSource.FilterMatchType.EndsWith:
                    return "ends-with";
                case Vendor.DataSource.FilterMatchType.Equals:
                    return "equals";
                case Vendor.DataSource.FilterMatchType.GreaterEqualTo:
                    return "greater-than-equals";
                case Vendor.DataSource.FilterMatchType.GreaterThan:
                    return "greater-than";
                case Vendor.DataSource.FilterMatchType.LessThan:
                    return "less-than";
                case Vendor.DataSource.FilterMatchType.LessThanEqualTo:
                    return "less-than-equals";
                case Vendor.DataSource.FilterMatchType.StartsWith:
                    return "starts-with";
            }
            return "";
        }
        #endregion

        #region "OrderBy"
        public static string RenderOrderByList(DataSourceInfo datasource, List<Vendor.DataSource.OrderBy> orderbyList)
        {
            var view = new View("/Views/DataSources/orderby.html");
            view["datasource"] = datasource.Key;
            if (orderbyList == null || orderbyList.Count == 0)
            {
                view.Show("no-content");
            }
            else
            {
                var html = new StringBuilder();
                orderbyList.ForEach(orderby =>
                {
                    html.Append(RenderOrderBy(orderby));
                });
                view["content"] = html.ToString();
                view.Show("has-content");
            }
            return view.Render();
        }

        public static string RenderOrderBy(Vendor.DataSource.OrderBy orderby)
        {
            var name = orderby.Column.Replace("_", " ").Capitalize();
            var view = new View("/Views/DataSources/orderby-item.html");
            view.Show(orderby.Direction == Vendor.DataSource.OrderByDirection.Ascending ? "asc" : "desc");
            view["column"] = orderby.Column;
            view["label"] = name;
            return view.Render();
        }
        #endregion

        #region "Position Settings"
        public static string RenderPositionSettings(DataSourceInfo datasource, Vendor.DataSource.PositionSettings settings = null)
        {
            var view = new View("Views/DataSources/position.html");
            view["start"] = settings?.Start.ToString() ?? "1";
            view["start-query"] = settings?.StartQuery;
            view["length"] = settings?.Length.ToString() ?? "10";
            view["length-query"] = settings?.LengthQuery;
            return view.Render();
        }
        #endregion
    }
}