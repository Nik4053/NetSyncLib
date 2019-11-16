using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSyncLib.Client;
using NetSyncLib.Helper;
using NetSyncLib.Server;
using NetSyncLibForLiteNetLib.Client;

namespace NetSyncLib.Tests.ServerTests
{
    [TestClass]
    public class ServerWeakNetObjectHandlerTests
    {
        [TestMethod]
        public void TestRegister()
        {
            NetOrganisator.ResetNet();
            NetOrganisator.StartAsServer();
            EmptyNetObject netObject = new EmptyNetObject();
            netObject.testInt = 9876;
            netObject.Register();
            Byte[] data = NetOrganisator.TESTDATA;
            ushort id = NetOrganisator.ServerNetObjectHandler[netObject];
            netObject.testInt = -6789;
            netObject.NetServerSendUpdate();
            Byte[] data2 = NetOrganisator.TESTDATA;
            NetOrganisator.ResetNet();
            NetOrganisator.StartAsClient();
            ClientListener listener = new ClientListener();
            listener.OnNetworkReceiveEvent(null, new NetDataReader(data), NetSyncDeliveryMethod.ReliableOrdered);
            INetObject netObject2 = null;
            NetOrganisator.ClientNetObjectHandler.NetObjects.TryGetValue(id,out netObject2);
            Assert.AreEqual(9876, ((EmptyNetObject)netObject2).testInt);
            listener.OnNetworkReceiveEvent(null, new NetDataReader(data2), NetSyncDeliveryMethod.ReliableOrdered);
            Assert.AreEqual(-6789, ((EmptyNetObject)netObject2).testInt);



        }
        [TestMethod]
        public void GetObject()
        {
            ServerWeakNetObjectHandler<INetObject> handler = new ServerWeakNetObjectHandler<INetObject>();
            INetObject netObject = new EmptyNetObject();
            ushort id = handler.AddObject(netObject);
            INetObject obj = handler.GetAllKeys()[0];
            if (obj != netObject)
                Assert.Fail();
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddObjectAgain()
        {
            ServerWeakNetObjectHandler<INetObject> handler = new ServerWeakNetObjectHandler<INetObject>();
            INetObject netObject = new EmptyNetObject();
            handler.AddObject(netObject);
            handler.AddObject(netObject);
        }
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetNotExistingObject()
        {
            ServerWeakNetObjectHandler<INetObject> handler = new ServerWeakNetObjectHandler<INetObject>();
            INetObject netObject = new EmptyNetObject();
            ushort id = handler[netObject];
        }
        [TestMethod]
        public void IdReuseOfGarbageCollectedObject()
        {
            ServerWeakNetObjectHandler<INetObject> handler = new ServerWeakNetObjectHandler<INetObject>();
            WeakReference weakObject = IdReuseOfGarbageCollectedObject2(handler, out ushort id);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (weakObject.IsAlive)
                Assert.Fail("NetObject was not garbage collected");
            INetObject netObject = new EmptyNetObject();
            ushort newId = handler.AddObject(netObject);
            Assert.IsTrue(id != newId, $"Ids have to be the same. Id1: {id}, Id2: {newId}");
        }

        private WeakReference IdReuseOfGarbageCollectedObject2(ServerWeakNetObjectHandler<INetObject> handler, out ushort id)
        {
            INetObject netObject = new EmptyNetObject();
            id = handler.AddObject(netObject);
            return new WeakReference(netObject);
        }
        [TestMethod]
        public void CheckKeyReuse()
        {
            ServerWeakNetObjectHandler<INetObject> handler = new ServerWeakNetObjectHandler<INetObject>();
            for (int i = 0; i < ServerWeakNetObjectHandler<INetObject>.RecycleThreshold+1; i++)
            {
                INetObject netObject = new EmptyNetObject();
                handler.AddObject(netObject);
                ushort id = handler[netObject];
                if (i+1 != id) Assert.Fail($"Key should have been the same but wasn't. Expected: {i} Was: {id}");
                handler.RemoveObject(netObject);
            }
            INetObject finalObject = new EmptyNetObject();
            handler.AddObject(finalObject);
            if (1 != handler[finalObject]) Assert.Fail($"Key should have been the same but wasn't. OldKey: {1} NewKey: {handler[finalObject]}");
        }
    }
}
