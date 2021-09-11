namespace Saber.Common.Platform
{
    public static class Controller
    {
        private static Saber.Controller GetController(Core.Controller controller)
        {
            var newcontroller = new Saber.Controller();
            newcontroller.Context = controller.Context;
            newcontroller.Parameters = controller.Parameters;
            newcontroller.Path = controller.Path;
            newcontroller.PathParts = controller.PathParts;
            newcontroller.Session = controller.Session;
            return newcontroller;
        }
        /// <summary>
        /// Set Saber.Controller as the type to use when executing CheckSecurity
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CheckSecurity(Core.Controller controller, string key = "")
        {
            var newcontroller = GetController(controller);
            return newcontroller.CheckSecurity(key);
        }

        public static Core.IUser GetUser(Core.Controller controller)
        {
            var newcontroller = GetController(controller);
            return newcontroller.User;
        }
    }
}
