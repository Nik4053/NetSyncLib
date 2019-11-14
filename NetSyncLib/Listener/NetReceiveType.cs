
using NetSyncLib.Helper;

namespace NetSyncLib
{
    public delegate void NetReceiveType(NetPacketReader reader, NetPeer peer, NetSyncDeliveryMethod deliveryMethod);
}