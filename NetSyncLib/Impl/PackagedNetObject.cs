using System;
using System.Diagnostics;
using System.Reflection;
using LiteNetLib.Utils;
using NetSyncLib.Helper;

namespace NetSyncLib.Impl
{
    /// <summary>
    /// A <see cref="NetObject"/> for classes that cannot extend <see cref="NetObject"/> and should not contain the added complexity of <see cref="INetObject"/>.
    /// This class will itself be an implementation of <see cref="INetObject"/> with your object as a property with <see cref="NetSyncLib.Helper.NetSynchronizeAttribute"/>. Therefor your object will be synchronized like an INetObject. Because of the added dependency every update will have an extra 1byte header.
    /// </summary>
    public sealed class PackagedNetObject : NetObject
    {
        [NetSynchronize(0, NetSyncDeliveryMethod.Unreliable)]
        private object synchronizedObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackagedNetObject"/> class.
        /// A <see cref="NetObject"/> for classes that cannot extend <see cref="NetObject"/> and should not contain the added complexity of <see cref="INetObject"/>.
        /// This class will itself be an implementation of <see cref="INetObject"/> with your object as a property with <see cref="NetSyncLib.Helper.NetSynchronizeAttribute"/>. Therefor your object will be synchronized like an INetObject. Because of the added dependency every update will have an extra 1byte header.
        /// If the given Object is a <see cref="INetSerializable"/> it will be serialized on client side creation.
        /// </summary>
        /// <param name="synchronizedObject">Make sure that the given object has a parameter-less constructor.</param>
        public PackagedNetObject(object synchronizedObject)
        {
            if (synchronizedObject == null) throw new ArgumentNullException($"The given synchronizedObject was null");
            this.CheckHasConstructorAlsoPrivate(synchronizedObject);
            this.synchronizedObject = synchronizedObject;
            this.AutoRegisterNetObject();
        }

        // for automatic net object creation
        private PackagedNetObject()
        {
            // will init on deserialize
        }

        public override void Deserialize(NetDataReader reader)
        {
            string path = reader.GetString();
            this.synchronizedObject = NetHelper.CreateObject(path);
            if (this.synchronizedObject is INetSerializable ser)
            {
                ser.Deserialize(reader);
            }

            base.Deserialize(reader);
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(this.synchronizedObject.GetType().AssemblyQualifiedName);
            if (this.synchronizedObject is INetSerializable ser)
            {
                ser.Serialize(writer);
            }

            base.Serialize(writer);
        }

        protected override void OnInitializeNetObject()
        {
            if (this.synchronizedObject == null) throw new ArgumentNullException($"Tried to init, but synchronizedObject was null.");
            base.OnInitializeNetObject();
        }

        [Conditional("DEBUG")]
        private void CheckHasConstructorAlsoPrivate(object synchronizedObject)
        {
            if (synchronizedObject.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null)
            {
                throw new ArgumentNullException($"The given synchronizedObject has no parameterless constructor");
            }
        }
    }
}