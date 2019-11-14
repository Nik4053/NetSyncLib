using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetSyncLib.Helper
{
    public sealed class NetPacketHelper
    {
        private enum ETagBitmaskType
        {
            Byte = 0,
            Ushort = 1,
            Uint = 2,
            Ulong = 3,
        }

        /// <summary>
        /// The type that is used for identifying changes. 1 bit for each synchronized attribute.
        /// </summary>
        private readonly ETagBitmaskType tagBitmaskType;

        public NetPacketHelper(object observedObject)
        {
            this.ObservedObject = observedObject;
            if (this.ObservedObject == null) throw new ArgumentNullException("Object cannot be null");
            int length = this.CreateAllPropertyPackets(observedObject, typeof(NetSynchronizeAttribute), true);
            if (length < 8)
            {
                this.tagBitmaskType = ETagBitmaskType.Byte;
            }
            else if (length < 16)
            {
                this.tagBitmaskType = ETagBitmaskType.Ushort;
            }
            else if (length < 32)
            {
                this.tagBitmaskType = ETagBitmaskType.Uint;
            }
            else if (length < 64)
            {
                this.tagBitmaskType = ETagBitmaskType.Ulong;
            }
            else
            {
                throw new NotSupportedException("Object with more than 64 synchronized values are not supported");
            }
        }

        public object ObservedObject { get; }

        private int CreateAllPropertyPackets(object observedObject, Type attributeType, bool onlyOne)
        {
            Type type = observedObject.GetType();
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            HashSet<MemberInfo> memberHashSet = new HashSet<MemberInfo>(); // used for identifying already added members
            HashSet<string> memberNames = new HashSet<string>(); // used for identifying already added members
            HashSet<NetPropertyPacket> pP = new HashSet<NetPropertyPacket>();
            HashSet<NetPropertyPacketRecursive> pPR = new HashSet<NetPropertyPacketRecursive>();
            while (type != null)
            {
                MemberInfo[] members = type.GetFields(bindingFlags).Cast<MemberInfo>()
                    .Concat(type.GetProperties(bindingFlags)).ToArray();
                type = type.BaseType;
                foreach (MemberInfo memberInfo in members)
                {
                    object[] customAttributes = memberInfo.GetCustomAttributes(attributeType, true);

                    if (onlyOne && customAttributes.Length > 1)
                    {
                        throw new InvalidOperationException("Property with two synchronized attributes is forbidden");
                    }

                    if (!memberHashSet.Add(memberInfo) || !memberNames.Add(memberInfo.Name))
                    {
                        continue;
                    }

                    if (customAttributes.Length == 0)
                    {
                        continue;
                    }

                    object val = memberInfo.GetValue(observedObject);
                    NetPropertyPacket netPropertyPacket = this.CreateNetPropertyPacket(1UL << pP.Count, memberInfo, (NetSynchronizeAttribute)customAttributes[0]);
                    if (netPropertyPacket is NetPropertyPacketRecursive netPropertyPacketRecursive)
                    {
                        pPR.Add(netPropertyPacketRecursive);
                    }
                    else
                    {
                        pP.Add(netPropertyPacket);
                    }
                }
            }

            this.propertyPackets = pP.ToArray();
            this.propertyPacketsRecursives = pPR.ToArray();
            for (int i = 0; i < this.propertyPacketsRecursives.Length; i++)
            {
                this.propertyPacketsRecursives[i].Tag = 1UL << (pP.Count + i);
            }

            return this.propertyPackets.Length + this.propertyPacketsRecursives.Length;
        }

        private NetPropertyPacket[] propertyPackets;
        private NetPropertyPacketRecursive[] propertyPacketsRecursives;

        private NetPropertyPacket CreateNetPropertyPacket(ulong tag, MemberInfo propertyInfo, NetSynchronizeAttribute attribute)
        {
            if (typeof(INetObject).IsAssignableFrom(propertyInfo.GetUnderlyingType()))
            {
                NetPacketHelperTypes.Handlers.TryGetValue(typeof(INetObject), out NetPacketHelperTypes.NetValueHandler netObjHandler);
                return netObjHandler.CreateNetPropertyPacket(tag, propertyInfo, attribute);
            }
            else if (NetPacketHelperTypes.Handlers.TryGetValue(propertyInfo.GetUnderlyingType(), out NetPacketHelperTypes.NetValueHandler valueHandler))
            {
                return valueHandler.CreateNetPropertyPacket(tag, propertyInfo, attribute);
            }
            else
            {
                return new NetPropertyPacketRecursive(propertyInfo.GetValue(this.ObservedObject), tag, propertyInfo, attribute);
            }
        }

        public struct NetPacketHelperUpdate
        {
            public readonly bool Changes;
            public readonly NetSyncDeliveryMethod NetSynchronizeDeliveryMethod;
            public readonly NetDataWriter Writer;

            internal NetPacketHelperUpdate(bool changes, NetSyncDeliveryMethod deliveryMethod, NetDataWriter writer)
            {
                this.Changes = changes;
                this.NetSynchronizeDeliveryMethod = deliveryMethod;
                this.Writer = writer;
            }
        }

        public void ResendAll(NetDataWriter writer)
        {
            NetDataWriter dataWriter = this.SendUpdate(true, false)[0].Writer;
            writer.Put(dataWriter.Data, 0, dataWriter.Length);
        }

        /// <summary>
        /// Will contain packets of changes. 0: reliableOrdered, 1: reliableUnordered, 2: unrealiable. If there where no changes the writer will contain a single 0.
        /// </summary>
        /// <param name="resendAll">If true will resend all. No checks for value changes or allowed update intervall.</param>
        /// <param name="differSendTypes">If false all changes will be in return[0]. There will be no seperation for SendTypes.</param>
        /// <returns>Will contain packets of changes. 0: reliableOrdered, 1: reliableUnordered, 2: unrealiable. If there where no changes the writer will contain a single 0.</returns>
        public NetPacketHelperUpdate[] SendUpdate(bool resendAll = false, bool differSendTypes = true)
        {
            long time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            NetDataWriter reliableWriter = new NetDataWriter();
            NetDataWriter reliableOrderedWriter = new NetDataWriter();
            NetDataWriter unreliableWriter = new NetDataWriter();
            if (!differSendTypes)
            {
                reliableOrderedWriter = reliableWriter;
                unreliableWriter = reliableWriter;
            }

            ulong reliableChanges = 0;
            ulong reliableOrderedChanges = 0;
            ulong unreliableChanges = 0;
            foreach (NetPropertyPacket pp in this.propertyPackets)
            {
                NetDataWriter writer;
                switch (pp.Attribute.NetSynchronizeDeliveryMethod)
                {
                    case NetSyncDeliveryMethod.ReliableOrdered:
                        writer = reliableOrderedWriter;
                        if (pp.TrySendUpdate(time, this.ObservedObject, writer, resendAll))
                        {
                            reliableOrderedChanges += pp.Tag;
                        }

                        break;

                    case NetSyncDeliveryMethod.ReliableUnordered:
                        writer = reliableWriter;
                        if (pp.TrySendUpdate(time, this.ObservedObject, writer, resendAll))
                        {
                            reliableChanges += pp.Tag;
                        }

                        break;

                    case NetSyncDeliveryMethod.Unreliable:
                        writer = unreliableWriter;
                        if (pp.TrySendUpdate(time, this.ObservedObject, writer, resendAll))
                        {
                            unreliableChanges += pp.Tag;
                        }

                        break;

                    default: throw new NotSupportedException();
                }
            }

            foreach (NetPropertyPacketRecursive ppR in this.propertyPacketsRecursives)
            {
                NetPacketHelperUpdate[] updatesPPR = ppR.TrySendUpdate(time, this.ObservedObject, resendAll, differSendTypes);
                if (updatesPPR == null) continue;

                if (updatesPPR[0].Changes)
                {
                    reliableOrderedChanges += ppR.Tag;
                    reliableOrderedWriter.Put(updatesPPR[0].Writer.Data, 0, updatesPPR[0].Writer.Length);
                }

                if (!differSendTypes) continue;

                if (updatesPPR[1].Changes)
                {
                    reliableChanges += ppR.Tag;
                    reliableWriter.Put(updatesPPR[1].Writer.Data, 0, updatesPPR[1].Writer.Length);
                }

                if (updatesPPR[2].Changes)
                {
                    unreliableChanges += ppR.Tag;
                    unreliableWriter.Put(updatesPPR[2].Writer.Data, 0, updatesPPR[2].Writer.Length);
                }
            }

            if (!differSendTypes)
            {
                ulong changes = reliableChanges + reliableOrderedChanges + unreliableChanges;
                NetDataWriter differSendTypesWriter = new NetDataWriter();
                this.WriteChangesValue(changes, differSendTypesWriter);
                differSendTypesWriter.Put(reliableOrderedWriter.Data, 0, reliableOrderedWriter.Length);
                NetPacketHelperUpdate[] differUpdates = new NetPacketHelperUpdate[1];
                differUpdates[0] = new NetPacketHelperUpdate(reliableOrderedChanges != 0, NetSyncDeliveryMethod.ReliableOrdered, differSendTypesWriter);
                return differUpdates;
            }

            NetPacketHelperUpdate[] updates = new NetPacketHelperUpdate[3];
            NetDataWriter dataWriter = new NetDataWriter();
            this.WriteChangesValue(reliableOrderedChanges, dataWriter);
            dataWriter.Put(reliableOrderedWriter.Data, 0, reliableOrderedWriter.Length);
            updates[0] = new NetPacketHelperUpdate(reliableOrderedChanges != 0, NetSyncDeliveryMethod.ReliableOrdered, dataWriter);
            dataWriter = new NetDataWriter();
            this.WriteChangesValue(reliableChanges, dataWriter);
            dataWriter.Put(reliableWriter.Data, 0, reliableWriter.Length);
            updates[1] = new NetPacketHelperUpdate(reliableChanges != 0, NetSyncDeliveryMethod.ReliableUnordered, dataWriter);
            dataWriter = new NetDataWriter();
            this.WriteChangesValue(unreliableChanges, dataWriter);
            dataWriter.Put(unreliableWriter.Data, 0, unreliableWriter.Length);
            updates[2] = new NetPacketHelperUpdate(unreliableChanges != 0, NetSyncDeliveryMethod.Unreliable, dataWriter);
            return updates;
        }

        public void ReceiveUpdate(NetDataReader reader)
        {
            ulong changes = this.ReadAndChangesValue(reader);
            if (changes == 0)
            {
                try
                {
                    Console.Error.WriteLine($"Warning: Received update to NetPacketHelper of type {this.ObservedObject} that was 0");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }

                return;
            }

            foreach (NetPropertyPacket packet in this.propertyPackets)
            {
                if (!this.IsFlagSet(packet.Tag, changes))
                {
                    continue;
                }

                packet.ReceiveUpdate(this.ObservedObject, reader);
            }

            foreach (NetPropertyPacket packetR in this.propertyPacketsRecursives)
            {
                if (!this.IsFlagSet(packetR.Tag, changes))
                {
                    continue;
                }

                packetR.ReceiveUpdate(this.ObservedObject, reader);
            }
        }

        /// <summary>
        /// checks if the flag is set.
        /// </summary>
        /// <param name="flag">already shifted.</param>
        /// <param name="value">the value to search for the flag.</param>
        /// <returns></returns>
        private bool IsFlagSet(ulong flag, ulong value)
        {
            return (value & flag) != 0;
        }

        private void WriteChangesValue(ulong changes, NetDataWriter writer)
        {
            switch (this.tagBitmaskType)
            {
                case ETagBitmaskType.Byte:
                    writer.Put((byte)changes);
                    break;

                case ETagBitmaskType.Ushort:
                    writer.Put((ushort)changes);
                    break;

                case ETagBitmaskType.Uint:
                    writer.Put((int)changes);
                    break;

                case ETagBitmaskType.Ulong: writer.Put(changes); break;
                default: throw new NotSupportedException("Unknown enumtype not supported: " + this.tagBitmaskType);
            }
        }

        private ulong ReadAndChangesValue(NetDataReader reader)
        {
            switch (this.tagBitmaskType)
            {
                case ETagBitmaskType.Byte: return reader.GetByte();
                case ETagBitmaskType.Ushort: return reader.GetUShort();
                case ETagBitmaskType.Uint: return reader.GetUInt();
                case ETagBitmaskType.Ulong: return reader.GetULong();
                default: throw new NotSupportedException("Unknown enumtype not supported: " + this.tagBitmaskType);
            }
        }
    }
}