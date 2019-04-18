using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;

namespace Saber
{
    public class Service : Datasilk.Web.Service
    {
        public Service(HttpContext context, Parameters parameters) : base(context, parameters)
        {
        }

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }
    }
}