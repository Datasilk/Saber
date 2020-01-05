namespace Saber.Vendor
{
    public interface IVendorViewRenderer
    {
        string Render(Request request, View view);
    }
}
