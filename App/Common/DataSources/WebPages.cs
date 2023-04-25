using Saber.Core;
using Saber.Vendor;
using Saber.Core.Extensions.Strings;
using System.Text;
using Query;
using System;

namespace Saber.Common.DataSources
{
    public class WebPages : IVendorDataSources
    {
        public string Vendor { get; set; } = "Web Pages";
        public string Prefix { get; set; } = "web-pages";
        public string Description { get; set; } = "A list of available web pages within your Saber website";
        public static List<Dictionary<string, string>> KnownPages = new List<Dictionary<string, string>>();

        public void Init() { }

        public void Create(IRequest request, string key, Dictionary<string, string> columns)
        {
            throw new NotImplementedException();
        }

        public List<Dictionary<string, string>> Filter(IRequest request, string key, int start = 1, int length = 0, string lang = "en", List<DataSource.FilterGroup> filter = null, List<DataSource.OrderBy> orderBy = null)
        {
            GetKnownPages();
            var results = KnownPages;
            if(filter.Count > 0)
            {
                foreach(var group in filter)
                {
                    results = FilterGroups(group, results, request);
                }
            }
            return results;
        }

        public Dictionary<string, List<Dictionary<string, string>>> Filter(IRequest request, string key, string lang = "en", Dictionary<string, DataSource.PositionSettings> positions = null, Dictionary<string, List<DataSource.FilterGroup>> filter = null, Dictionary<string, List<DataSource.OrderBy>> orderBy = null, string[] childKeys = null)
        {
            //not implemented
            return new Dictionary<string, List<Dictionary<string, string>>>();
        }

        public int FilterTotal(IRequest request, string key, string lang = "en", List<DataSource.FilterGroup> filter = null)
        {
            GetKnownPages();
            var results = KnownPages.ToList();
            return results.Count();
        }

        public Dictionary<string, int> FilterTotal(IRequest request, string key, string lang = "en", Dictionary<string, List<DataSource.FilterGroup>> filters = null, string[] childKeys = null)
        {
            //not implemented
            return new Dictionary<string, int>();
        }

        public DataSource Get(string key)
        {
            return new DataSource()
            { 
                Name = "Web Pages",
                Key = "web-pages",
                Relationships = new DataSource.Relationship[0],
                Columns = new DataSource.Column[]
                {
                    new DataSource.Column(){ Name = "file", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "path", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "parent-path", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "title", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "description", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "datecreated", DataType = DataSource.DataType.DateTime }
                }
            };
        }

        public List<KeyValuePair<string, string>> List()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("web-pages", "Web Pages")
            };
            return list;
        }

        public void Update(IRequest request, string key, string id, Dictionary<string, string> columns)
        {
            throw new NotImplementedException();
        }

        #region "Helpers"
        private void GetKnownPages()
        {
            //get list of known pages
            if(KnownPages.Count > 0) { return; }
            var dir = new DirectoryInfo(App.MapPath("/Content/pages"));
            var prefix = App.MapPath("/Content/pages/");
            var files = dir.GetFiles("*.html", SearchOption.AllDirectories)
                .Select(a => a.FullName.Replace(prefix, "").Replace(".html", "").Replace("\\", "/"));
            foreach (var file in files)
            {
                //get page info from associated json file
                var info = PageInfo.GetPageConfig("/Content/pages/" + file);
                var parts = file.Split("/");
                var path = string.Join("/", info.Paths) + "/";
                var parent = string.Join("/", parts.SkipLast(1)) + "/";
                var name = parts.Last();
                KnownPages.Add(new Dictionary<string, string>()
                {
                    {"file", file },
                    {"path", path },
                    {"parent-path", parent },
                    {"title", info != null ? info.Title.body : parent + name.Replace("-", " ").Capitalize() },
                    {"description", info != null ? info.Description : "" },
                    {"datecreated", info != null ? info.DateCreated.ToString("yyyy-MM-dd HH:mm") : "" }
                });
            }
        }

        private static List<Dictionary<string, string>> FilterGroups(DataSource.FilterGroup group, List<Dictionary<string, string>> records, IRequest request)
        {
            if(records.Count == 0) { return records; }
            var results = new List<Dictionary<string, string>>();
            if (group.Elements != null && group.Elements.Count > 0)
            {
                for (var x = 0; x < group.Elements.Count; x++)
                {
                    var element = group.Elements[x];
                    var value = element.Value;
                    var userecords = group.Match == DataSource.GroupMatchType.All ? results : records;
                    var results2 = new List<Dictionary<string, string>>();

                    if (element.Column == "datecreated")
                    {
                        //match on DateTime
                        var datetime = DateTime.Parse(element.Value);
                        switch (element.Match)
                        {
                            case DataSource.FilterMatchType.Equals:
                                results2.AddRange(userecords.Where(a => datetime == DateTime.Parse(a["datecreated"])));
                                break;
                            case DataSource.FilterMatchType.GreaterThan:
                                results2.AddRange(userecords.Where(a => datetime > DateTime.Parse(a["datecreated"])));
                                break;
                            case DataSource.FilterMatchType.GreaterEqualTo:
                                results2.AddRange(userecords.Where(a => datetime >= DateTime.Parse(a["datecreated"])));
                                break;
                            case DataSource.FilterMatchType.LessThan:
                                results2.AddRange(userecords.Where(a => datetime < DateTime.Parse(a["datecreated"])));
                                break;
                            case DataSource.FilterMatchType.LessThanEqualTo:
                                results2.AddRange(userecords.Where(a => datetime <= DateTime.Parse(a["datecreated"])));
                                break;
                        }
                    }
                    else
                    {
                        //match on text
                        value = value.ToLower();
                        switch (element.Match)
                        {
                            case DataSource.FilterMatchType.StartsWith:
                                results2.AddRange(userecords.Where(a => a[element.Column].ToLower().StartsWith(value)));
                                break;
                            case DataSource.FilterMatchType.EndsWith:
                                results2.AddRange(userecords.Where(a => a[element.Column].ToLower().EndsWith(value)));
                                break;
                            case DataSource.FilterMatchType.Contains:
                                results2.AddRange(userecords.Where(a => a[element.Column].ToLower().Contains(value)));
                                break;
                            case DataSource.FilterMatchType.Equals:
                                results2.AddRange(userecords.Where(a => a[element.Column].ToLower() == value));
                                break;
                        }
                    }

                    if(group.Match == DataSource.GroupMatchType.All)
                    {
                        //narrow ressults
                        results = results2;
                    }
                    else
                    {
                        //combine results
                        results.AddRange(results2.Where(a => !results.Any(b => b["path"] == a["path"])));
                    }
                }
            }
            if(group.Groups.Count > 0)
            {
                if(group.Match == DataSource.GroupMatchType.All)
                {
                    //narrow matches based on all sub group matches
                    foreach (var subgroup in group.Groups)
                    {
                        results = FilterGroups(subgroup, results, request);
                    }
                }
                else
                {
                    //combine all matches from group & sub groups
                    foreach (var subgroup in group.Groups)
                    {
                        var results2 = FilterGroups(subgroup, results, request);
                        results.AddRange(results2.Where(a => !results.Any(b => b["path"] == a["path"])));
                    }
                }
            }
            return results;
        }

        #endregion
    }
}
