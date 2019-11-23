using LiteNetLib.Utils;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;

namespace NetSyncLib
{
    public delegate void NetReceiveType(DataReader reader, IPeer peer, NetSyncDeliveryMethod deliveryMethod);
}