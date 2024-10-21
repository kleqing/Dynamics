using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dynamics.Models.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Dynamics.Utility
{
    public static class SessionExtension
    {
        //public static void Set<T>(this ISession session, string key, T value)
        //{
        //    session.SetString(key, JsonSerializer.Serialize(value));
        //}

        //public static T? Get<T>(this ISession session, string key)
        //{
        //    var value = session.GetString(key);
        //    return value == null ? default : JsonSerializer.Deserialize<T>(value);
        //}


        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        };

        public static void Set<T>(this ISession session, string key, T value)
        {
            var jsonValue = JsonSerializer.Serialize(value, jsonOptions);
            session.SetString(key, jsonValue);
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value, jsonOptions);
        }

        /**
         * Get the current user set in the session
         */
        public static User? GetCurrentUser(this ISession session)
        {
            var userJson = session.GetString("user");
            User currentUser;
            return !string.IsNullOrEmpty(userJson) ? JsonConvert.DeserializeObject<User>(userJson) : null;
        }
        public static void SetCurrentUser(this ISession session, User user)
        {
            session.SetString("user", JsonConvert.SerializeObject(user));
        }
    }
}
