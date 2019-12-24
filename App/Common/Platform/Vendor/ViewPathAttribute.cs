using System;

namespace Saber.Vendor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ViewPathAttribute: Attribute
    {
        public string Path { get; set; }
        public ViewPathAttribute(string viewPath) {
            Path = viewPath;
        }
    }
}
