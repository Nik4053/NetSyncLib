using LiteNetLib.Utils;
using System.Reflection;

namespace NetSyncLib.Helper
{
    public delegate TValueType ValueReceiver<TValueType>(DataReader reader);

    public delegate void ValueSender<TValueType>(TValueType obj, DataWriter writer);

    /// <summary>
    /// Packet containing all necessary information about a property/field.
    /// </summary>
    public class NetPropertyPacket<TValueType> : NetPropertyPacket
    {
        private readonly NetPacketHelperTypes.NetValueHandler<TValueType> netValueHandler;

        public NetPropertyPacket( ulong tag, MemberInfo propertyInfo, NetSynchronizeAttribute attribute, NetPacketHelperTypes.NetValueHandler<TValueType> netValueHandler, TValueType initValue = default(TValueType)) : base(tag, propertyInfo, attribute)
        {
            this.netValueHandler = netValueHandler;
            this.TypedValue = initValue;
        }

        public TValueType TypedValue { get; private set; }

        public override object Value
        {
            get => this.TypedValue;
            protected set { this.TypedValue = (TValueType)value; }
        }

        public bool HasValueChangedTyped(object observedObject, out TValueType newValue)
        {
            newValue = (TValueType)this.PropertyInfo.GetValue(observedObject);
            return !newValue.Equals(this.TypedValue);
        }

        public override bool TrySendUpdate(long time, object observedObject, DataWriter writer, bool resendAll = false)
        {
            if (!resendAll)
            {
                if (!this.IsAllowedToUpdate(time)) return false;
                if (!this.HasValueChangedTyped(observedObject, out TValueType newValue)) return false;
                this.TypedValue = newValue;
            }

            this.netValueHandler.Sender(this.TypedValue, writer);
            return true;
        }

        public override void ReceiveUpdate(object observedObject, DataReader reader)
        {
            this.PropertyInfo.SetValue(observedObject, this.netValueHandler.Receiver(reader));
        }
    }
}