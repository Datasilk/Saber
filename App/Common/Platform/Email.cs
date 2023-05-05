using System.Net.Mail;
using MimeKit;
using Saber.Vendor;

namespace Saber.Common.Platform
{
    public static class Email
    {

        public static List<EmailAction> Types = new List<EmailAction>()
        {
            new EmailAction()
            {
                Key = "signup",
                Name = "Sign Up",
                Description = "Email sent when a user creates a new account",
                TemplateFile = "signup.html",
                UserDefinedSubject = true
            },
            new EmailAction()
            {
                Key="forgotpass",
                Name = "Recover Password",
                Description = "Email sent when user requests to reset a forgotten password",
                TemplateFile = "forgot-pass.html",
                UserDefinedSubject = true
            },
            new EmailAction()
            {
                Key="activation",
                Name = "Activate Account",
                Description = "Email sent when a user manually requests thier account activation",
                TemplateFile = "activation.html",
                UserDefinedSubject = true
            }
        };

        public static void Send(MailMessage message, string type)
        {
            var config = Website.Settings.Load();
            var action = config.Email.Actions.Where(a => a.Type == type).FirstOrDefault();
            if(action == null)
            {
                //log error, could not send email
                Query.Logs.LogError(0, "", "Email.Send", "Could not find Email Action Type \"" + type + "\"", "");
                return;
            }

            var clientConfig = GetClientConfig(action.ClientId);
            var client = GetClient(clientConfig.Key);
            if (client == null)
            {
                //log error, could not send email
                Query.Logs.LogError(0, "", "Email.Send", "Could not find Email Client \"" + action.ClientId + "\" for action type " + type, "");
                return;
            }
            var _msg = "";

            try
            {
                client.Send(clientConfig, message, delegate () {
                    //only get RFC 2822 message if vendor plugin specifically requests it
                    if (string.IsNullOrEmpty(_msg))
                    {
                        _msg = GetRFC2822FormattedMessage(message);
                    }
                    return _msg;
                });
            }
            catch(Exception ex)
            {
                Query.Logs.LogError(0, "", "Email.Send", ex.Message, ex.StackTrace, "to: <" + string.Join(", ", message.To.Select(a => a.Address)) + ">, subject: " + message.Subject);
                throw new Exception(ex.Message, ex);
            }
        }

        public static string GetRFC2822FormattedMessage(MailMessage message)
        {
            return MimeMessage.CreateFromMailMessage(message).ToString();
        }

        public static List<EmailAction> Actions
        {
            get
            {
                var actions = new List<EmailAction>();
                actions.AddRange(Types);
                actions.AddRange(Core.Vendors.EmailTypes.Values);
                return actions;
            }
        }

        public static EmailAction GetAction(string key)
        {
            return Actions.FirstOrDefault(a => a.Key == key);
        }

        public static Models.Website.EmailAction GetActionConfig(string key)
        {
            var config = Website.Settings.Load();
            return config.Email.Actions.FirstOrDefault(a => a.Type == key);
        }

        public static Models.Website.EmailClient GetClientConfig(Guid Id)
        {
            var config = Website.Settings.Load();
            return config.Email.Clients.FirstOrDefault(a => a.Id == Id);
        }

        public static IVendorEmailClient GetClient(string key)
        {
            return Core.Vendors.EmailClients.Where(a => a.Key == key).FirstOrDefault().Value;
        }

        public static IVendorEmailClient GetClientForAction(EmailAction action)
        {
            return GetClientForAction(action.Key);
        }

        public static IVendorEmailClient GetClientForAction(string key)
        {
            var configAction = GetActionConfig(key);
            if(configAction != null)
            {
                var configClient = GetClientConfig(configAction.ClientId);
                if (configClient != null)
                {
                    return GetClient(configClient.Key);
                }
            }
            return null;
        }
    }
}
