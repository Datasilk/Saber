using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Saber.Common
{
    public static class Session
    {
        public static Dictionary<string, string> Get (string key, int ExpiresInMinutes)
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(Query.Sessions.Get(key, ExpiresInMinutes));
        }
        public static void Set(string key, string Serialize, int ExpiresInMinutes)
        {
            Query.Sessions.Set(key, Serialize, ExpiresInMinutes);
        }
    }
}
