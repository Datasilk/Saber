using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;

namespace Saber
{
    public class Service : Datasilk.Service
    {
        public Service(HttpContext context) : base(context) { }


        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }
    }
}