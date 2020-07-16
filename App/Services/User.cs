using System.Text.Json;

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
            if (Server.resetPass == true)
            {
                var update = false; //security check
                var emailAddr = "";
                var adminId = 1;
                if (Server.resetPass == true)
                {
                    //securely change admin password
                    //get admin email address from database
                    emailAddr = Query.Users.GetEmail(adminId);
                    if (emailAddr != "" && emailAddr != null) { update = true; }
                }
                if (update == true)
                {
                    Query.Users.UpdatePassword(adminId, EncryptPassword(emailAddr, password));
                    Server.resetPass = false;
                }
                return Success();
            }
            Context.Response.StatusCode = 500;
            return "";
        }

        public string CreateAdminAccount(string name, string email, string password)
        {
            if (Server.hasAdmin == false)
            {
                Query.Users.CreateUser(new Query.Models.User()
                {
                    name = name,
                    email = email,
                    password = EncryptPassword(email, password)
                });
                Server.hasAdmin = true;
                Server.resetPass = false;
                return "success";
            }
            Context.Response.StatusCode = 500;
            return "";
        }

        public void LogOut()
        {
            User.LogOut();
        }

        public string EncryptPassword(string email, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(email + Server.salt + password, Server.bcrypt_workfactor);

        }

        public bool DecryptPassword(string email, string password, string encrypted)
        {
            return BCrypt.Net.BCrypt.Verify(email + Server.salt + password, encrypted);
        }
    }
}