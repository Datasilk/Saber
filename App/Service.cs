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
                    user = Saber.User.Get(Context);
                }
                return user;
            }
            set { user = value; }
        }
    }
}