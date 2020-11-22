﻿using System.Collections.Generic;

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
                new { user.name, user.email, user.password, user.photo }
            );
        }

        public static Models.User AuthenticateUser(string email, string password)
        {
            var list = Sql.Populate<Models.User>("User_Authenticate", new { email, password });
            if (list.Count > 0) { return list[0]; }
            return null;
        }

        public static Models.User AuthenticateUser(string token)
        {
            var list = Sql.Populate<Models.User>("User_AuthenticateByToken", new { token });
            if (list.Count > 0) { return list[0]; }
            return null;
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

        public static void UpdatePassword(int userId, string password)
        {
            Sql.ExecuteNonQuery("User_UpdatePassword", new { userId, password });
        }

        public static string GetEmail(int userId)
        {
            return Sql.ExecuteScalar<string>("User_GetEmail", new { userId });
        }

        public static string GetPassword(string email)
        {
            return Sql.ExecuteScalar<string>("User_GetPassword", new { email });
        }

        public static void UpdateEmail(int userId, string email)
        {
            Sql.ExecuteNonQuery("User_UpdateEmail", new { userId, email });
        }

        public static bool HasPasswords()
        {
            return Sql.ExecuteScalar<int>("Users_HasPasswords") == 1;
        }

        public static bool HasAdmin()
        {
            return Sql.ExecuteScalar<int>("Users_HasAdmin") == 1;
        }

        public static List<Models.UserWithSecurityCount> GetList(int page = 1, int length = 25, string search = "", int orderby = 1)
        {
            return Sql.Populate<Models.UserWithSecurityCount>("Users_GetList", new { page, length, search, orderby });
        }
    }
}
