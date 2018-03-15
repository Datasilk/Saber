namespace Saber
{
    public class Service : Datasilk.Service
    {
        private User _userInfo;

        public Service(global::Core DatasilkCore) : base(DatasilkCore) {}

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }

        public User UserInfo
        {
            get
            {
                if (_userInfo == null) { _userInfo = new User(S); }
                return _userInfo;
            }
        }
    }
}