﻿using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Client;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using NetSyncLib.Server;
using NetSyncLibForLiteNetLib.Client;
using NetSyncLibForLiteNetLib.Listener;
using NetSyncLibForLiteNetLib.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetSyncLib
{
    public static class NetOrganisator
    {
        /// <summary>
        /// Gets the netId of this application.
        /// -2 is offline, -1 is server, 0 -> is client.
        /// </summary>
        public static int NetPeerId { get; internal set; } = -2;


        public static ENetState NetState { get; private set; } = ENetState.Offline;
        internal static ClientNetObjectHandler<INetObject> ClientNetObjectHandler { get; private set; }

        internal static NetControllerServerHandler ControllerServerHandler { get; private set; }

        internal static ServerWeakNetObjectHandler<INetObject> ServerNetObjectHandler { get; private set; }
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
        public static void ResendAllNetObjects(List<IPeer> peers = null)
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
       

        public static void StartAsServer() {
            if (NetState != ENetState.Offline) throw new InvalidOperationException("Cannot start Server while there is an active connection of Type: " + NetState);
            NetState = ENetState.Server;
            ServerNetObjectHandler = new ServerWeakNetObjectHandler<INetObject>();
            ControllerServerHandler = new NetControllerServerHandler();
            NetPeerId = -1;
        }

        public static void ResetNet()
        {
            NetState = ENetState.Offline;
            NetPeerId = -2;
            ClientNetObjectHandler = null;
            ServerNetObjectHandler = null;
        }
        public static void StartAsClient() {
            if (NetState != ENetState.Offline) throw new InvalidOperationException("Cannot start Server while there is an active connection of Type: " + NetState);
            NetState = ENetState.Client;
            ClientNetObjectHandler = new ClientNetObjectHandler<INetObject>();
           
        }



        internal static void SendToServer(NetDataWriter writer, NetSyncDeliveryMethod options)
        {
            TESTDATA = writer.CopyData();
            Console.WriteLine($"OPTION: {options} \n DATA: " + writer.Data);
            //Manager.FirstPeer.Send(writer, (LiteNetLib.DeliveryMethod)options);
        }

        internal static Byte[] TESTDATA;
        /// <summary>
        /// Will send data to the given peers. Leaving <see cref="sendTo"/> == null will be the same as calling <see cref="SendToAll"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="options"></param>
        /// <param name="sendTo">The peers this message should be send to. If null it will be sent to everybody. If no content given data will not be send.</param>
        public static void Send(NetDataWriter writer, NetSyncDeliveryMethod options, IEnumerable<IPeer> sendTo = null)
        {
            TESTDATA = writer.CopyData();
            return;
            if (sendTo == null)
            {
                SendToAll(writer, options);
                return;
            }

            foreach (IPeer peer in sendTo)
            {
                peer.Send(writer, options);
            }
        }

        public static void SendToAll(NetDataWriter writer, NetSyncDeliveryMethod options)
        {
            TESTDATA = writer.CopyData();
            return;
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

        internal static void SendToAllClients(NetDataWriter writer, NetSyncDeliveryMethod options)
        {
            TESTDATA = writer.CopyData();
            //Manager.SendToAll(writer, options);
        }
    }
}