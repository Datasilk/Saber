using System;
using System.Linq;
using System.Net.Mail;
using Saber.Core.Extensions.Strings;
using Saber.Vendor;

namespace Saber.Services
{
    public class User : Service
    {
        public string homePath = "home"; //user home path used to redirect after user log in success

        public string Authenticate(string email, string password)
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            var encrypted = Query.Users.GetPassword(email);
            if(encrypted == null) { return Error(); }
            if (!DecryptPassword(email, password, encrypted)) { return Error(); }
            {
                //password verified by Bcrypt
                var user = Query.Users.Authenticate(email, encrypted);
                if (user != null)
                {
                    User.LogIn(user.userId, user.email, user.name, user.datecreated, user.photo, user.isadmin);
                    User.Save(true);
                    return JsonResponse(new { redirect = homePath });
                }
            }
            return Error();
        }

        public string OAuth(string clientId, string email, string password)
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            if(clientId == "") { return Error("Client ID was not provided");  }
            if(!Server.DeveloperKeys.Any(a => a.Client_ID == clientId)) { return Error("Client ID not found"); }
            var encrypted = Query.Users.GetPassword(email);
            if (encrypted == null) { return Error(); }
            if (!DecryptPassword(email, password, encrypted)) { return Error(); }
            {
                //password verified by Bcrypt
                var user = Query.Users.Authenticate(email, encrypted);
                if (user != null)
                {
                    //TODO: generate temporary code for user account & save in database
                    var code = "notimplimented";
                    return Server.DeveloperKeys.Where(a => a.Client_ID == clientId).First().Redirect_URI + "?code=" + code;
                }
            }
            return Error();
        }

        [PublicApi("Generate an OAuth 2.0 persistent token for user authentication.", "A temporary random string provided after the user logs into their account")]
        public string Token(string code)
        {
            //TODO: generate OAuth 2.0 token for user authenication from temporary code
            return Error("Endpoint not yet implemented");
        }

        [PublicApi("Generate an OAuth 2.0 persistent token for user authentication by supplying an expired token.", "The expired persistent token associated with a user account")]
        public string NewToken(string oldtoken)
        {
            //TODO: generate OAuth 2.0 token for user authenication from expired token
            return Error("Endpoint not yet implemented");
        }

        public string CreateAdminAccount(string name, string email, string password, string password2)
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            if (Server.HasAdmin == true) { return Error(); }
            if (!CheckEmailAddress(email)) { return Error("Email address is invalid"); }
            if (password != password2) { return Error("Passwords do not match"); }
            try
            {
                CheckPassword(password);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
            if (string.IsNullOrEmpty(name)) { return Error("Please specify your name"); }
            var userId = Query.Users.CreateUser(new Query.Models.User()
            {
                name = name,
                email = email,
                password = EncryptPassword(email, password),
                isadmin = true
            }, true);
            Server.HasAdmin = true;
            Server.ResetPass = false;

            //raise Saber Event on all supported Vendor plugins
            Core.Vendors.EventHandlers.ForEach(a =>
            {
                a.CreatedUser(userId, name, email);
            });
            return "success";
        }

        [PublicApi("Create a new public user account", "Display name of the user", "Valid email address used to send an authentication email", "A password that adheres to the current password policies", "Force the user to retype their password")]
        public string SignUp(string name, string emailaddr, string password, string password2)
        {
            if (!CheckEmailAddress(emailaddr)) { return Error("Email address is invalid"); }
            if (Query.Users.Exists(emailaddr)) { return Error("Another account is already using the email address \"" + emailaddr + "\""); }
            if(password != password2) { return Error("Passwords do not match"); }
            try
            {
                CheckPassword(password);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
            if (string.IsNullOrEmpty(name)) { return Error("Please specify your name"); }
            var activationkey = Generate.NewId(16);
            var userId = Query.Users.CreateUser(new Query.Models.User()
            {
                name = name,
                email = emailaddr,
                password = EncryptPassword(emailaddr, password),
                tempkey = activationkey
            });

            //send signup activation email
            var viewEmail = new View("/Content/emails/signup.html");
            viewEmail["userId"] = userId.ToString();
            viewEmail["name"] = name;
            viewEmail["email"] = emailaddr;
            viewEmail["activation-key"] = activationkey;
            var msg = Core.Email.Create(Core.Email.From, new MailAddress(emailaddr, name), "Welcome to Saber!", viewEmail.Render());
            Core.Email.Send(msg, "signup");

            //raise Saber Event on all supported Vendor plugins
            Core.Vendors.EventHandlers.ForEach(a =>
            {
                a.CreatedUser(userId, name, emailaddr);
            });

            return "success";
        }

        [PublicApi("Request that an activation email be sent to a new user account", "A valid email address associated with the user's account")]
        public string RequestActivation(string emailaddr)
        {
            if (Query.Users.CanActivate(emailaddr))
            {
                var activationkey = Generate.NewId(16);
                Query.Users.RequestActivation(emailaddr, activationkey);
                //TODO: send signup activation email
            }
            return Success();
        }

        public string Activate(string emailaddr, string activationkey)
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            if(Query.Users.Activate(emailaddr, activationkey))
            {
                return Success();
            }
            else {
                return Error();
            }
        }

        public string ForgotPassword(string emailaddr)
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            if (!Query.Users.CanActivate(emailaddr))
            {
                var activationkey = Generate.NewId(16);
                Query.Users.ForgotPassword(emailaddr, activationkey);
                Query.Logs.LogForgotPassword(emailaddr);
                var viewEmail = new View("/Content/emails/forgot-pass.html");
                viewEmail["email"] = emailaddr;
                viewEmail["url"] = App.Host + "/resetpass#" + activationkey;
                viewEmail["activation-key"] = activationkey;
                var msg = Core.Email.Create(Core.Email.From, new MailAddress(emailaddr), "Password reset for Saber", viewEmail.Render());
                Core.Email.Send(msg, "updatepass");
                return Success();
            }
            return Error("Email is not eligible for a password reset");
        }

        public string ResetPassword(string key, string password, string password2)
        {
            if (IsPublicApiRequest) { return AccessDenied(); }
            if (password != password2) { return Error("Passwords do not match"); }
            try
            {
                CheckPassword(password);
            }catch(Exception ex)
            {
                return Error(ex.Message);
            }
            var email = Query.Users.GetEmailFromResetKey(key);
            if (string.IsNullOrEmpty(email))
            {
                return Error("Password reset authentication key has expired.");
            }
            if (Query.Users.ResetPassword(EncryptPassword(email, password), key))
            {
                return Success();
            }
            else
            {
                return Error("Password reset authentication key expired.");
            }
        }

        public void LogOut()
        {
            User.LogOut();
        }

        private bool CheckEmailAddress(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public void CheckPassword(string password)
        {
            if (IsPublicApiRequest) { return; }
            var config = Common.Platform.Website.Settings.Load();
            if (password.Length < config.Passwords.MinChars)
            {
                throw new Exception("Password must be at least " + config.Passwords.MinChars + " characters in length");
            }
            if (password.Length > config.Passwords.MaxChars)
            {
                throw new Exception("Password must be less than " + (config.Passwords.MaxChars + 1) + " characters in length");
            }
            char lastchar = char.MaxValue;
            var numbers = 0;
            var uppercase = 0;
            var special = 0;
            var spaces = 0;
            var consecutives = 0;
            var maxconsecutives = 0;
            for (var x = 0; x < password.Length; x++)
            {
                var p = password[x];
                if(lastchar == p)
                {
                    consecutives++;
                    if(consecutives > maxconsecutives)
                    {
                        maxconsecutives = consecutives;
                    }
                }
                else if(consecutives > 0)
                {
                    consecutives = 0;
                }
                if (char.IsNumber(p)) { numbers++; }
                else if (char.IsUpper(p)) { uppercase++; }
                else if (p == ' ') { spaces++; }
                else if (char.IsLetter(p)) { }
                else { special++; }
            }

            if (numbers < config.Passwords.MinNumbers)
            {
                throw new Exception("Password must contain at least " + config.Passwords.MinNumbers + " number" + (config.Passwords.MinNumbers > 1 ? "s" : ""));
            }
            if (numbers < config.Passwords.MinUppercase)
            {
                throw new Exception("Password must contain at least " + config.Passwords.MinUppercase + " uppercase letter" + (config.Passwords.MinUppercase > 1 ? "s" : ""));
            }
            if (numbers < config.Passwords.MinSpecialChars)
            {
                throw new Exception("Password must contain at least " + config.Passwords.MinSpecialChars + " special character" + (config.Passwords.MinSpecialChars > 1 ? "s" : ""));
            }
            if (config.Passwords.NoSpaces && spaces > 0)
            {
                throw new Exception("Password cannot contain spaces");
            }
        }

        private string EncryptPassword(string email, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(email + Server.Salt + password, Server.BcryptWorkfactor);

        }

        private bool DecryptPassword(string email, string password, string encrypted)
        {
            return BCrypt.Net.BCrypt.Verify(email + Server.Salt + password, encrypted);
        }
    }
}