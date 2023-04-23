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
                        notification = "One or more of your Email Actions are missing a selected Email Client",
                        url = "javascript:S.editor.websettings.show('email-settings')",
                        type = Type,
                        datecreated = DateTime.Now,
                        notifId = Guid.Empty
                    });
                }
                if (missingSubject)
                {
                    //one or more actions are missing a subject
                    notifs.Add(new Notification()
                    {
                        notification = "One or more of your Email Actions are missing a Subject",
                        url = "javascript:S.editor.websettings.show('email-settings')",
                        type = Type,
                        datecreated = DateTime.Now,
                        notifId = Guid.Empty
                    });
                }

                //check all clients being used by actions for configuration
                foreach(var key in usedClients)
                {
                    if(Core.Vendors.EmailClients.ContainsKey(key))
                    {
                        var client = Core.Vendors.EmailClients[key];
                        if (!client.IsConfigured())
                        {
                            notifs.Add(new Notification()
                            {
                                notification = "Your " + client.Name + " Email Client is not configured to send emails yet",
                                url = "javascript:S.editor.websettings.show('email-settings')",
                                type = Type,
                                datecreated = DateTime.Now,
                                notifId = Guid.Empty
                            });
                        }
                    }
                }
            }
            else
            {

            }

            return notifs.ToArray();
        }
    }
}
