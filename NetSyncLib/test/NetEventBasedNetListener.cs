using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using System;
using System.Collections.Generic;

namespace NetSyncLibForLiteNetLib.Listener
{
    public abstract class NetEventBasedNetListener
    {
        public readonly Dictionary<byte, NetReceiveType> NetworkReceiveTypes = new Dictionary<byte, NetReceiveType>();

        protected NetEventBasedNetListener()
        {
            
        }

        internal void OnNetworkReceiveEvent(IPeer peer, NetDataReader reader, NetSyncDeliveryMethod deliveryMethod)
        {
            byte packetType = reader.GetByte();
            if (NetworkReceiveTypes.TryGetValue(packetType, out NetReceiveType receiveType))
            {
#if !DEBUG
                try
                {
#endif
                receiveType((NetDataReader)reader, peer, deliveryMethod);
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