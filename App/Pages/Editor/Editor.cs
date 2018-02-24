using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saber.Pages
{
    public class Editor : Page
    {
        public Editor(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            var scaffold = new Scaffold("/Pages/Editor/editor.html");
            if(S.User.userId > 0)
            {
                //load editor
            }
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
