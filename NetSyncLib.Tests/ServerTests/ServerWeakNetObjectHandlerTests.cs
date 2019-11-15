using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSyncLib.Server;

namespace NetSyncLib.Tests.ServerTests
{
    [TestClass]
    public class ServerWeakNetObjectHandlerTests
    {
        [TestMethod]
        public void TestRegister()
        {

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
            INetObject netObject = new EmptyNetObject();
            handler.AddObject(netObject);
            ushort id = handler[netObject];
            handler.RemoveObject(netObject);
            netObject = new EmptyNetObject();
            handler.AddObject(netObject);
            if (id != handler[netObject]) Assert.Fail($"Key should have been the same but wasn't. OldKey: {id} NewKey: {handler[netObject]}");
        }
    }
}
