using System;

namespace FunkinSharp.Game.Core.Utils
{
    // idk if i should rename it to EnumStringValue
    /// <summary>
    ///     Attribute for Enums to give them a String value
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        public string Value { get; }

        public StringValueAttribute(string value)
        {
            Value = value;
        }
    }

    public static class EnumExtensions
    {
        public static string GetString<T>(this T value) where T : Enum
        {
            var memberInfo = typeof(T).GetMember(value.ToString());
            var attribute = memberInfo[0].GetCustomAttributes(typeof(StringValueAttribute), false);
            return attribute.Length > 0 ? ((StringValueAttribute)attribute[0]).Value : null;
        }
    }
}
