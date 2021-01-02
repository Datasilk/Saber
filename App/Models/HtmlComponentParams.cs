namespace Saber.Models
{
    public class HtmlComponentParams
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public int DataType { get; set; }
        public bool List { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
        public string[] ListOptions { get; set; }
        public string Description { get; set; }
    }
}
