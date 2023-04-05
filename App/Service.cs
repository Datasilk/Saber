using Saber.Core;

namespace Saber
{
    public class Service : Core.Service
    {
        public override IUser User
        {
            get
            {
                if (user == null)
                {
                    user = Saber.User.Get(Context, Session);
                }
                return user;
            }
            set { user = value; }
        }

        public override void Init()
        {
            if (Server.DeveloperKeys.Count > 0 && Server.DeveloperKeys.Any(a => (Context.Request.Scheme + "://" + Context.Request.Host.Value).IndexOf(a.Host) == 0) || Parameters.ContainsKey("apikey"))
            {
                Console.WriteLine("authenticating user based on API key");
                Console.WriteLine("request host = " + Context.Request.Scheme + "://" + Context.Request.Host.Value);
                //require a Public API developer key to continue
                if (!Parameters.ContainsKey("apikey"))
                {
                    //no api key found in request
                    Context.Response.WriteAsync(AccessDenied("apikey parameter required"));
                    return;
                }
                else if (!CheckApiKey(Parameters["apikey"]))
                {
                    //api key doesn't exist in config.json
                    Context.Response.WriteAsync(AccessDenied("access denied"));
                    return;
                }
                else
                {
                    //authenticate user account (if required)
                    if (Parameters.ContainsKey("token"))
                    {
                        var user = Query.Users.AuthenticateApi(Parameters["token"]);
                        if (user != null)
                        {
                            User.LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin, true);
                            IsPublicApiRequest = true;
                        }
                        else
                        {
                            Context.Response.WriteAsync(AccessDenied("Expired user token"));
                        }
                    }
                    else
                    {
                        var apiInfo = GetApiKeyInfo(Parameters["apikey"]);
                        if(apiInfo != null && apiInfo.UserId.HasValue == true)
                        {
                            var user = Query.Users.GetDetails(apiInfo.UserId.Value);
                            User.LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin, true);
                        }

                        User.PublicApi = true;
                        IsPublicApiRequest = true;
                    }
                }
            }
        }

        public override bool CheckSecurity(string key = "")
        {
            return Common.Platform.Service.CheckSecurity(User, key);
        }
    }
}