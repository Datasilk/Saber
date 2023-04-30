using System;
using System.Collections.Generic;

namespace Query
{
    public static class Users
    {
        public static bool Exists(string email)
        {
            return Sql.ExecuteScalar<int>("User_Exists", new { email }) == 1;
        }

        public static int CreateUser(Models.User user)
        {
            return Sql.ExecuteScalar<int>("User_Create",
                new { user.name, user.email, user.password, user.photo, user.isadmin, user.tempkey, user.activate }
            );
        }

        public static Models.User Authenticate(string email, string password)
        {
            var list = Sql.Populate<Models.User>("User_Authenticate", new { email, password });
            if (list.Count > 0) { return list[0]; }
            return null;
        }

        public static Models.User Authenticate(string token, bool delete = true)
        {
            var list = Sql.Populate<Models.User>("User_AuthenticateByToken", new { token, delete });
            if (list.Count > 0) { return list[0]; }
            return null;
        }

        public static Models.User AuthenticateApi(string token)
        {
            var list = Sql.Populate<Models.User>("User_AuthenticateApiToken", new { token });
            if (list.Count > 0) { return list[0]; }
            return null;
        }

        public static bool CanActivate(string email)
        {
            return Sql.ExecuteScalar<int>("User_CanActivate", new { email }) == 1;
        }

        public static bool Activate(string tempkey)
        {
            return Sql.ExecuteScalar<int>("User_Activate", new { tempkey }) == 1;
        }

        public static void RequestActivation(string email, string tempkey)
        {
            Sql.ExecuteNonQuery("User_RequestActivation", new { email, tempkey });
        }

        public static bool ForgotPassword(string email, string tempkey)
        {
            return Sql.ExecuteScalar<int>("User_ForgotPassword", new { email, tempkey }) == 1;
        }

        public static string GetEmailFromResetKey(string tempkey)
        {
            return Sql.ExecuteScalar<string>("User_GetEmailFromResetKey", new { tempkey });
        }

        public static bool ResetPassword(string password, string tempkey)
        {
            return Sql.ExecuteScalar<int>("User_ResetPassword", new { password, tempkey }) == 1;
        }

        public static void UpdatePassword(string email, string password)
        {
            Sql.ExecuteNonQuery("User_UpdatePassword", new { email, password });
        }

        public static Models.User GetDetails(int userId)
        {
            var list = Sql.Populate<Models.User>("User_GetDetails", new { userId });
            if (list.Count > 0) { return list[0]; }
            return null;
        }

        public static string CreateAuthToken(int userId, int expireDays = 30)
        {
            return Sql.ExecuteScalar<string>("User_CreateAuthToken", new { userId, expireDays });
        }

        public static string CreateApiToken(int userId, string api, int expireDays = 730)
        {
            return Sql.ExecuteScalar<string>("User_CreateApiToken", new { userId, api, expireDays });
        }

        public static string GetEmail(int userId)
        {
            return Sql.ExecuteScalar<string>("User_GetEmail", new { userId });
        }

        public static string GetPassword(string email)
        {
            return Sql.ExecuteScalar<string>("User_GetPassword", new { email });
        }

        public static void UpdateEmail(int userId, string email, string password)
        {
            Sql.ExecuteNonQuery("User_UpdateEmail", new { userId, email, password });
        }

        public static void UpdateName(int userId, string name)
        {
            Sql.ExecuteNonQuery("User_UpdateName", new { userId, name });
        }

        public static bool HasPasswords()
        {
            return Sql.ExecuteScalar<int>("Users_HasPasswords") == 1;
        }

        public static bool HasAdmin()
        {
            return Sql.ExecuteScalar<int>("Users_HasAdmin") == 1;
        }

        public static void UpdateAdmin(int userId, bool isadmin)
        {
            Sql.ExecuteNonQuery("User_UpdateAdmin", new { userId, isadmin });
        }

        public static List<Models.UserWithSecurityCount> GetList(int page = 1, int length = 25, string search = "", int orderby = 1)
        {
            return Sql.Populate<Models.UserWithSecurityCount>("Users_GetList", new { page, length, search, orderby });
        }

        public static void Disable(int userId)
        {
            Sql.ExecuteNonQuery("User_Disable", new { userId });
        }

        public static void Enable(int userId)
        {
            Sql.ExecuteNonQuery("User_Enable", new { userId });
        }


        public static void PermDelete(int userId)
        {
            Sql.ExecuteNonQuery("User_PermDelete", new { userId });
        }

        public static int CreatedInTimeRange(int minutesAgo, DateTime? dateEnd = null)
        {
            return Sql.ExecuteScalar<int>("Users_CreatedInTimeRange", new { minutes = minutesAgo, dateend = dateEnd });
        }
    }
}
