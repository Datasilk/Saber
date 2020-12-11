using System;
using System.Linq;
using System.Text;
using Saber.Common.Platform;

namespace Saber.Services
{
    public class Languages: Service
    {
        public string Get()
        {
            var html = new StringBuilder();
            foreach (var lang in App.Languages)
            {
                html.Append(lang.Key + ',' + lang.Value + '|');
            }
            return html.ToString().TrimEnd('|');
        }

        public string Create(string name, string abbr)
        {
            if (!CheckSecurity("edit-content")) { return AccessDenied(); }
            try
            {
                var config = Website.Settings.Load();
                if(!config.Languages.Any(a => a.Id == abbr))
                {
                    config.Languages.Add(new Models.Website.Language() { Id = abbr, Name = name });
                    App.Languages.Add(abbr, name);
                }
                Website.Settings.Save(config);
                return Success();
            }
            catch (Exception)
            {
                return Error("Could not create language");
            }
        }
    }
}
