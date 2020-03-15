using System;

namespace Common
{
    public static class DateTimeExtensions
    {
        public static bool IsEmpty(this DateTime self)
        {
            return self == DateTime.MinValue;
        }
    }
}