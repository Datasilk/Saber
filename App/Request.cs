using System.Text;
namespace Saber
{
    public class Request : Datasilk.Core.Web.Request
    {
        protected User user;
        public User User
        {
            get
            {
                if (user == null)
                {
                    user = User.Get(Context);
                }
                return user;
            }
        }

        protected virtual string RenderView(View view)
        {
            //check for vendor-related View rendering
            var vendors = new StringBuilder();
            if (Server.viewRenderers.ContainsKey(view.Filename)) 
            {
                var renderers = Server.viewRenderers[view.Filename];
                foreach(var renderer in renderers)
                {
                    vendors.Append(renderer.Render(this, view));
                }
            }
            if(vendors.Length > 0)
            {
                view["vendor"] = vendors.ToString();
            }
            
            return view.Render();
        }
    }
}
