using System;

namespace Query.Models
{
    public class User
    {
        public int userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public bool photo { get; set; }
        public DateTime datecreated { get; set; }
        public DateTime dateactivated { get; set; }
        public DateTime keyexpires { get; set; }
        public string tempkey { get; set; }
    }

    public class UserWithSecurityCount: User
    {
        public int security { get; set; }
    }
}
