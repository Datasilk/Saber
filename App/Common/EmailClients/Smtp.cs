using System.Net.Mail;
using MimeKit;
using Saber.Vendor;

namespace Saber.Common.EmailClients
{
    public class Smtp : IVendorEmailClient
    {
        public string Key { get; } = "smtp";
        public string Name { get; } = "SMTP Server";
        public string FromKey { get; } = "from";
        public string FromNameKey { get; } = "from-name";

        public Dictionary<string, EmailClientParameter> Parameters { get; set; } = new Dictionary<string, EmailClientParameter>()
            {
                {
                    "domain",
                    new EmailClientParameter()
                    {
                        Name = "Host",
                        DataType = EmailClientDataType.Text,
                        Description = "The remote domain name or IP address where your email server resides."
                    }
                },
                {
                    "port",
                    new EmailClientParameter()
                    {
                        Name = "Port",
                        DataType = EmailClientDataType.Number,
                        Description = "Port number where your email server resides. Default is port 25."
                    }
                },
                {
                    "ssl",
                    new EmailClientParameter()
                    {
                        Name = "Use SSL",
                        DataType = EmailClientDataType.Boolean,
                        Description = "Whether or not the connection to your email server uses SSL."
                    }
                },
                {
                    "from",
                    new EmailClientParameter()
                    {
                        Name = "From Address",
                        DataType = EmailClientDataType.Text,
                        Description = "The email address used to send emails to your users with on behalf of your website."
                    }
                },
                {
                    "from-name",
                    new EmailClientParameter()
                    {
                        Name = "From Name",
                        DataType = EmailClientDataType.Text,
                        Description = "The name of the person sending emails on behalf of your website."
                    }
                },
                {
                    "user",
                    new EmailClientParameter()
                    {
                        Name = "Username / Email",
                        DataType = EmailClientDataType.Text,
                        Description = "The username or email used to authenticate before sending an email via SMTP."
                    }
                },
                {
                    "pass",
                    new EmailClientParameter()
                    {
                        Name = "Password",
                        DataType = EmailClientDataType.Password,
                        Description = "The password used to authenticate before sending an email via SMTP."
                    }
                }
            };

        public Dictionary<string, string> GetConfig()
        {
            var config = Platform.Website.Settings.Load();
            return new Dictionary<string, string>()
                {
                    { "domain", config.Email.Smtp.Domain },
                    { "port", config.Email.Smtp.Port.ToString() },
                    { "ssl", config.Email.Smtp.SSL ? "1" : "0" },
                    { "from", config.Email.Smtp.From },
                    { "from-name", config.Email.Smtp.FromName },
                    { "user", config.Email.Smtp.Username },
                    { "pass", config.Email.Smtp.Password }
                };
        }

        public bool IsConfigured()
        {
            var config = Platform.Website.Settings.Load();
            if (string.IsNullOrEmpty(config.Email.Smtp.Domain)) { return false; }
            if (config.Email.Smtp.Port <= 0) { return false; }
            if (string.IsNullOrEmpty(config.Email.Smtp.From)) { return false; }
            if (string.IsNullOrEmpty(config.Email.Smtp.FromName)) { return false; }
            if (string.IsNullOrEmpty(config.Email.Smtp.Username)) { return false; }
            if (string.IsNullOrEmpty(config.Email.Smtp.Password)) { return false; }
            return true;
        }

        public void Init()
        {

        }

        public void SaveConfig(Dictionary<string, string> parameters)
        {
            var config = Platform.Website.Settings.Load();
            int.TryParse(parameters["port"], out var port);
            var ssl = parameters["ssl"] ?? "";
            var pass = parameters["pass"] ?? "";
            config.Email.Smtp.Domain = parameters["domain"] ?? "";
            config.Email.Smtp.Port = port;
            config.Email.Smtp.SSL = ssl.ToLower() == "true";
            config.Email.Smtp.From = parameters["from"];
            config.Email.Smtp.FromName = parameters["from-name"];
            config.Email.Smtp.Username = parameters["user"];
            if (pass != "" && pass.Any(a => a != '*'))
            {
                config.Email.Smtp.Password = parameters["pass"];
            }
            Platform.Website.Settings.Save(config);
        }

        public void Send(MailMessage message, Func<string> GetRFC2822)
        {
            try
            {
                var config = Platform.Website.Settings.Load().Email.Smtp;
                var client = new MailKit.Net.Smtp.SmtpClient();
                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(config.FromName, config.From));
                foreach (var to in message.To)
                {
                    msg.To.Add(new MailboxAddress(to.DisplayName, to.Address));
                }
                msg.Subject = message.Subject;

                msg.Body = new TextPart("html")
                {
                    Text = message.Body
                };

                client.Connect(config.Domain, config.Port, config.SSL);
                //disable the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(config.Username, config.Password);
                client.Send(msg);
                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                Query.Logs.LogError(0, "", "Email.Smtp.Send", ex.Message, ex.StackTrace);
            }
        }
    }
}
