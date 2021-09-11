
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
        public static bool CheckSecurity(Core.Service service, string key = "")
        {
            var newservice = new Saber.Service()
            {
                Context = service.Context,
                Parameters = service.Parameters,
                Path = service.Path,
                PathParts = service.PathParts
            };
            return newservice.CheckSecurity(key);
        }

        public static Core.IUser GetUser(Core.Service service)
        {
            var newservice = GetService(service);
            return newservice.User;
        }
    }
}
