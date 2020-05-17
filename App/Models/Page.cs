using System;
using System.Collections.Generic;

namespace Saber.Models.Page
{
    public class Settings
    {
        public Title title { get; set; } = new Title();
        public string description { get; set; } = "";
        public string thumbnail { get; set; } = "";
        public DateTime datecreated { get; set; }
        public Security security { get; set; } = new Security();
        public Template header { get; set; } = new Template();
        public Template footer { get; set; } = new Template();
        public List<string> scripts { get; set; } = new List<string>();

        public Settings()
        {
            datecreated = DateTime.Now;
            header.file = "header.html";
            footer.file = "footer.html";
        }
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
        public bool secure { get; set; } = false;
        public int[] read { get; set; } = new int[] { };
        public int[] write { get; set; } = new int[] { };
    }

    public class Template
    {
        public string file { get; set; } = "";
        public Dictionary<string, string> fields { get; set; } = new Dictionary<string, string>();
    }
}
