using System;

namespace Query.Models
{
    public class Notification
    {
        public Guid notifId { get; set; }
        public DateTime datecreated { get; set; }
        public int? userId { get; set; }
        public int? groupId { get; set; }
        public string securityKey { get; set; }
        public string type { get; set; }
        public string notification { get; set; }
        public string url { get; set; }
    }
}
