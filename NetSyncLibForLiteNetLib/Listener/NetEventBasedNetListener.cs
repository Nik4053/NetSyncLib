using LiteNetLib;
using NetSyncLib;
using System;
using System.Collections.Generic;

namespace NetSyncLibForLiteNetLib.Listener
{
    public abstract class NetEventBasedNetListener : EventBasedNetListener
    {
        public readonly Dictionary<byte, NetReceiveType> NetworkReceiveTypes = new Dictionary<byte, NetReceiveType>();

        protected NetEventBasedNetListener()
        {
            AddDefaultEvents();
        }

        public virtual void AddDefaultEvents()
        {
            NetworkReceiveEvent -= this.OnNetworkReceiveEvent;
            NetworkReceiveEvent += this.OnNetworkReceiveEvent;
        }

        private void OnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, NetSynchronizeDeliveryMethod deliveryMethod)
        {
            byte packetType = reader.GetByte();
            if (NetworkReceiveTypes.TryGetValue(packetType, out NetReceiveType receiveType))
            {
#if !DEBUG
                try
                {
#endif
                receiveType(reader, peer, deliveryMethod);
#if !DEBUG
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }

#endif
            }
            else
            {
                Console.Error.WriteLine(NetOrganisator.NetState + ": Received unknown packetType: " + packetType);
            }
        }
    }
}