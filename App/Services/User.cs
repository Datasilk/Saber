using Microsoft.AspNetCore.Http;

namespace Saber.Services
{
    public class User : Service
    {
        public string homePath = "home"; //user home path used to redirect after user log in success

        public User(HttpContext context) : base(context)
        {
        }

        public string Authenticate(string email, string password)
        {

            //var sqlUser = new SqlQueries.User(S);
            var query = new Query.Users(server.sqlConnectionString);
            var encrypted = query.GetPassword(email);
            if (!DecryptPassword(email, password, encrypted)) { return Error(); }
            {
                //password verified by Bcrypt
                var user = query.AuthenticateUser(email, encrypted);
                if (user != null)
                {
                    User.userId = user.userId;
                    User.email = email;
                    User.photo = user.photo;
                    User.name = user.name;
                    User.datecreated = user.datecreated;
                    User.Save(true);
                    return "success|" + homePath;
                }
            }
            return Error();
        }

        public string SaveAdminPassword(string password)
        {
            if (server.resetPass == true)
            {
                var update = false; //security check
                var emailAddr = "";
                var queryUser = new Query.Users(server.sqlConnectionString);
                var adminId = 1;
                if (server.resetPass == true)
                {
                    //securely change admin password
                    //get admin email address from database
                    emailAddr = queryUser.GetEmail(adminId);
                    if (emailAddr != "" && emailAddr != null) { update = true; }
                }
                if (update == true)
                {
                    queryUser.UpdatePassword(adminId, EncryptPassword(emailAddr, password));
                    server.resetPass = false;
                }
                return Success();
            }
            context.Response.StatusCode = 500;
            return "";
        }

        public string CreateAdminAccount(string name, string email, string password)
        {
            if (server.hasAdmin == false && server.environment == Server.enumEnvironment.development)
            {
                var queryUser = new Query.Users(server.sqlConnectionString);
                queryUser.CreateUser(new Query.Models.User()
                {
                    name = name,
                    email = email,
                    password = EncryptPassword(email, password)
                });
                server.hasAdmin = true;
                server.resetPass = false;
                return "success";
            }
            context.Response.StatusCode = 500;
            return "";
        }

        public void LogOut()
        {
            User.LogOut();
        }

        public string EncryptPassword(string email, string password)
        {
            var bCrypt = new BCrypt.Net.BCrypt();
            return BCrypt.Net.BCrypt.HashPassword(email + server.salt + password, server.bcrypt_workfactor);

        }

        public bool DecryptPassword(string email, string password, string encrypted)
        {
            return BCrypt.Net.BCrypt.Verify(email + server.salt + password, encrypted);
        }
    }
}