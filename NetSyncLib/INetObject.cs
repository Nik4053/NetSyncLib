using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.NetLibInterfaces;
using System.Collections.Generic;

namespace NetSyncLib
{
    public interface INetObject : INetSerializable
    {
        /// <summary>
        /// Will check for updates and send them to the clients.
        /// Packet id has to be included into the writer from inside this method.
        /// </summary>
        /// <param name="sendTo">The clients to send this update to. If null it will be sent to everybody. If no content given data will not be send.</param>
        void NetServerSendUpdate(IEnumerable<IPeer> sendTo = null);

        void NetClientReceiveUpdate(NetDataReader reader);

        /// <summary>
        /// Calling this method yourself is discouraged.
        /// Allows this INetObject to initialize before it gets registered. Will be called automatically on <see cref="INetObjectExt.TryRegister(INetObject)"/>, or <see cref="INetObjectExt.Register(INetObject)"/>.
        /// Will only be called server-side. Use <see cref="INetSerializable.Deserialize(NetDataReader)"/> client-side.
        /// </summary>
        void InitializeNetObject();
    }
}