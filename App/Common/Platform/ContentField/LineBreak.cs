using System.Collections.Generic;
using Saber.Core;
using Saber.Vendor;

namespace Saber.Common.Platform.ContentField
{
    [ContentField("#")]
    [ReplaceRow]
    public class LineBreak : IVendorContentField
    {
        public string Render(IRequest request, Dictionary<string, string> args, string data, string id, string prefix, string key, string lang, string container)
        {
            return "";
        }
    }
}
