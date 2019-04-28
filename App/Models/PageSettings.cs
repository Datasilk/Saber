using System;

namespace Saber.Models.Page
{
    public class Settings
    {
        public Title title { get; set; }
        public string description { get; set; } = "";
        public string thumbnail { get; set; } = "";
        public DateTime datecreated { get; set; }
        public Security security { get; set; }
        public string header { get; set; } = "";
        public string footer { get; set; } = "";
    }

    public class Title
    {
        public string prefix { get; set; } = "";
        public string body { get; set; } = "";
        public string suffix { get; set; } = "";
        public int prefixId { get; set; }
        public int suffixId { get; set; }
    }

    public class Security
    {
        public bool secure { get; set; }
        public int[] read { get; set; }
        public int[] write { get; set; }
    }
    
}
