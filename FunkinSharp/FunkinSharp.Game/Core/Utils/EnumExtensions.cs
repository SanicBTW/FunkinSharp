using System;
using System.Reflection;

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
        /// <summary>
        ///     Gets the <see cref="StringValueAttribute.Value"/> from an <see cref="Enum"/> Value that has the attribute of <see cref="StringValueAttribute"/>.
        /// </summary>
        /// <typeparam name="T">An <see cref="Enum"/></typeparam>
        /// <param name="value">A Value of type <typeparamref name="T"/> with the attribute <see cref="StringValueAttribute"/>.</param>
        /// <returns>The <see cref="StringValueAttribute.Value"/> from <paramref name="value"/></returns>
        public static string GetString<T>(this T value) where T : Enum
        {
            MemberInfo[] memberInfo = typeof(T).GetMember(value.ToString());
            object[] attribute = memberInfo[0].GetCustomAttributes(typeof(StringValueAttribute), false);
            return attribute.Length > 0 ? ((StringValueAttribute)attribute[0]).Value : null;
        }
    }
}
