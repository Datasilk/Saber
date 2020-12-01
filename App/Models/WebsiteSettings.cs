namespace Saber.Models.Website
{
    public class Settings
    {
        public Email Email { get; set; } = new Email();
    }

    public class Email
    {
        public Smtp Smtp { get; set; } = new Smtp();
        public EmailActions Actions { get; set; } = new EmailActions();
    }

    public class Smtp
    {
        public string Domain { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EmailActions
    {
        public string SignUp { get; set; }
        public string ForgotPass { get; set; }
        public string Newsletter { get; set; }
    }
}
