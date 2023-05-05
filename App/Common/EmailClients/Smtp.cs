using System.Net.Mail;
using MimeKit;
using Saber.Models.Website;
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

        public bool IsConfigured(EmailClient emailClient)
        {
            return !string.IsNullOrEmpty(emailClient.Parameters["domain"])
                && !string.IsNullOrEmpty(emailClient.Parameters["port"])
                && !string.IsNullOrEmpty(emailClient.Parameters["from"])
                && !string.IsNullOrEmpty(emailClient.Parameters["from-name"])
                && !string.IsNullOrEmpty(emailClient.Parameters["user"])
                && !string.IsNullOrEmpty(emailClient.Parameters["pass"]);
        }

        public void Validate(EmailClient emailClient)
        {
            if (string.IsNullOrEmpty(emailClient.Parameters["domain"]))
            {
                throw new Exception("Domain is a required field");
            }
            if (string.IsNullOrEmpty(emailClient.Parameters["port"]))
            {
                throw new Exception("Port is a required field");
            }
            if (string.IsNullOrEmpty(emailClient.Parameters["from"]))
            {
                throw new Exception("From Address is a required field");
            }
            if (string.IsNullOrEmpty(emailClient.Parameters["from-name"]))
            {
                throw new Exception("From Name is a required field");
            }
            if (string.IsNullOrEmpty(emailClient.Parameters["user"]))
            {
                throw new Exception("Username is a required field");
            }
            if (string.IsNullOrEmpty(emailClient.Parameters["pass"]))
            {
                throw new Exception("Password is a required field");
            }
        }

        public void Send(EmailClient emailClient, MailMessage message, Func<string> GetRFC2822)
        {
            try
            {
                var client = new MailKit.Net.Smtp.SmtpClient();
                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(emailClient.Parameters[FromNameKey], emailClient.Parameters[FromKey]));
                foreach (var to in message.To)
                {
                    msg.To.Add(new MailboxAddress(to.DisplayName, to.Address));
                }
                msg.Subject = message.Subject;

                msg.Body = new TextPart("html")
                {
                    Text = message.Body
                };

                client.Connect(emailClient.Parameters["domain"], int.Parse(emailClient.Parameters["port"]), bool.Parse(emailClient.Parameters["ssl"]));
                //disable the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(emailClient.Parameters["user"], emailClient.Parameters["pass"]);
                client.Send(msg);
                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                Query.Logs.LogError(0, "", "Email.Smtp.Send", ex.Message, ex.StackTrace, "to: <" + string.Join(", ", message.To.Select(a => a.Address)) + ">, subject: " + message.Subject);
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
