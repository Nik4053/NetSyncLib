﻿using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using System;
using System.Collections.Generic;

namespace NetSyncLib.Client
{
    public static class ClientNetPacketTypes
    {
        public const byte TypeSetPeerId = 1;
        public const byte TypeCreateINetObject = 2;
        public const byte TypeUpdateINetObject = 3;
        public const byte TypeDestroyINetObject = 4;
        public const byte TypeTextMessage = 5;

        public static void SendSetPeerId(int id, IPeer peer)
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeSetPeerId);
            writer.Put(id);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableOrdered, new List<IPeer> { peer });
        }

        public static void ReadSetPeerId(DataReader reader, IPeer peer = null, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            int newId = reader.GetInt();
            if (NetOrganisator.NetPeerId != -2) Console.Error.WriteLine("Set peer id was called by the server but id was already set. " + $"OldId: {NetOrganisator.NetPeerId}, NewId: {newId}");
            NetOrganisator.NetPeerId = newId;
        }

        public static void SendCreateINetObject(INetObject netObject, IEnumerable<IPeer> sendTo = null)
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeCreateINetObject);
            writer.Put(NetOrganisator.ServerNetObjectHandler[netObject]);
            writer.Put(netObject.GetType().AssemblyQualifiedName);
            netObject.Serialize(writer);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableOrdered, sendTo);
        }

        public static void ReadCreateINetObject(DataReader reader, IPeer peer = null, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            Console.Write("Getting INetObject");
            ushort netId = reader.GetUShort();
            string path = reader.GetString();
            INetObject netObject = (INetObject)NetHelper.CreateObject(path);
            NetOrganisator.ClientNetObjectHandler.AddObjectWithKey(netId, netObject);
            netObject.Deserialize(reader);
            Console.Write($", id: {netId}, type: {path} \n");

            // return netObject;
        }

        public static void SendDestroyINetObject(INetObject netObject)
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeDestroyINetObject);
            writer.Put(NetOrganisator.ServerNetObjectHandler[netObject]);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableOrdered);
        }

        public static void ReadDestroyINetObject(DataReader reader, IPeer peer = null, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            Console.Write("Destroying INetObject");
            ushort netId = reader.GetUShort();
            Console.Write($", id: {netId} \n");
            if (!NetOrganisator.ClientNetObjectHandler.RemoveObject(netId)) Console.Error.WriteLine($"Tried to destroy netObject of id {netId}, but failed");
        }

        /// <summary>
        /// Will crash if the caller is not the server.
        /// Will add the required header for the client to know to what netobject this data should go to. So don't include this data yourself!.
        /// </summary>
        /// <param name="netObject"></param>
        /// <param name="writer"></param>
        /// <param name="deliveryMethod"></param>
        /// <param name="sendTo">The peers this message should be send to. If null it will be sent to everybody.</param>
        public static void SendUpdateINetObject(INetObject netObject, DataWriter writer, NetSyncDeliveryMethod deliveryMethod, IEnumerable<IPeer> sendTo = null)
        {
            DataWriter finalWriter = new DataWriter();
            finalWriter.Put(TypeUpdateINetObject);
            finalWriter.Put(NetOrganisator.ServerNetObjectHandler[netObject]);
            finalWriter.Put(writer.Data, 0, writer.Length);
            NetOrganisator.Send(finalWriter, deliveryMethod, sendTo);
        }

        public static void ReadUpdateINetObject(DataReader reader, IPeer peer = null, NetSyncDeliveryMethod deliveryMethod = NetSyncDeliveryMethod.Unreliable)
        {
            ushort netId = reader.GetUShort();
            if (NetOrganisator.ClientNetObjectHandler.NetObjects.TryGetValue(netId, out INetObject netObject))
            {
                netObject.NetClientReceiveUpdate(reader);
            }
            else
            {
                Console.Error.WriteLine($"Tried to update Netobject of id {netId}, but couldn't find it");
            }
        }

        public static void SendMessage(string message, IEnumerable<IPeer> sendTo = null)
        {
            DataWriter writer = new DataWriter();
            writer.Put(TypeTextMessage);
            writer.Put(message);
            NetOrganisator.Send(writer, NetSyncDeliveryMethod.ReliableUnordered, sendTo);
        }

        public static void ReadMessage(DataReader reader, IPeer peer, NetSyncDeliveryMethod deliveryMethod)
        {
            Console.WriteLine(reader.GetString());
        }
    }
}