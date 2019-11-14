using System.Reflection;
using LiteNetLib.Utils;

namespace NetSyncLib.Helper
{
    public abstract class NetPropertyPacket
    {
        protected NetPropertyPacket(ulong tag, MemberInfo propertyInfo, NetSynchronizeAttribute attribute)
        {
            this.Tag = tag;
            this.PropertyInfo = propertyInfo;
            this.Attribute = attribute;
            this.LastUpdate = 0;
        }

        public ulong Tag { get; internal set; }

        public MemberInfo PropertyInfo { get; }

        public NetSynchronizeAttribute Attribute { get; }

        public virtual object Value { get; protected set; }

        public long LastUpdate { get; protected set; }

        public bool IsAllowedToUpdate(long time)
        {
            return time - this.LastUpdate > this.Attribute.UpdateFrequency;
        }

        public bool HasValueChanged(object observedObject)
        {
            return this.HasValueChanged(observedObject, out object obj);
        }

        public bool HasValueChanged(object observedObject, out object newValue)
        {
            newValue = this.PropertyInfo.GetValue(observedObject);
            return !newValue.Equals(this.Value);
        }

        /// <summary>
        /// False if there was nothing to send and nothing was added to the reader.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="observedObject"></param>
        /// <param name="writer"></param>
        /// <param name="resendAll">If true data will be written even if nothing changed since last time and even if it should not be allowed to update.</param>
        /// <returns>False if there was nothing to send and nothing was added to the reader.</returns>
        public abstract bool TrySendUpdate(long time, object observedObject, NetDataWriter writer, bool resendAll = false);

        public abstract void ReceiveUpdate(object observedObject, NetDataReader reader);
    }
}