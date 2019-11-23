using System;
using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;

namespace NetSyncLib.Impl
{
    public abstract class NetController : NetObject, INetController
    {
        private int ownerId = -2;

        protected NetController() : this(-2)
        {
        }

        protected NetController(int ownerId) : base()
        {
            if (NetOrganisator.IsServer())
            {
                this.WeakReference = new WeakReference<INetController>(this);
            }

            this.ownerId = ownerId;
        }

        ~NetController()
        {
            NetOrganisator.ControllerServerHandler?.RemoveController(this);
        }

        protected override void OnInitializeNetObject()
        {
            if (NetOrganisator.IsServer())
            {
                NetOrganisator.ControllerServerHandler.AddController(this, this.ownerId);
            }

            base.OnInitializeNetObject();
        }

        /// <summary>
        /// Gets or sets the id of the owner in multi-player mode. That allows one client to change the state of this object on the server. The starting value should be set to -2.
        /// <para></para>IMPORTANT!!! Make sure to include a private set in your implementation. Synchronizing will fail if the value cannot be set.
        /// <para>-2 for not initialized.</para>
        /// <para>-1 for not server.</para>
        /// <para>starting at 0 for not client id.</para>
        /// <para>-3 for everyone.</para>
        /// <para>does not support other negative numbers.</para>
        /// </summary>
        [NetSynchronize(0, NetSyncDeliveryMethod.ReliableOrdered)]
        public int OwnerId
        {
            get => this.ownerId;
            protected set
            {
                if (value != this.ownerId)
                {
                    NetOrganisator.ControllerServerHandler?.ChangeIdOfOwner(this, value);
                    this.ownerId = value;
                }
            }
        }

        public WeakReference<INetController> WeakReference { get; }

        public void NetClientSendUpdate()
        {
            if (NetOrganisator.NetState == ENetState.Client)
            {
                if (this.IsOwner(NetOrganisator.NetPeerId)) this.OnSendStatus();
            }
        }

        public void NetServerReceiveUpdate(IPeer clientPeer, DataReader reader)
        {
            if (NetOrganisator.NetState == ENetState.Server)
            {
                if (this.IsOwner(clientPeer.Id)) this.OnReceiveStatus(clientPeer, reader);
            }
        }

        /// <summary>
        /// Returns true if the given peerId has the rights to change this controller.
        /// </summary>
        /// <param name="peerId"></param>
        /// <returns></returns>
        public virtual bool IsOwner(int peerId)
        {
            if (peerId == this.OwnerId)
            {
                return true;
            }
            else if (this.OwnerId == -3)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Server receives status and checks.
        /// </summary>
        protected abstract void OnReceiveStatus(IPeer clientPeer, DataReader reader);

        /// <summary>
        /// Client sends status to server.
        /// </summary>
        protected abstract void OnSendStatus();
    }
}