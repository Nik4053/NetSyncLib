using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetSyncLib
{
    public interface INetController : INetObject
    {
        /// <summary>
        /// Gets the id of the owner in multi-player mode. That allows one client to change the state of this object on the server. The starting value should be set to -2.
        /// <para></para>IMPORTANT!!! Make sure to include a private set in your implementation. Synchronizing will fail if the value cannot be set.
        /// <para>-2 for not initialized.</para>
        /// <para>-1 for not server.</para>
        /// <para>starting at 0 for not client id.</para>
        /// <para>-3 for everyone.</para>
        /// <para>other negative numbers can be used as wished.</para>
        /// </summary>
        int OwnerId { get; }

        /// <summary>
        /// Gets a reference to this object for server side validation.
        /// Make sure this value never changes. Only necessary for the server. Client side the value of this property should be treated as undefined.
        /// </summary>
        WeakReference<INetController> WeakReference { get; }

        /// <summary>
        /// Client sends status to server.
        /// </summary>
        void NetClientSendUpdate();

        /// <summary>
        /// Server receives status and checks.
        /// </summary>
        /// <param name="clientPeer">The client that send the message.</param>
        void NetServerReceiveUpdate(NetPeer clientPeer, NetDataReader reader);

        /// <summary>
        /// Returns true if the given peerId has the rights to change this controller.
        /// </summary>
        /// <param name="ownerId">The id of the peer to check.</param>
        /// <returns>true if the given peerId has the rights to change this controller.</returns>
        bool IsOwner(int ownerId);
    }
}