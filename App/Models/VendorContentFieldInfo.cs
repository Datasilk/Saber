using Saber.Vendor;

namespace Saber.Models
{
    public class VendorContentFieldInfo
    {
        public IVendorContentField ContentField { get; set; }
        public bool ReplaceRow { get; set; } = false;
    }
}
