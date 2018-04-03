namespace Saber.Models.Page
{
    public class Title
    {
        public string prefix;
        public string body;
        public string suffix;
        public int prefixId;
        public int suffixId;
    }

    public class Security
    {
        public bool secure;
        public int[] read;
        public int[] write;
    }
    
    public class Settings
    {
        public Title title;
        public string description;
        public Security security;
    }
}
