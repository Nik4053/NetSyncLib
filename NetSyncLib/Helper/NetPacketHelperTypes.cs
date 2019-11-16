using NetSyncLib.Helper.ValueManager;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetSyncLib.Helper
{
    public static class NetPacketHelperTypes
    {
        static NetPacketHelperTypes()
        {
            handlers = new Dictionary<Type, NetPacketHelperTypes.NetValueHandler>();
            //BasicValueHandler.InitBasicHandlers();
            ReflectiveSearchForNetValueHandlers();
        }

        private static void ReflectiveSearchForNetValueHandlers()
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            foreach (MethodInfo method in HelperMethods.GetMethodsWith<NetValueHandlerAttribute>(false, bindingFlags))
            {
                var del = (Action)method.CreateDelegate(typeof(Action));
                del();
            }
        }

        public abstract class NetValueHandler
        {
            public abstract NetPropertyPacket CreateNetPropertyPacket(ulong tag, MemberInfo propertyInfo, NetSynchronizeAttribute attribute, object initVal = default);
        }

        public class NetValueHandler<TValueType> : NetValueHandler
        {
            public ValueSender<TValueType> Sender;
            public ValueReceiver<TValueType> Receiver;

            public NetValueHandler(ValueSender<TValueType> sender, ValueReceiver<TValueType> receiver)
            {
                this.Sender = sender;
                this.Receiver = receiver;
            }

            public override NetPropertyPacket CreateNetPropertyPacket(ulong tag, MemberInfo propertyInfo, NetSynchronizeAttribute attribute, object initValue= default)
            {
                return new NetPropertyPacket<TValueType>(tag, propertyInfo, attribute, this, (TValueType)initValue);
            }

            public override string ToString()
            {
                string msg = base.ToString();
                msg += $"| Sender: {this.Sender}, Receiver: {this.Receiver}";
                return msg;
            }
        }

        private static readonly Dictionary<Type, NetValueHandler> handlers;

        /// <summary>
        /// Gets a dictionary containing all NetValueHandlers accessible by type.
        /// </summary>
        public static IReadOnlyDictionary<Type, NetValueHandler> Handlers => handlers;

        public static ValueSender<TValueType> GetSender<TValueType>()
        {
            ValueSender<TValueType> val =
                ((NetValueHandler<TValueType>)handlers[typeof(TValueType)]).Sender;
            return val;
        }

        public static ValueReceiver<TValueType> GetReceiver<TValueType>()
        {
            ValueReceiver<TValueType> val =
                ((NetValueHandler<TValueType>)handlers[typeof(TValueType)]).Receiver;
            return val;
        }

        public static NetValueHandler<TValueType> GetValueHandler<TValueType>()
        {
            NetValueHandler<TValueType> val =
                (NetValueHandler<TValueType>)handlers[typeof(TValueType)];
            return val;
        }

        public static bool TryGetValueHandler<TValueType>(out NetValueHandler<TValueType> valueHandler)
        {
            if (handlers.TryGetValue(typeof(TValueType), out NetValueHandler val))
            {
                valueHandler = (NetValueHandler<TValueType>)val;
                return true;
            }

            valueHandler = null;
            return false;
        }

        /// <summary>
        /// Warning changing the sender without knowing what the receiver does can and most likely will break the synchronization.
        /// </summary>
        /// <typeparam name="TValueType">The type of the sender to change.</typeparam>
        /// <param name="senderToChange">The new sender.</param>
        public static void ChangeSender<TValueType>(ValueSender<TValueType> senderToChange)
        {
            GetValueHandler<TValueType>().Sender = senderToChange;
        }

        /// <summary>
        /// Warning changing the receiver without knowing what the sender does can and most likely will break the synchronization.
        /// If there is no current handler for the receiver the method will trow an exception.
        /// </summary>
        /// <typeparam name="TValueType">The type of the receiver to change.</typeparam>
        /// <param name="receiverToChange">The new receiver.</param>
        public static void ChangeReceiver<TValueType>(ValueReceiver<TValueType> receiverToChange)
        {
            GetValueHandler<TValueType>().Receiver = receiverToChange;
        }

        /// <summary>
        /// Will add the given handler to the handler list. If a handler that manages the same Type already exist it will be removed an this one used instead.
        /// </summary>
        /// <typeparam name="TValueType">The type the handler is for.</typeparam>
        /// <param name="handler">the new handler.</param>
        public static void AddValueHandler<TValueType>(NetValueHandler<TValueType> handler)
        {
            if (typeof(TValueType).IsSubclassOf(typeof(INetObject)))
            {
                throw new NotSupportedException("NetObjects will always be synchronized by reference and therefore cannot have a value handler");
            }
#if DEBUG
            if (handlers.ContainsKey(typeof(TValueType))) Console.Error.WriteLine($"Warning overwriting NetValueHandler of type {typeof(TValueType)}: OldHandler {handlers[typeof(TValueType)]}, NewHandler: {handler}");
#endif
            NetPacketHelperTypes.handlers.Add(typeof(TValueType), handler);
        }

        public static void RemoveValueHandler<TValueType>()
        {
            handlers.Remove(typeof(TValueType));
        }
    }
}