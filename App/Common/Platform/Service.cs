
using Saber.Core;

namespace Saber.Common.Platform
{
    public static class Service
    {
        private static Saber.Service GetService(Core.Service service)
        {
            var newservice = new Saber.Service()
            {
                Context = service.Context,
                Parameters = service.Parameters,
                Path = service.Path,
                PathParts = service.PathParts
            };
            return newservice;
        }

        public static void Init(Core.Service service)
        {
            var newservice = GetService(service);
            newservice.Init();
            service.User = newservice.User;
        }

        /// <summary>
        /// Set Saber.Service as the type to use when executing CheckSecurity
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CheckSecurity(IUser User, string key = "")
        {
            if (User.IsAdmin) { return true; }
            if (User.PublicApi == true)
            {
                //using Public API
                if (User.UserId > 0)
                {
                    //user is logged in
                    if (key != "")
                    {
                        //check if user has access to specific key
                        return Query.Security.Users.Check(User.UserId, key);
                    }
                    else
                    {
                        //key not specified
                        return true;
                    }
                }
            }
            else
            {
                //using private API (origin domain)
                if (key != "" && User.UserId > 0 && User.Keys.Any(a => a.Key == key && a.Value == true))
                {
                    //user has access to specified security key 
                    return true;
                }
                else if (key == "" && User.UserId > 0)
                {
                    //user is logged in
                    return true;
                }
            }
            return false;
        }

        public static Core.IUser GetUser(Core.Service service)
        {
            var newservice = GetService(service);
            return newservice.User;
        }
    }
}
