using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.Helper;
using NetSyncLib.Server;

namespace NetSyncLib
{
    public static class INetControllerExt
    {
        public static void TrySendNetControllerUpdate(this INetController obj, NetDataWriter writer, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.ReliableOrdered)
        {
            if (!NetOrganisator.IsClient()) return;
            if (obj.IsOwner())
                ServerNetPacketTypes.SendUpdateController(obj, writer, deliveryMethod);
        }

        /// <summary>
        /// Returns true if this peer has the rights to change this controller.
        /// </summary>
        /// <returns>true if this peer has the rights to change this controller.</returns>
        public static bool IsOwner(this INetController obj)
        {
            return obj.IsOwner(NetOrganisator.NetPeerId);
        }
    }
}