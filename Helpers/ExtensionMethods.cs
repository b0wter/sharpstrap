using System;

namespace Cootstrap.Helpers
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static bool IsNotNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s) == false;
        }
    }
}