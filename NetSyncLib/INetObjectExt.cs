using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.Client;
using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using System.Collections.Generic;

namespace NetSyncLib
{
    public static class INetObjectExt
    {
        /// <summary>
        /// Warning if you don't know what this method is for use <see cref="Register(INetObject)"/> instead.
        /// Wrapper for <see cref="INetObjectExt.Register(INetObject)"/> that checks if the object that should be registered is of the highest possible type it can be. Useful for automatic constructor initialization, because all calls to this method, that do come from base constructors, will be ignored . The object that will be initialized has to be fully constructed before doing so.
        /// This method will make sure that if it is called by an constructor only the highest constructor calls <see cref="INetObjectExt.Register(INetObject)"/>.
        /// </summary>
        public static void AutoRegisterNetObject<T>(this T obj) where T : INetObject
        {
            if (obj.GetType() == typeof(T))
                obj.Register();
            return;
        }

        /// <summary>
        /// Warning if you don't know what this method is for use <see cref="TryRegister(INetObject)"/> instead.
        /// Wrapper for <see cref="INetObjectExt.TryRegister(INetObject)"/> that checks if the object that should be registered is of the highest possible type it can be. Useful for automatic constructor initialization, because all calls to this method, that do come from base constructors, will be ignored . The object that will be initialized has to be fully constructed before doing so.
        /// This method will make sure that if it is called by an constructor only the highest constructor calls <see cref="INetObjectExt.TryRegister(INetObject)"/>.
        /// </summary>
        public static void TryAutoRegisterNetObject<T>(this T obj) where T : INetObject
        {
            if (obj.GetType() == typeof(T))
                obj.TryRegister();
            return;
        }

        /// <summary>
        /// Tries to register the given <see cref="INetObject"/>. Only works <see cref="NetState"/> == <see cref="ENetState.Server"/>. Else will do nothing and return false.
        /// <para></para>If you call this from the constructor of an <see cref="INetObject"/> or a derived class it is recommended to use <see cref="TryAutoRegisterNetObject{T}(T)"/>.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <returns>True if the given obj was added.</returns>
        public static bool TryRegister(this INetObject obj)
        {
            switch (NetOrganisator.NetState)
            {
                case ENetState.Server:
                    Register(obj);
                    return true;

                default: return false;
            }
        }

        /// <summary>
        /// Registers the given <see cref="INetObject"/>. Only works <see cref="NetState"/> == <see cref="ENetState.Server"/>. Else will throw exception. Will call <see cref="INetObject.InitializeNetObject"/> after adding the object to the <see cref="Server.ServerWeakNetObjectHandler{TKey}"/> and before sending the client the call to create a new object.
        /// <para></para>If you call this from the constructor of an <see cref="INetObject"/> or a derived class it is recommended to use <see cref="AutoRegisterNetObject{T}(T)"/>.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        public static void Register(this INetObject obj)
        {
            NetOrganisator.ServerNetObjectHandler.AddObject(obj);
            obj.InitializeNetObject();
            ClientNetPacketTypes.SendCreateINetObject(obj);
        }

        /// <summary>
        /// Tries to unregister the given <see cref="INetObject"/>. Only works <see cref="NetState"/> == <see cref="ENetState.Server"/>. Else will do nothing and return false.
        /// </summary>
        /// <param name="obj">The object to unregister.</param>
        /// <returns>True if the given obj was unregistered.</returns>
        public static bool TryUnregister(this INetObject obj)
        {
            switch (NetOrganisator.NetState)
            {
                case ENetState.Server:
                    if (!NetOrganisator.ServerNetObjectHandler.TryGetId(obj, out ushort id))
                    {
                        System.Console.Error.WriteLine("Tried to destroy INetObject that was not registered. Type: " + obj.GetType() + " | Instance: " + obj);
                        return false;
                    }

                    ClientNetPacketTypes.SendDestroyINetObject(obj);
                    NetOrganisator.ServerNetObjectHandler.RemoveObject(obj);
                    return true;

                default: return false;
            }
        }

        /// <summary>
        /// Unregisters the given <see cref="INetObject"/>. Only works <see cref="NetState"/> == <see cref="ENetState.Server"/>. Else will throw exception.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        public static void Unregister(this INetObject obj)
        {
            if (!NetOrganisator.ServerNetObjectHandler.TryGetId(obj, out ushort id))
            {
                System.Console.Error.WriteLine("Tried to destroy INetObject that was not registered. Type: " + obj.GetType() + " | Instance: " + obj);
                return;
            }

            ClientNetPacketTypes.SendDestroyINetObject(obj);
            NetOrganisator.ServerNetObjectHandler.RemoveObject(obj);
        }

        /// <summary>
        /// Sends the given data to all clients. This method will only resolve if the caller is the server. Will do nothing if the caller is not the server.
        /// Will add the required header for the client to know to what netobject this data should go to. So don't include this data yourself!.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writer">The required header for the client to know to what netobject this data should go to will automatically be included. So don't include this data yourself!.</param>
        /// <param name="deliveryMethod"></param>
        /// <param name="sendTo">The peers this message should be send to. If null it will be sent to everybody</param>
        public static void TrySendNetUpdate(this INetObject obj, NetDataWriter writer, NetSyncDeliveryMethod deliveryMethod, IEnumerable<IPeer> sendTo = null)
        {
            if (!NetOrganisator.IsServer()) return;
            ClientNetPacketTypes.SendUpdateINetObject(obj, writer, deliveryMethod,sendTo);
        }
    }
}