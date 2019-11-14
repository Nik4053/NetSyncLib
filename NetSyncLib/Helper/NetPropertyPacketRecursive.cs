using LiteNetLib.Utils;
using System;
using System.Reflection;

namespace NetSyncLib.Helper
{
    public class NetPropertyPacketRecursive : NetPropertyPacket
    {
        private NetPacketHelper netPacketHelper;

        public NetPropertyPacketRecursive(object value, ulong tag, MemberInfo propertyInfo, NetSynchronizeAttribute attribute) : base(tag, propertyInfo, attribute)
        {
            this.Value = value;
            if (this.Value == null)
            {
                throw new NotSupportedException("Recursive calling objects that are null isn't not supported");
            }

            // TODO check if property is readonly or does not have a setter
            this.netPacketHelper = new NetPacketHelper(this.Value); // propertyInfo.GetUnderlyingType());
        }

        public override void ReceiveUpdate(object observedObject, NetDataReader reader)
        {
            this.netPacketHelper.ReceiveUpdate(reader);
        }

        public override bool TrySendUpdate(long time, object observedObject, NetDataWriter writer, bool resendAll = false)
        {
            throw new NotSupportedException();
        }

        public NetPacketHelper.NetPacketHelperUpdate[] TrySendUpdate(long time, object observedObject, bool resendAll = false, bool differSendTypes = true)
        {
            if (!resendAll)
            {
                if (!this.IsAllowedToUpdate(time)) return null;
                if (this.HasValueChanged(observedObject, out object newVal))
                {
                    if (newVal.GetType().Equals(this.Value.GetType()))
                    {
                        Console.Error.WriteLine("Warning the Value of a netSynchronized recursive object reference has changed!! Things may break. " + $"Property: {this.PropertyInfo} OldVal: {this.Value}, NewVal: {newVal}, InObject: {observedObject}");
                        this.netPacketHelper = new NetPacketHelper(newVal);
                        this.Value = newVal;
                    }
                    else
                    {
                        throw new NotSupportedException("The Type of a netSynchronized recursive object reference has changed to object of different type");
                    }
                }
            }

            return this.netPacketHelper.SendUpdate(resendAll, differSendTypes);
        }
    }
}