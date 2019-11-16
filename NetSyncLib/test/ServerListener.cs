using LiteNetLib;
using NetSyncLibForLiteNetLib.Client;
using NetSyncLibForLiteNetLib.Listener;
using System;
using System.Collections.Generic;
using NetSyncLib.Server;
using NetSyncLib.NetLibInterfaces;
using NetSyncLib.Client;
using NetSyncLib;

namespace NetSyncLibForLiteNetLib.Server
{
    public class ServerListener : NetEventBasedNetListener
    {
        private readonly string password;
        private readonly int maxConnections;

        public ServerListener(string password, int maxConnections)
        {
            this.password = password;
            this.maxConnections = maxConnections;
            AddReceiveTypes();
        }

        private void AddReceiveTypes()
        {
            NetworkReceiveTypes.Add(ServerNetPacketTypes.TypeUpdateController, ServerNetPacketTypes.ReadUpdateController);
            NetworkReceiveTypes.Add(ServerNetPacketTypes.TypeTextMessage, ServerNetPacketTypes.ReadMessage);
            NetworkReceiveTypes.Add(ServerNetPacketTypes.TypeCloneAll, ServerNetPacketTypes.ReadCloneAll);
        }

        private void OnPeerConnectedEvent(IPeer peer)
        {
            //Console.WriteLine("Server: We got connection: {0}, Id will be: {1}", peer.EndPoint, peer.Id);
            ClientNetPacketTypes.SendSetPeerId(peer.Id, peer);
            NetOrganisator.ResendAllNetObjects(new List<IPeer> { peer });
        }
    }
}