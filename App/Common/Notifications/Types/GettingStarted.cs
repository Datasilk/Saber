using Saber.Core.Vendor;
using Saber.Models;

namespace Saber.Common.Notifications.Types
{
    public class GettingStarted : NotificationType
    {
        public override string Type { get; set; } = "start";
        public override string Icon { get; set; } = "icon-check";

        public override Notification[] GetDynamicList()
        {
            var notifs = new List<Notification>();
            var settings = Platform.Website.Settings.Load();

            //check all email actions
            if(settings.Email.Actions.Count > 0)
            {
                var missingClient = false;
                var missingSubject = false;
                foreach (var action in settings.Email.Actions)
                {
                    if (action.Client == "")
                    {
                        missingClient = true;
                    }
                    if(action.Subject == "")
                    {
                        missingSubject = true;
                    }
                    if(missingClient && missingSubject) { break; }
                }
                if (missingClient)
                {
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
                    notifs.Add(new Notification()
                    {
                        notification = "One or more of your Email Actions are missing a Subject",
                        url = "javascript:S.editor.websettings.show('email-settings')",
                        type = Type,
                        datecreated = DateTime.Now,
                        notifId = Guid.Empty
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
