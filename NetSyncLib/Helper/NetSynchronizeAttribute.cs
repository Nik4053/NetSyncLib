using System;

namespace NetSyncLib.Helper
{
    public enum NetSyncDeliveryMethod
    {
        Unreliable = 0,
        ReliableUnordered = 1,
        ReliableOrdered = 3,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class NetSynchronizeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSynchronizeAttribute"/> class.
        /// If the property is an <see cref="INetObject"/> only the reference (netid) of this object will be synced.
        /// If a property having this Attribute gets overridden, the new versions attribute will be used. If the new version doesn't have one it will not be synced.
        /// </summary>
        /// <param name="updateFrequency">Update frequency in milliseconds.</param>
        public NetSynchronizeAttribute(int updateFrequency = 0, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            this.UpdateFrequency = updateFrequency;
            this.NetSynchronizeDeliveryMethod = deliveryMethod;
        }

        /// <summary>
        /// Gets or sets update frequency in milliseconds.
        /// </summary>
        public int UpdateFrequency { get; set; }

        public NetSyncDeliveryMethod NetSynchronizeDeliveryMethod { get; set; }
    }
}