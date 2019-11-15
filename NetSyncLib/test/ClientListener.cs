using NetSyncLib.Client;
using NetSyncLibForLiteNetLib.Listener;

namespace NetSyncLibForLiteNetLib.Client
{
    public class ClientListener : NetEventBasedNetListener
    {
        public ClientListener()
        {
            AddReceiveTypes();
        }

        private void AddReceiveTypes()
        {
            NetworkReceiveTypes.Add(ClientNetPacketTypes.TypeSetPeerId, ClientNetPacketTypes.ReadSetPeerId);
            NetworkReceiveTypes.Add(ClientNetPacketTypes.TypeCreateINetObject, ClientNetPacketTypes.ReadCreateINetObject);
            NetworkReceiveTypes.Add(ClientNetPacketTypes.TypeUpdateINetObject, ClientNetPacketTypes.ReadUpdateINetObject);
            NetworkReceiveTypes.Add(ClientNetPacketTypes.TypeDestroyINetObject, ClientNetPacketTypes.ReadDestroyINetObject);
            NetworkReceiveTypes.Add(ClientNetPacketTypes.TypeTextMessage, ClientNetPacketTypes.ReadMessage);
        }
    }
}