using Saber.Core;
using Saber.Vendor;
using Saber.Models;

namespace Saber.Common.Notifications.Types
{
    public class GettingStarted : NotificationType
    {
        public override string Type { get; set; } = "start";
        public override string Icon { get; set; } = "icon-check";

        public override Notification[] GetDynamicList(IUser user)
        {
            var notifs = new List<Notification>();
            var settings = Platform.Website.Settings.Load();

            //check all email actions & email clients configurations
            if(settings.Email.Actions.Count > 0)
            {
                var missingClient = false;
                var missingSubject = false;
                var usedClients = new List<string>();
                foreach (var action in settings.Email.Actions)
                {
                    if (string.IsNullOrEmpty(action.Client))
                    {
                        missingClient = true;
                    }
                    else if(!usedClients.Contains(action.Client))
                    {

                    }
                    if(action.Subject == "")
                    {
                        missingSubject = true;
                    }
                    if(missingClient && missingSubject) { break; }
                }
                if (missingClient)
                {
                    //one or more actions are missing a client
                    notifs.Add(new Notification()
                    {
                        Text = "One or more of your <b>Email Actions</b> are missing a selected Email Client",
                        Url = "javascript:S.editor.websettings.show('email-settings')",
                        Type = Type,
                        DateCreated = DateTime.Now,
                        NotifId = Guid.Empty
                    });
                }
                if (missingSubject)
                {
                    //one or more actions are missing a subject
                    notifs.Add(new Notification()
                    {
                        Text = "One or more of your <b>Email Actions</b> are missing a Subject",
                        Url = "javascript:S.editor.websettings.show('email-settings')",
                        Type = Type,
                        DateCreated = DateTime.Now,
                        NotifId = Guid.Empty
                    });
                }

                //check all clients being used by actions for configuration
                if(usedClients.Count > 0)
                {
                    foreach (var key in usedClients)
                    {
                        if (Core.Vendors.EmailClients.ContainsKey(key))
                        {
                            var client = Core.Vendors.EmailClients[key];
                            if (!client.IsConfigured())
                            {
                                notifs.Add(new Notification()
                                {
                                    Text = "Your <b>" + client.Name + "</b> Email Client is not configured to send emails yet",
                                    Url = "javascript:S.editor.websettings.show('email-settings')",
                                    Type = Type,
                                    DateCreated = DateTime.Now,
                                    NotifId = Guid.Empty
                                });
                            }
                        }
                    }
                }
                else
                {
                    notifs.Add(new Notification()
                    {
                        Text = "No <b>Email Clients</b> are configured to send emails yet.",
                        Url = "javascript:S.editor.websettings.show('email-settings')",
                        Type = Type,
                        DateCreated = DateTime.Now,
                        NotifId = Guid.Empty
                    });
                }
            }
            else
            {

            }

            return notifs.ToArray();
        }
    }
}
