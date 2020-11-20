using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saber.Services
{
    public class Security : Service
    {
        public string Groups()
        {
            if (!CheckSecurity("manage-security")) { return AccessDenied(); }
            var view = new View("/Views/Security/security.html");
            var listitem = new View("/Views/Users/list-item.html");
            var lists = new StringBuilder();
            return Error();
        }
    }
}
