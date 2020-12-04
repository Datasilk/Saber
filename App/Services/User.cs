using Saber.Core.Extensions.Strings;

namespace Saber.Services
{
    public class User : Service
    {
        public string homePath = "home"; //user home path used to redirect after user log in success

        public string Authenticate(string email, string password)
        {
            var encrypted = Query.Users.GetPassword(email);
            if(encrypted == null) { return Error(); }
            if (!DecryptPassword(email, password, encrypted)) { return Error(); }
            {
                //password verified by Bcrypt
                var user = Query.Users.AuthenticateUser(email, encrypted);
                if (user != null)
                {
                    User.LogIn(user.userId, user.email, user.name, user.datecreated, "", 1, user.photo);
                    User.Save(true);
                    return JsonResponse(new { redirect = homePath });
                }
            }
            return Error();
        }

        public string SaveAdminPassword(string password)
        {
            if (Server.ResetPass == true)
            {
                var update = false; //security check
                var emailAddr = "";
                var adminId = 1;
                if (Server.ResetPass == true)
                {
                    //securely change admin password
                    //get admin email address from database
                    emailAddr = Query.Users.GetEmail(adminId);
                    if (emailAddr != "" && emailAddr != null) { update = true; }
                }
                if (update == true)
                {
                    Query.Users.UpdatePassword(emailAddr, EncryptPassword(emailAddr, password));
                    Server.ResetPass = false;
                }
                return Success();
            }
            Context.Response.StatusCode = 500;
            return "";
        }

        public string CreateAdminAccount(string name, string email, string password)
        {
            if (!CheckEmailAddress(email)) { return Error("Email address is invalid"); }
            if (Server.HasAdmin == false)
            {
                Query.Users.CreateUser(new Query.Models.User()
                {
                    name = name,
                    email = email,
                    password = EncryptPassword(email, password)
                }, true);
                Server.HasAdmin = true;
                Server.ResetPass = false;
                return "success";
            }
            Context.Response.StatusCode = 500;
            return "";
        }

        public string SignUp(string name, string emailaddr, string password)
        {
            if (!CheckEmailAddress(emailaddr)) { return Error("Email address is invalid"); }
            if (Query.Users.Exists(emailaddr)) { return Error("Another account is already using the email address \"" + emailaddr + "\""); }
            //TODO: Check password strength
            if (string.IsNullOrEmpty(name)) { return Error("Please specify your name"); }
            var userId = Query.Users.CreateUser(new Query.Models.User()
            {
                name = name,
                email = emailaddr,
                password = EncryptPassword(emailaddr, password)
            });

            //TODO: send signup activation email

            return "success";
        }

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
            if (!Query.Users.CanActivate(emailaddr))
            {
                var activationkey = Generate.NewId(16);
                Query.Users.ForgotPassword(emailaddr, activationkey);
                //TODO: send forgot password email
                return Success();
            }
            return Error("Email is not eligible for a password reset");
        }

        public string ResetPassword(string emailaddr, string password, string activationkey)
        {
            if (Query.Users.ResetPassword(emailaddr, password, activationkey))
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
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
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