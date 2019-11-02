using Datasilk.Core.Web;

namespace Saber
{
    public class Service : Request, IService
    {
        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }

        public string Success()
        {
            return "success";
        }

        public string Empty() { return "{}"; }
    }
}