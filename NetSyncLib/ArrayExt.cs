using System;
using System.Linq;
using System.Reflection;

namespace NetSyncLib
{
    /// <summary>
    /// TODO: Move to another lib.
    /// </summary>
    public static class ArrayExt
    {
        /// <summary>
        /// Sets all fields of the given array to the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="value"></param>
        /// <returns>The same array as the input filled with the given value.</returns>
        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }

            return arr;
        }
    }

    public static class HelperMethods
    {
        public static System.Collections.Generic.IEnumerable<System.Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : System.Attribute
        {
            return from a in System.AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }

        public static System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> GetMethodsWith<TAttribute>(bool inherit, System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Default)
        {
            return from a in System.AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   from m in t.GetMethods(bindingAttr)
                   where m.IsDefined(typeof(TAttribute), inherit)
                   select m;
        }
    }
}