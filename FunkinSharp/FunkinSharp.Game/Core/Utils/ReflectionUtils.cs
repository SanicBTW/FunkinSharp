using System;
using System.Reflection;

namespace FunkinSharp.Game.Core.Utils
{
    // Wrapper class for Reflection
    public static class ReflectionUtils
    {
        public static ConstructorInfo GetConstructorFrom<T>(BindingFlags bindFlags, Type[] argTypes)
        {
            Type target = typeof(T);
            return target.GetConstructor(bindFlags, null, argTypes, null);
        }

        public static MethodInfo GetMethodFrom<T>(string name, BindingFlags bindFlags = BindingFlags.Default)
        {
            Type target = typeof(T);
            return target.GetMethod(name, bindFlags);
        }

        public static FieldInfo GetField<T>(string name, BindingFlags bindFlags = BindingFlags.Default)
        {
            Type target = typeof(T);
            return target.GetField(name, bindFlags);
        }

        public static PropertyInfo GetProperty<T>(string name, BindingFlags bindFlags = BindingFlags.Default)
        {
            Type target = typeof(T);
            return target.GetProperty(name, bindFlags);
        }
    }
}
