using System.Text;

namespace Saber.Services
{
    public class Languages
    {
        public string Get()
        {
            var html = new StringBuilder();
            foreach (var lang in Server.languages)
            {
                html.Append(lang.Key + ',' + lang.Value + '|');
            }
            return html.ToString().TrimEnd('|');
        }
    }
}
