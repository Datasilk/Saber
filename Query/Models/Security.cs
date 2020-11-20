namespace Query.Models
{
    public class SecurityKey
    {
        public string key { get; set; }
        public bool value { get; set; }
    }

    public class SecurityGroup
    {
        public string name { get; set; }
        public int platformKeys { get; set; }
        public int pluginKeys { get; set; }
        public string keys { get; set; }
    }
}
