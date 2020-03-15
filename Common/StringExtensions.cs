using System;
using System.Text;
using Newtonsoft.Json;

namespace Common
{
    public static class StringExtensions
    {
        public static byte[] GetBytes(this string self)
        {
            return Encoding.UTF8.GetBytes(self);
        }

        public static bool IsEmpty(this string self)
        {
            if (self is null)
                return true;

            return self.Length == 0;
        }

        public static string ToUtf8Base64String(this string self)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(self));
        }
        
        public static T FromJson<T>(this string self)
        {
            if (self.IsEmpty())
                return default(T);

            return JsonConvert.DeserializeObject<T>(self);
        }

        public static string ToJson(this object self)
        {
            return JsonConvert.SerializeObject(self);
        }
    }
}