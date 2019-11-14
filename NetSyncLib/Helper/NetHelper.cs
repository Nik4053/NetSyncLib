using System;

namespace NetSyncLib.Helper
{
    public static class NetHelper
    {
        /// <summary>
        /// Creates the object of the class with the given name via reflection.
        /// </summary>
        /// <param name="assemblyQualifiedName">See <see cref="Type.AssemblyQualifiedName"/>.</param>
        /// <returns></returns>
        public static object CreateObject(string assemblyQualifiedName)
        {
            return CreateObject(Type.GetType(assemblyQualifiedName));
        }

        public static object CreateObject(Type type)
        {
            return Activator.CreateInstance(type, true); // System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(fullName);
        }

        public static TType CreateObject<TType>()
        {
            return Activator.CreateInstance<TType>(); // System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(fullName);
        }
    }
}