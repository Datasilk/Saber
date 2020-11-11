using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Datasilk.Core.Web;
using Saber.Core;

namespace Saber
{
    public class Service : Request, Core.IRequest, Core.IService
    {
        protected StringBuilder Scripts = new StringBuilder();
        protected StringBuilder Css = new StringBuilder();
        protected List<string> Resources = new List<string>();

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }

        protected IUser user;
        public IUser User
        {
            get
            {
                if (user == null)
                {
                    user = Saber.User.Get(Context);
                }
                return user;
            }
            set { user = value; }
        }

        public void Init() { }

        public string JsonResponse(dynamic obj)
        {
            Context.Response.ContentType = "text/json";
            return JsonSerializer.Serialize(obj);
        }

        public bool CheckSecurity(string key = "")
        {
            if (User.UserId == 1) { return true; }
            if (key != "" && User.UserId > 0 && !User.Keys.Any(a => a.Key == key && a.Value == true))
            {
                return false;
            }
            else if (key == "" && User.UserId <= 0)
            {
                return false;
            }
            return true;
        }

        public override void Dispose()
        {
            if (user != null)
            {
                User.Save();
            }
        }

        public string Success()
        {
            return "success";
        }

        public string Empty() { return "{}"; }

        public string AccessDenied(string message = "Error 403")
        {
            Context.Response.StatusCode = 403;
            return message;
        }

        public string Error(string message = "Error 500")
        {
            Context.Response.StatusCode = 500;
            return message;
        }

        public string BadRequest(string message = "Bad Request 400")
        {
            Context.Response.StatusCode = 400;
            return message;
        }

        public void AddScript(string url, string id = "", string callback = "")
        {
            if (ContainsResource(url)) { return; }
            Scripts.Append("S.util.js.load('" + url + "', '" + id + "', " + (callback != "" ? callback : "null") + ");");
        }

        public void AddCSS(string url, string id = "")
        {
            if (ContainsResource(url)) { return; }
            Css.Append("S.util.css.load('" + url + "', '" + id + "');");
        }

        protected bool ContainsResource(string url)
        {
            if (Resources.Contains(url)) { return true; }
            Resources.Add(url);
            return false;
        }
    }
}