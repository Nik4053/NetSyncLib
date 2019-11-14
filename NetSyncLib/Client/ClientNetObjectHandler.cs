using System;
using System.Collections.Generic;

namespace NetSyncLib.Client
{
    internal class ClientNetObjectHandler<TValue>
    {
        private readonly Dictionary<ushort, TValue> netObjects = new Dictionary<ushort, TValue>();

        internal IReadOnlyDictionary<ushort, TValue> NetObjects => this.netObjects;

        internal void AddObjectWithKey(ushort id, TValue obj)
        {
            if (this.netObjects.ContainsValue(obj))
            {
                ushort currKey = this.GetIdOfNetObject(obj);
                throw new ArgumentException($"Given {typeof(TValue).Name} was already added with the given key. CurrentKey: " + currKey + " NewKey:" + id + " ObjectType: " + obj.GetType().FullName);
            }

            if (this.netObjects.ContainsKey(id))
            {
                if (this.netObjects[id].GetType().Equals(obj.GetType()))
                {
                    throw new ArgumentException($"Given {typeof(TValue).Name} was already added with the given key: " + id + " ObjectType: " + obj.GetType().FullName);
                }
                else
                {
                    throw new ArgumentException("Given key was already added and used by another netObject: " + id + " OtherObjectType: " + this.netObjects[id].GetType().FullName + " ThisObjectType: " + obj.GetType().FullName);
                }
            }

            // End of prechecks
            this.netObjects.Add(id, obj);
        }

        internal bool RemoveObject(ushort id)
        {
            return this.netObjects.Remove(id);
        }

        /// <summary>
        /// WARNING!! NOT RECOMMENDED Method. Will search trough all objects to find a key. Should not be used.
        /// </summary>
        /// <param name="obj">The object to search for.</param>
        /// <returns>The id of the object.</returns>
        internal ushort GetIdOfNetObject(TValue obj)
        {
            // get key of value
            ushort currKey = 0;
            bool isSet = false;
            foreach (ushort ekey in this.netObjects.Keys)
            {
                if (this.netObjects[ekey].Equals(obj))
                {
                    currKey = ekey;
                    isSet = true;
                    continue;
                }
            }

            if (!isSet)
            {
                throw new ArgumentException("Given netobject is not part of this Handler. ObjectType: " + obj.GetType().FullName);
            }

            return currKey;
        }
    }
}