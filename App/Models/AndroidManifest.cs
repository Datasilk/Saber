using System.Collections.Generic;

namespace Saber.Models
{
    public class AndroidManifest
    {
        public string name { get; set; } = "Saber";
        public string start_url { get; set; } = "/";
        public string background_color { get; set; } = "#ffffff";
        public string display { get; set; } = "standalone";
        public List<AndroidIcon> icons { get; set; } = new List<AndroidIcon>();
    }

    public class AndroidIcon
    {
        public string src { get; set; }
        public string sizes { get; set; }
        public string type { get; set; }
        public string density { get; set; }
    }
}
