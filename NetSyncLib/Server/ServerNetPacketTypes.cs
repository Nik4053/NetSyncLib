using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Client;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using System;
using System.Collections.Generic;

namespace NetSyncLib.Server
{
    public static class ServerNetPacketTypes
    {
        public const byte TypeCloneAll = 1;
        public const byte TypeUpdateController = 2;
        public const byte TypeTextMessage = 3;

        public static void SendUpdateController(INetController netObject, DataWriter dataWriter, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.ReliableOrdered)
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeUpdateController);
            writer.Put(NetOrganisator.ClientNetObjectHandler.GetIdOfNetObject(netObject));
            writer.Put(netObject.OwnerId);
            writer.Put(dataWriter.Data, 0, dataWriter.Length);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableOrdered);
        }

        public static void ReadUpdateController(DataReader reader, IPeer peer, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            Console.Write("Getting controller update");
            ushort netId = reader.GetUShort();
            Console.Write($", id: {netId} \n");
            INetController con = NetOrganisator.ControllerServerHandler.GetController(reader.GetInt(), netId);
            if (con == null)
            {
                Console.Error.WriteLine($"Received update for controller that didn't exist.");
                return;
            }

            con.NetServerReceiveUpdate(peer, reader);
        }

        public static void SendMessage(string message)
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeTextMessage);
            writer.Put(message);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableUnordered);
        }

        public static void ReadMessage(DataReader reader, IPeer peer = null, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            string message = reader.GetString();
            var name = peer.Id;
            //if (peer == null && NetOrganisator.IsServer()) name = "Server";
            message = name + ": " + message;
            Console.WriteLine(message);
            ClientNetPacketTypes.SendMessage(message);
        }

        public static void SendCloneAllRequest()
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeCloneAll);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableOrdered);
        }

        public static void ReadCloneAll(DataReader reader, IPeer peer = null, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            NetOrganisator.ResendAllNetObjects(new List<IPeer> { peer });
        }
    }
}