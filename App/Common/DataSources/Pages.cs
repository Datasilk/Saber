using Saber.Core;
using Saber.Vendor;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.DataSources
{
    public class Pages : IVendorDataSources
    {
        public string Vendor { get; set; } = "Web Pages";
        public string Prefix { get; set; } = "web-pages";
        public string Description { get; set; } = "A list of available web pages within your Saber website";
        public static List<Dictionary<string, string>> KnownPages = new List<Dictionary<string, string>>();

        public void Create(IRequest request, string key, Dictionary<string, string> columns)
        {
            throw new NotImplementedException();
        }

        public List<Dictionary<string, string>> Filter(IRequest request, string key, int start = 1, int length = 0, string lang = "en", List<DataSource.FilterGroup> filter = null, List<DataSource.OrderBy> orderBy = null)
        {
            GetKnownPages();
            var results = KnownPages.ToList();
            return results;
        }

        public Dictionary<string, List<Dictionary<string, string>>> Filter(IRequest request, string key, string lang = "en", Dictionary<string, DataSource.PositionSettings> positions = null, Dictionary<string, List<DataSource.FilterGroup>> filter = null, Dictionary<string, List<DataSource.OrderBy>> orderBy = null, string[] childKeys = null)
        {
            GetKnownPages();
            var results = KnownPages.ToList();
            return new Dictionary<string, List<Dictionary<string, string>>>(){
                {"web-pages", results }
            };
        }

        public int FilterTotal(IRequest request, string key, string lang = "en", List<DataSource.FilterGroup> filter = null)
        {
            GetKnownPages();
            var results = KnownPages.ToList();
            return results.Count();
        }

        public Dictionary<string, int> FilterTotal(IRequest request, string key, string lang = "en", Dictionary<string, List<DataSource.FilterGroup>> filters = null, string[] childKeys = null)
        {
            throw new NotImplementedException();
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
                    new DataSource.Column(){ Name = "title", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "description", DataType = DataSource.DataType.Text },
                    new DataSource.Column(){ Name = "datecreated", DataType = DataSource.DataType.DateTime }
                }
            };
        }

        public void Init(){}

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
                var parent = string.Join("/", parts.SkipLast(1)) + "/";
                var name = parts.Last();
                KnownPages.Add(new Dictionary<string, string>()
                {
                    {"file", file },
                    {"title", info != null ? info.title.body : parent + name.Replace("-", " ").Capitalize() },
                    {"description", info != null ? info.description : "" },
                    {"datecreated", info != null ? info.datecreated.ToString("yyyy-MM-dd HH:mm") : "" }
                });
            }
        }
    }
}
