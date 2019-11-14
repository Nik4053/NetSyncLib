using LiteNetLib.Utils;
using NetSyncLib.Helper;

namespace NetSyncLib
{
    public delegate void NetReceiveType(NetDataReader reader, NetPeer peer, NetSyncDeliveryMethod deliveryMethod);
}