using System;

namespace MementoHealth.Classes
{
    public static class StringExtensions
    {
        public static string Truncate(this string str, int maxLength) 
            => string.IsNullOrEmpty(str) ? str : (str.Substring(0, Math.Min(str.Length, Math.Max(0, maxLength - 3))) + "...");
    }
}