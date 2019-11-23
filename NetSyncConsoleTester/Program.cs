using LiteNetLib;
using NetSyncLib;
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
            }
        }

        public static bool StartClient(string address = "localhost", int port = 9050, string password = "", string username = "UnknownPlayer", bool autoUpdate = true)
        {
            INetEventListener listener = new EventBasedNetListener();
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
            INetEventListener listener = new EventBasedNetListener();
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
            server.SimulateLatency = true;
            server.SimulationMaxLatency = 1000;

            server.SimulationMinLatency = 1000;
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
                    //ResetNet();
                }).Start();
            }
        }
    }
}
