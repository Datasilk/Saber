using System;
using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;

namespace Datasilk
{
    public partial class User
    {
        public string language = "en";

        public void SetLanguage(string language)
        {
            this.language = language;
            changed = true;
        }
    }
}
