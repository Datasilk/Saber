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
            var host = Context.Request.Headers.Origin.ToString();
            if (Server.DeveloperKeys.Count > 0 && Server.DeveloperKeys.Any(a => host.IndexOf(a.Host) == 0))
            {
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
                    Console.WriteLine("api key doesn't exist in config.json");
                    Context.Response.WriteAsync(AccessDenied("access denied"));
                    return;
                }
                else if (Parameters.ContainsKey("token"))
                {
                    //authenticate user account (if required)
                    var user = Query.Users.AuthenticateApi(Parameters["token"]);
                    if (user != null)
                    {
                        User.LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin, true);
                        IsPublicApiRequest = true;
                    }
                    else
                    {
                        Context.Response.WriteAsync(AccessDenied("Expired user token"));
                        return;
                    }
                }
                else
                {
                    //finally, try authenticating using valid API key
                    var apiInfo = GetApiKeyInfo(Parameters["apikey"]);
                    if (apiInfo != null && apiInfo.UserId.HasValue == true)
                    {
                        var user = Query.Users.GetDetails(apiInfo.UserId.Value);
                        User.LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin, true);
                    }

                    User.PublicApi = true;
                    IsPublicApiRequest = true;
                    return;
                }
            }
            if (Parameters.ContainsKey("auth-token"))
            {
                //authenticate user using their temporary session-based authentication token
                var user = Query.Users.Authenticate(Parameters["auth-token"], false);
                if(user != null)
                {
                    User.LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin, true);
                    IsPublicApiRequest = true;
                }
                else
                {
                    Context.Response.WriteAsync(AccessDenied("Could not authenticate user"));
                    return;
                }
            }
        }

        public override bool CheckSecurity(string key = "")
        {
            return Common.Platform.Service.CheckSecurity(User, key);
        }
    }
}