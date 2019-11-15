using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.NetLibInterfaces;

namespace NetSyncLib.Impl
{
    /// <summary>
    /// A <see cref="INetObject"/> implementation that automatically registers itself, but doesn't have an <see cref="NetSyncLib.Helper.NetPacketHelper"/> like <see cref="NetObject"/> has.
    /// </summary>
    public abstract class SimpleNetObject : INetObject
    {
        protected SimpleNetObject()
        {
        }

        ~SimpleNetObject()
        {
            this.TryUnregister();
        }

        /// <summary>
        /// Calling this method yourself is discouraged.
        /// Use <see cref="SimpleNetObjectExt.InitializeNetObject{T}(T, string)"/> instead of calling this method directly.
        /// See <see cref="INetObjectExt.Register(INetObject)"/> for timing information.
        /// </summary>
        protected virtual void OnInitializeNetObject()
        {
        }

        /// <summary>
        /// Wrapper for <see cref="INetObjectExt.Register(INetObject)"/> that checks if the object that should be registered is of the highest possible type it can be. Useful for automatic constructor initialization, because all calls to this method will be ignored that do come from base constructors. The object that will be initialized has to be fully constructed before doing so.
        /// This method will make sure that if it is called by an constructor only the highest constructor calls <see cref="INetObjectExt.Register(INetObject)"/>.
        /// </summary>
        protected static void AutoRegisterNetObject<T>(T obj) where T : Impl.SimpleNetObject
        {
            if (obj.GetType() == typeof(T))
                obj.Register();
            return;
        }

        /// <summary>
        /// Calling this method yourself is discouraged.
        /// Will initialize this <see cref="SimpleNetObject"/>.
        /// <para></para>Note that this method is only a wrapper for calling <see cref="SimpleNetObject.OnInitializeNetObject"/>. Calling <see cref="SimpleNetObject.OnInitializeNetObject"/> directly is discouraged.
        /// Will be automatically called by <see cref="INetObjectExt.Register(INetObject)"/>. See <see cref="INetObjectExt.Register(INetObject)"/> for timing information.
        /// </summary>
        public void InitializeNetObject()
        {
            this.OnInitializeNetObject();
        }

        public abstract void Deserialize(NetDataReader reader);

        public abstract void NetClientReceiveUpdate(NetDataReader reader);

        public abstract void NetServerSendUpdate(IEnumerable<IPeer> sendTo = null);

        public abstract void Serialize(NetDataWriter writer);
    }
}