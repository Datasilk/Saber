using System;
using Microsoft.AspNetCore.Http;

namespace Saber
{
    public class User
    {
        private Core S;
        public string language = "en";

        public User(Core SaberCore)
        {
            S = SaberCore;

            //load user session
            if (S.Session.Get("userinfo") != null)
            {
                var user = (User)S.Util.Serializer.ReadObject(S.Util.Str.GetString(S.Session.Get("userinfo")), GetType());
                language = user.language;
            }
        }

        private void Save()
        {
            S.Session.Set("userinfo", S.Util.Serializer.WriteObject(this));
        }

        public void SetLanguage(string language)
        {
            this.language = language;
            Save();
        }
    }
}
