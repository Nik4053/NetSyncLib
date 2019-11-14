using LiteNetLib;
using NetSyncLibForLiteNetLib.Client;
using NetSyncLibForLiteNetLib.Listener;
using System;
using System.Collections.Generic;

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

        public override void AddDefaultEvents()
        {
            base.AddDefaultEvents();
            PeerConnectedEvent -= OnPeerConnectedEvent;
            PeerConnectedEvent += OnPeerConnectedEvent;
            ConnectionRequestEvent -= OnConnectionRequestEvent;
            ConnectionRequestEvent += OnConnectionRequestEvent;
        }

        private void AddReceiveTypes()
        {
            NetworkReceiveTypes.Add(ServerNetPacketTypes.TypeUpdateController, ServerNetPacketTypes.ReadUpdateController);
            NetworkReceiveTypes.Add(ServerNetPacketTypes.TypeTextMessage, ServerNetPacketTypes.ReadMessage);
            NetworkReceiveTypes.Add(ServerNetPacketTypes.TypeCloneAll, ServerNetPacketTypes.ReadCloneAll);
        }

        private void OnPeerConnectedEvent(NetPeer peer)
        {
            Console.WriteLine("Server: We got connection: {0}, Id will be: {1}", peer.EndPoint, peer.Id);
            ClientNetPacketTypes.SendSetPeerId(peer.Id, peer);
            NetOrganisator.ResendAllNetObjects(new List<NetPeer> { peer });
        }

        private void OnConnectionRequestEvent(ConnectionRequest request)
        {
            if (NetOrganisator.Manager.PeersCount < maxConnections /* max connections */)
            {
                string name = request.Data.GetString();
                NetPeer newPeer = request.AcceptIfKey(password);
                newPeer.Name = name;
            }
            else
            {
                request.Reject();
            }
        }
    }
}