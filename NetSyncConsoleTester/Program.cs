using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using NetSyncLib.Tests;
using NetSyncLibForLiteNetLib.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSyncConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo keyInfo=  Console.ReadKey();
            if (keyInfo.KeyChar == 's')
            {
                Console.WriteLine("Starting as Server");
                StartServer();
            }
            else
            {
                Console.WriteLine("Starting as Client");
                StartClient();
            }
            while (true) { }
        }

        public static bool StartClient(string address = "localhost", int port = 9050, string password = "", string username = "UnknownPlayer", bool autoUpdate = true)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            NetManager client = new NetManager(listener);
            client.Start();
           
            NetPeer connection = client.Connect(address, port,"HELLO");
            NetOrganisator.StartAsClient();
            while (connection.ConnectionState == ConnectionState.Outcoming)
            {
                client.PollEvents();

                // Console.WriteLine("Client: listening...");
                Thread.Sleep(15);
            }
            if(connection.ConnectionState == ConnectionState.Disconnected)
            {

                Console.WriteLine("No Server Found");
            }
            if (autoUpdate)
            {
                new Thread(() =>
                {
                    while (!Console.KeyAvailable)
                    {
                        client.PollEvents();
                        Thread.Sleep(5);
                    }

                    Console.WriteLine("Disconnected from Server");
                    client.Stop();
                }).Start();
            }

            return true;
        }
        public static void StartServer(string addressIPv4 = null, string addressIPv6 = null, int port = 9050, string password = "", int maxConnections = 10, bool autoUpdate = true)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
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
            if (!server.Start(ipv4, ipv6, port))
            {
                Console.WriteLine("ERROR CREATING SERVER"); return;
            }

            NetOrganisator.StartAsServer(DelayedReaderHandler.ReaderHandler);
            // serverThread
            new Thread(() =>
            {
                ulong bytesSend = 0;
                ulong packetsSend = 0;
                while (!Console.KeyAvailable)
                {
                    ulong newBytesSend = server.Statistics.BytesSent;
                    ulong newPacketsSend = server.Statistics.PacketsSent;
                    Console.WriteLine($"Send: {newBytesSend - bytesSend} Bytes/s, {newPacketsSend - packetsSend} Packages/s, {server.Statistics.PacketLossPercent}% PacketLoss");
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
                    //ResetNet();
                }).Start();
                new Thread(() =>
                {
                    while (!Console.KeyAvailable)
                    {
                        byte[] data = DelayedReaderHandler.Read();
                        Console.WriteLine("NEW DATA");
                        server.SendToAll(data,DeliveryMethod.ReliableOrdered);

                    }

                    Console.WriteLine("Stopping Reader...");
                    server.Stop();
                    //ResetNet();
                }).Start();
            }

            EmptyNetObject netObject = new EmptyNetObject();
            netObject.testInt = -4000;
            netObject.Register();
            char i = Console.ReadKey().KeyChar;
            Console.WriteLine("RETURNING");

            return;

        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            ClientListener listener2 = new ClientListener();
            listener2.OnNetworkReceiveEvent(null,new DataReader(reader.RawData,reader.UserDataOffset),NetSyncDeliveryMethod.ReliableOrdered);
        }

        private static void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            request.Accept();
            Console.WriteLine("Accepted Request");
            NetOrganisator.InitNewPeer(new Peer {Id=2 });
        }
    }
    public class Peer : IPeer
    {
        public ushort Id { get; set; }
    }
}
