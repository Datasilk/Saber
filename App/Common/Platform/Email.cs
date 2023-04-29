using System.Net.Mail;
using MimeKit;
using Saber.Vendor;

namespace Saber.Common.Platform
{
    public static class Email
    {
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

            var client = Core.Vendors.EmailClients.Values.Where(a => a.Key == action.Client).FirstOrDefault();
            if (client == null)
            {
                //log error, could not send email
                Query.Logs.LogError(0, "", "Email.Send", "Could not find Email Client \"" + action.Client + "\" for action type " + type, "");
                return;
            }
            var _msg = "";

            try
            {
                client.Send(message, delegate () {
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
                Query.Logs.LogError(0, "", "Email.Send", ex.Message, ex.StackTrace);
                throw new Exception(ex.Message, ex);
            }
        }

        public static string GetRFC2822FormattedMessage(MailMessage message)
        {
            return MimeMessage.CreateFromMailMessage(message).ToString();
        }

        public static List<EmailType> Types = new List<EmailType>()
        {
            new EmailType()
            {
                Key = "signup",
                Name = "Sign Up",
                Description = "",
                TemplateFile = "signup.html",
                UserDefinedSubject = true
            },
            new EmailType()
            {
                Key="updatepass",
                Name = "Update Password",
                Description = "",
                TemplateFile = "update-pass.html",
                UserDefinedSubject = true
            },
            new EmailType()
            {
                Key="forgotpass",
                Name = "Recover Password",
                Description = "",
                TemplateFile = "forgot-pass.html",
                UserDefinedSubject = true
            }
        };

        public static List<EmailType> Actions
        {
            get
            {
                var actions = new List<EmailType>();
                actions.AddRange(Types);
                actions.AddRange(Core.Vendors.EmailTypes.Values);
                return actions;
            }
        }

        public static EmailType? GetAction(string key)
        {
            return Actions.Where(a => a.Key == key).FirstOrDefault();
        }

        public static Models.Website.EmailAction GetActionConfig(string key)
        {
            var config = Website.Settings.Load();
            return config.Email.Actions.Where(a => a.Type == key).FirstOrDefault() ?? new Models.Website.EmailAction();
        }

        public static IVendorEmailClient GetClientForAction(EmailType action)
        {
            return GetClientForAction(action.Key);
        }

        public static IVendorEmailClient GetClientForAction(string key)
        {
            //load website config
            var config = Website.Settings.Load();
            var configAction = config.Email.Actions.Where(a => a.Type == key).FirstOrDefault();
            if (configAction != null && Core.Vendors.EmailClients.ContainsKey(configAction.Client))
            {
                return Core.Vendors.EmailClients[configAction.Client];

            }
            return null;
        }
    }
}
