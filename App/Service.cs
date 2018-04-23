using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;

namespace Saber
{
    public class Service : Datasilk.Service
    {
        public Service(HttpContext context) : base(context) { }

        //override Datasilk.User with Saber.User
        private User user;
        public new User User
        {
            get
            {
                if (user != null) { return user; }
                user = User.Get(context);
                return user;
            }
        }

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }

        public new void Unload()
        {
            if (user != null) { User.Save(); }
        }
    }
}