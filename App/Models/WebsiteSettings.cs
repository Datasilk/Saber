using System.Collections.Generic;

namespace Saber.Models.Website
{
    public class Settings
    {
        public Email Email { get; set; } = new Email();
        public Passwords Passwords { get; set; } = new Passwords();
        public List<Language> Languages { get; set; } = new List<Language>();
        public List<PageTitle> PageTitles { get; set; } = new List<PageTitle>();
        public List<string> Stylesheets { get; set; } = new List<string>();
        public List<string> Scripts { get; set; } = new List<string>();
    }

    public class Email
    {
        public Smtp Smtp { get; set; } = new Smtp();
        public List<EmailAction> Actions { get; set; } = new List<EmailAction>()
        {
            new EmailAction() { Type = "signup", Subject = "Welcome to Saber!" },
            new EmailAction() { Type = "forgotpass", Subject = "Saber Password Reset" },
            new EmailAction() { Type = "newsletter" }
        };
    }

    public class Smtp
    {
        public string Domain { get; set; } = "";
        public int Port { get; set; } = 25;
        public bool SSL { get; set; } = false;
        public string From { get; set; } = "";
        public string FromName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class EmailAction
    {
        public string Type { get; set; }
        public string Client { get; set; }
        public string Subject { get; set; }
    }

    public class Passwords
    {
        public int MinChars { get; set; } = 8;
        public int MaxChars { get; set; } = 16;
        public int MinNumbers { get; set; } = 1;
        public int MinUppercase { get; set; } = 1;
        public int MinSpecialChars { get; set; } = 0;
        public bool NoSpaces { get; set; } = true;
        public int MaxConsecutiveChars { get; set; } = 3;
    }

    public class Language
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public enum PageTitleType
    {
        Prefix = 0,
        Suffix = 1
    }

    public class PageTitle
    {
        public string Value { get; set; }
        public PageTitleType Type { get; set; }
    }
}
