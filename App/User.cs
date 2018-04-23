using System;
using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;

namespace Saber
{
    public class User: Datasilk.User
    {
        //constructor
        public User(HttpContext context) : base(context) { }

        //properties
        public string language = "en";

        //get User object from session
        public static User Get(HttpContext context)
        {
            User user;
            if (context.Session.Get("user") != null)
            {
                user = (User)Serializer.ReadObject(context.Session.Get("user").GetString(), typeof(User));
            }
            else
            {
                user = new User(context);
            }
            user.Init();
            return user;
        }

        //initialize user after they visit website for the first time
        public override void Init()
        {
            base.Init();
        }

        public new void Save(bool changed = false)
        {
            if (this.changed == true || changed == true)
            {
                context.Session.Set("user", Serializer.WriteObject(this));
            }
        }

        public void SetLanguage(string language)
        {
            this.language = language;
            changed = true;
        }
    }
}
