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
                    Query.Users.UpdatePassword(adminId, EncryptPassword(emailAddr, password));
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
                });
                Server.HasAdmin = true;
                Server.ResetPass = false;
                return "success";
            }
            Context.Response.StatusCode = 500;
            return "";
        }

        public string SignUp(string name, string emailaddr, string password, string password2)
        {
            if (!CheckEmailAddress(emailaddr)) { return Error("Email address is invalid"); }
            if (Query.Users.Exists(emailaddr)) { return Error("Another account is already using the email address \"" + emailaddr + "\""); }
            if (password == password2) { return Error("Passwords do not match"); }
            Query.Users.CreateUser(new Query.Models.User()
            {
                name = name,
                email = emailaddr,
                password = EncryptPassword(emailaddr, password)
            });
            Server.HasAdmin = true;
            Server.ResetPass = false;
            return "success";
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