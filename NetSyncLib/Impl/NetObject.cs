using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;

namespace NetSyncLib.Impl
{
    /// <summary>
    /// Basic Implementation of <see cref="INetObject"/>. Uses <see cref="netPacketHelper"/> to synchronize fields/properties marked with <see cref="NetSynchronizeAttribute"/>.
    /// Automatically calls <see cref="INetObjectExt.TryRegister"/> on creation and <see cref="INetObjectExt.TryUnregister"/> on destructor.
    /// </summary>
    public abstract class NetObject : SimpleNetObject
    {
        private NetPacketHelper netPacketHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetObject"/> class.
        /// </summary>
        /// <param name="autoInitialize">if false use <see cref="InitializeNetObject"/> method yourself.</param>
        protected NetObject() : base()
        {
        }

        protected override void OnInitializeNetObject()
        {
#if DEBUG
            if (this.netPacketHelper != null) throw new InvalidOperationException("tried to init net object thats netpackethelper has already been set");
#endif
            this.netPacketHelper = new NetPacketHelper(this);
            base.OnInitializeNetObject();
        }

        public override void Serialize(DataWriter writer)
        {
            this.netPacketHelper.ResendAll(writer);
        }

        public override void Deserialize(DataReader reader)
        {
#if DEBUG
            if (this.netPacketHelper != null) throw new InvalidOperationException("tried to deserialize net object thats netpackethelper has already been set");
#endif
            this.netPacketHelper = new NetPacketHelper(this);
            this.netPacketHelper.ReceiveUpdate(reader);
        }

        public sealed override void NetServerSendUpdate(IEnumerable<IPeer> sendTo = null)
        {
            if (NetOrganisator.IsServer())
            {
                NetPacketHelper.NetPacketHelperUpdate[] updates = this.netPacketHelper.SendUpdate();
                if (updates[0].Changes)
                {
                    this.TrySendNetUpdate(updates[0].Writer, NetSyncDeliveryMethod.ReliableOrdered, sendTo);
                }

                if (updates[1].Changes)
                {
                    this.TrySendNetUpdate(updates[1].Writer, NetSyncDeliveryMethod.ReliableUnordered, sendTo);
                }

                if (updates[2].Changes)
                {
                    this.TrySendNetUpdate(updates[2].Writer, NetSyncDeliveryMethod.Unreliable, sendTo);
                }
            }
        }

        public sealed override void NetClientReceiveUpdate(DataReader reader)
        {
            this.netPacketHelper.ReceiveUpdate(reader);
        }
    }
}