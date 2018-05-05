namespace Saber.Tests.Models
{
    class Authenticate
    {
        public string email;
        public string password;

        public Authenticate(string email, string pass)
        {
            this.email = email;
            password = pass;
        }
    }
}
