using System;
using System.Text;

namespace Saber.Services
{
    public class Languages: Service
    {
        public string Get()
        {
            var html = new StringBuilder();
            foreach (var lang in Server.Languages)
            {
                html.Append(lang.Key + ',' + lang.Value + '|');
            }
            return html.ToString().TrimEnd('|');
        }

        public string Create(string name, string abbr)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                Query.Languages.Create(new Query.Models.Language()
                {
                    langId = abbr.ToLower(),
                    language = name
                });
                return Success();
            }
            catch (Exception)
            {
                return Error("Could not create language");
            }
        }
    }
}
