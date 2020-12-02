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
        public string Domain { get; set; } = "";
        public int Port { get; set; } = 25;
        public bool SSL { get; set; } = false;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class EmailActions
    {
        public EmailAction SignUp { get; set; } = new EmailAction() { Subject = "Welcome to Saber!" };
        public EmailAction ForgotPass { get; set; } = new EmailAction() { Subject = "Saber Password Reset" };
        public EmailAction Newsletter { get; set; } = new EmailAction();
    }

    public class EmailAction
    {
        public string Client { get; set; }
        public string Subject { get; set; }
        public string File { get; set; }
    }
}
