using Saber.Core;
using Saber.Vendor;
using Saber.Models;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.Notifications.Types
{
    public class GettingStarted : NotificationType
    {
        public override string Type { get; set; } = "start";
        public override string Icon { get; set; } = "icon-check";

        public override Notification[] GetDynamicList(IUser user)
        {
            if(user.IsAdmin == false) { 
                //only admins should see these notifications
                return new Notification[] { };  
            }

            var notifs = new List<Notification>();
            var settings = Platform.Website.Settings.Load();

            //check security groups
            if(Query.Security.Groups.GetCount() == 0)
            {
                notifs.Add(new Notification()
                {
                    Text = "No <b>Security Groups</b> exist yet. You must create at least one that users can belong to.",
                    Url = "javascript:S.editor.security.show(S.editor.security.groups.create.show);",
                    Type = Type,
                    DateCreated = DateTime.Now,
                    NotifId = Guid.Empty
                });
            }

            //check for default security group
            if(settings.Users.groupId.HasValue == false || settings.Users.groupId.Value <= 0)
            {
                notifs.Add(new Notification()
                {
                    Text = "You must select a default <b>Security Group</b> to use for new users who signup for your website.",
                    Url = "javascript:S.editor.users.show(() => {setTimeout(S.editor.users.settings.show, 50)});",
                    Type = Type,
                    DateCreated = DateTime.Now,
                    NotifId = Guid.Empty
                });
            }

            //check all email actions & email clients configurations
            if (Platform.Email.Actions.Count > 0)
            {
                var missingClient = false;
                var missingSubject = false;
                var usedClients = new List<string>();
                foreach (var action in Platform.Email.Actions)
                {
                    var config = Platform.Email.GetActionConfig(action.Key);
                    if (string.IsNullOrEmpty(config.Client))
                    {
                        missingClient = true;
                    }
                    else if (!usedClients.Contains(config.Client))
                    {
                        usedClients.Add(config.Client);
                    }
                    if (action.UserDefinedSubject == true && config.Subject == "")
                    {
                        missingSubject = true;
                    }
                    if (missingClient && missingSubject) { break; }
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
                if (usedClients.Count > 0)
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

            //check website icons
            var webIconPath = App.MapPath("/wwwroot/images/web-icon.png");
            var hasWebIcon = false;
            if (File.Exists(webIconPath))
            {
                //compare temp web icon with live web icon
                var tmpIcon = Generate.MD5Hash(File.ReadAllText(App.MapPath("/Content/temp/images/web-icon.png")));
                var webIcon = Generate.MD5Hash(File.ReadAllText(webIconPath));
                if(tmpIcon != webIcon)
                {
                    //found unique web icon file
                    hasWebIcon = true;
                }
            }

            if(hasWebIcon == false)
            {
                //user hasn't changed web icon yet
                notifs.Add(new Notification()
                {
                    Text = "This website is using the default <b>Website Icon</b>. Please change the icon before publishing your website.",
                    Url = "javascript:S.editor.websettings.show('icons')",
                    Type = Type,
                    DateCreated = DateTime.Now,
                    NotifId = Guid.Empty
                });
            }

            return notifs.ToArray();
        }
    }
}
