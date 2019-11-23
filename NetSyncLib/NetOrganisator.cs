using LiteNetLib.Utils;
using NetSyncLib.Client;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using NetSyncLib.Server;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NetSyncLib
{
    public static class NetOrganisator
    {
        /// <summary>
        /// Gets the netId of this application.
        /// WARNING: the <see cref="IPeer.Id"/> is a ushort. This is only an int because we need negative values for non peer and also reach the ushort max limit with a now signed value.
        /// -2 is offline, -1 is server, 0 -> <see cref="UInt16.MaxValue"/> is client.
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

       

        /// <summary>
        /// The handler used for any output that needs to be send to the clients.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="deliveryMethod">How it should be send.</param>
        /// <param name="sendTo">List containing every Peer this data should be send to or Null if it should be send to everyone.</param>
        public delegate void OutputHandler(byte[] data, NetSyncDeliveryMethod deliveryMethod, IEnumerable<IPeer> sendTo);
        public static OutputHandler outputHandler;
        public static void StartAsServer(OutputHandler handler)
        {
            if (NetState != ENetState.Offline) throw new InvalidOperationException("Cannot start Server while there is an active connection of Type: " + NetState);
            NetState = ENetState.Server;
            outputHandler = handler;
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

        public static void StartAsClient()
        {
            if (NetState != ENetState.Offline) throw new InvalidOperationException("Cannot start Server while there is an active connection of Type: " + NetState);
            NetState = ENetState.Client;
            ClientNetObjectHandler = new ClientNetObjectHandler<INetObject>();
        }


        /// <summary>
        /// If Server: Will send data to the given peers. Leaving <see cref="sendTo"/> == null for sending to everyone.
        /// <para></para>
        /// If Client: Will send data to the server. Param <see cref="sendTo"/> will be ignored.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="options"></param>
        /// <param name="sendTo">The peers this message should be send to. If null it will be sent to everybody.</param>
        public static void Send(DataWriter writer, NetSyncDeliveryMethod options, IEnumerable<IPeer> sendTo = null)
        {
            if (IsClient())
            {
                outputHandler(writer.CopyData(), options, null);
            }
            else if(IsServer())
            {
                outputHandler(writer.CopyData(), options, sendTo);
            }
            else
            {
                throw new InvalidOperationException("Cannot Send data while offline");
            }
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
    }
}