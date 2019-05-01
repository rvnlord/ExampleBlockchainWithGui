using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string EnumToString(this Enum en, bool toLower = false, string betweenWords = "")
        {
            var name = Enum.GetName(en.GetType(), en);
            if (!string.IsNullOrEmpty(betweenWords))
                name = Regex.Replace(name ?? "", @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", $"{betweenWords}$0");
            if (toLower)
                name = name?.ToLower();
            return name;
        }

        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return null;
            var field = type.GetField(name);
            if (field == null) return null;
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attr?.Description;
        }
    }
}
