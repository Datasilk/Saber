namespace Saber
{
    public class Request : Datasilk.Core.Web.Request
    {
        private User user;
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
            if (Server.viewRenderers.ContainsKey(view.Filename)) 
            {
                var renderers = Server.viewRenderers[view.Filename];
                foreach(var renderer in renderers)
                {
                    renderer.Render(this, view);
                }
            }
            return view.Render();
        }
    }
}
