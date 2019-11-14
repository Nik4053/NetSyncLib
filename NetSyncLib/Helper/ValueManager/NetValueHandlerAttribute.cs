using System;

namespace NetSyncLib.Helper.ValueManager
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class NetValueHandlerAttribute : Attribute
    {
        public NetValueHandlerAttribute()
        {
        }
    }
}
