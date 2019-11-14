using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Server;
using NetSyncLibForLiteNetLib.Client;
using NetSyncLibForLiteNetLib.Listener;
using NetSyncLibForLiteNetLib.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetSyncLibForLiteNetLib
{
    public static class NetOrganisator
    {
        public static NetEventBasedNetListener EventListener { get; private set; }

        public static NetManager Manager { get; private set; }

        /// <summary>
        /// Gets the netId of this application.
        /// -2 is offline, -1 is server, 0 -> is client.
        /// </summary>
        public static int NetPeerId { get; internal set; } = -2;

        public static ENetState NetState { get; private set; } = ENetState.Offline;

        internal static ClientNetObjectHandler<INetObject> ClientNetObjectHandler { get; private set; }

        internal static NetControllerServerHandler ControllerServerHandler { get; private set; }

        internal static ServerWeakNetObjectHandler<INetObject> ServerNetObjectHandler { get; private set; }

        public static void UpdateNet()
        {
            if (NetState != ENetState.Offline)
                Manager.PollEvents();
        }

        public static bool StartClient(string address = "localhost", int port = 9050, string password = "", string username = "UnknownPlayer", bool autoUpdate = true)
        {
            if (NetState != ENetState.Offline) throw new InvalidOperationException("Cannot start Client while there is an active connection of Type: " + NetState);
            ClientListener listener = new ClientListener();
            NetManager client = new NetManager(listener);
            client.Start();
            NetDataWriter conWriter = new NetDataWriter();
            conWriter.Put(username.Trim());
            conWriter.Put(password);
            NetPeer connection = client.Connect(address, port, conWriter);
            while (connection.ConnectionState == ConnectionState.Outcoming)
            {
                client.PollEvents();

                // Console.WriteLine("Client: listening...");
                Thread.Sleep(15);
            }

            if (connection.ConnectionState != ConnectionState.Connected)
            {
                Console.WriteLine("Cannot find server. Returning...");
                ResetNet();
                return false;
            }

            EventListener = listener;
            Manager = client;
            NetState = ENetState.Client;
            ClientNetObjectHandler = new ClientNetObjectHandler<INetObject>();
            if (autoUpdate)
            {
                new Thread(() =>
                {
                    while (!Console.KeyAvailable)
                    {
                        //client.PollEvents();
                        UpdateNet();
                        Thread.Sleep(5);
                    }

                    Console.WriteLine("Disconnected from Server");
                    client.Stop();
                    ResetNet(); // TODO reset net not called when auto-update = false
                }).Start();
            }

            return true;
        }

        public static void StartServer(string addressIPv4 = null, string addressIPv6 = null, int port = 9050, string password = "", int maxConnections = 10, bool autoUpdate = true)
        {
            if (NetState != ENetState.Offline) throw new InvalidOperationException("Cannot start Server while there is an active connection of Type: " + NetState);
            ServerListener listener = new ServerListener(password, maxConnections);
            NetManager server = new NetManager(listener);
            IPAddress ipv4;
            if (addressIPv4 != null)
            {
                ipv4 = NetUtils.ResolveAddress(addressIPv4);
            }
            else
            {
                ipv4 = IPAddress.Any;
            }

            IPAddress ipv6;
            if (addressIPv6 != null)
            {
                ipv6 = NetUtils.ResolveAddress(addressIPv6);
            }
            else
            {
                ipv6 = IPAddress.IPv6Any;
            }

            if (server.Start(ipv4, ipv6, port))
            {
                EventListener = listener;
                Manager = server;
                NetState = ENetState.Server;
                ServerNetObjectHandler = new ServerWeakNetObjectHandler<INetObject>();
                ControllerServerHandler = new NetControllerServerHandler();
                NetPeerId = -1;
            }
            server.SimulateLatency = true;
            server.SimulationMaxLatency = 1000;

            server.SimulationMinLatency = 1000;
            // serverThread
            new Thread(() =>
            {
                ulong bytesSend = 0;
                ulong packetsSend = 0;
                while (!Console.KeyAvailable)
                {
                    ulong newBytesSend = Manager.Statistics.BytesSent;
                    ulong newPacketsSend = Manager.Statistics.PacketsSent;
                    Console.WriteLine($"Send: {newBytesSend - bytesSend} Bytes/s, {newPacketsSend - packetsSend} Packages/s, {Manager.Statistics.PacketLossPercent}% PacketLoss");
                    bytesSend = newBytesSend;
                    packetsSend = newPacketsSend;
                    Thread.Sleep(1000);
                }
            }).Start();
            if (autoUpdate)
            {
                new Thread(() =>
                {
                    while (!Console.KeyAvailable)
                    {
                        server.PollEvents();
                        Thread.Sleep(5);
                    }

                    Console.WriteLine("Stopping Server...");
                    server.Stop();
                    ResetNet();
                }).Start();
            }
        }

        /// <summary>
        /// Wrapper for <see cref="NetState"/> == <see cref="ENetState.Server"/>.
        /// </summary>
        /// <returns>The result of <see cref="NetState"/> == <see cref="ENetState.Server"/>.</returns>
        public static bool IsServer()
        {
            return NetState == ENetState.Server;
        }

        public static bool IsClient()
        {
            return NetState == ENetState.Client;
        }

        private static void ResetNet()
        {
            NetState = ENetState.Offline;
            Manager = null;
            NetPeerId = -2;
            EventListener = null;
            ClientNetObjectHandler = null;
            ServerNetObjectHandler = null;
        }

        /// <summary>
        /// Will send data to the given peers. Leaving <see cref="sendTo"/> == null will be the same as calling <see cref="SendToAll"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="options"></param>
        /// <param name="sendTo">The peers this message should be send to. If null it will be sent to everybody. If no content given data will not be send.</param>
        public static void Send(NetDataWriter writer, NetSynchronizeDeliveryMethod options, IEnumerable<NetPeer> sendTo = null)
        {
            if (sendTo == null)
            {
                SendToAll(writer, options);
                return;
            }

            foreach (NetPeer peer in sendTo)
            {
                peer.Send(writer, options);
            }
        }

        public static void SendToAll(NetDataWriter writer, NetSynchronizeDeliveryMethod options)
        {
            switch (NetState)
            {
                case ENetState.Server:
                    SendToAllClients(writer, options);
                    break;

                case ENetState.Client:
                    SendToServer(writer, options);
                    break;

                default: return;
            }
        }

        internal static void SendToAllClients(NetDataWriter writer, NetSynchronizeDeliveryMethod options)
        {
            Manager.SendToAll(writer, options);
        }

        internal static void SendToServer(NetDataWriter writer, NetSynchronizeDeliveryMethod options)
        {
            Manager.FirstPeer.Send(writer, options);
        }

        public static void UpdateAllNetObjects()
        {
            if (NetState != ENetState.Server) return;

            // TODO UpdateAllNetObjects
            List<INetObject> netObjectList = ServerNetObjectHandler.GetAllKeys();
            foreach (INetObject obj in netObjectList)
            {
                obj.NetServerSendUpdate();
            }
        }

        public static void ResendAllNetObjects(List<NetPeer> peers = null)
        {
            if (NetState != ENetState.Server) return;

            // TODO UpdateAllNetObjects
            List<INetObject> netObjectList = ServerNetObjectHandler.GetAllKeys();
            foreach (INetObject obj in netObjectList)
            {
                ClientNetPacketTypes.SendDestroyINetObject(obj);
                ClientNetPacketTypes.SendCreateINetObject(obj, peers);
            }
        }
    }
}