using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetSyncLib.Server
{
    internal class ServerWeakNetObjectHandler<TKey>
        where TKey : class
    {
        private const int RecycleThreshold = 100;

        /// <summary>
        /// List containing ids that have been recycled and are ready to be used again.
        /// </summary>
        private readonly Queue<Id> recycledIdList = new Queue<Id>();

        /// <summary>
        /// Id zero is reserved.
        /// </summary>
        private readonly ConditionalWeakTable<TKey, Id> netObjects = new ConditionalWeakTable<TKey, Id>();

        private List<WeakReference<TKey>> cachedValues = new List<WeakReference<TKey>>();

        /// <summary>
        /// The current value of the generator used for getting the id of the next object. Starting with 1.
        /// </summary>
        private ushort generatorId = 1;

        public ushort this[TKey netObject] => this.GetId(netObject).Val;

        /// <summary>
        /// Adds the given <see cref="TKey"/>.
        /// </summary>
        /// <param name="netObject">The <see cref="TKey"/> to add.</param>
        /// <exception cref="ArgumentException">Adding an already added Object will result in an Exception.</exception>
        /// <returns>The Id of the <see cref="TKey"/>.</returns>
        public ushort AddObject(TKey netObject)
        {
            if (this.TryGetId(netObject, out ushort id))
            {
                throw new ArgumentException("The given netObject is already registered with id: " + id);
            }

            Id newId = this.GenerateNewId();
            Console.WriteLine($"Added new NetObject of Id: {newId.Val} : {netObject}");
            this.netObjects.Add(netObject, newId);
            this.cachedValues.Add(new WeakReference<TKey>(netObject));
            return newId.Val;
        }

        public List<TKey> GetAllKeys()
        {
            List<TKey> keys = new List<TKey>();
            List<WeakReference<TKey>> newCachedValues = new List<WeakReference<TKey>>();
            foreach (WeakReference<TKey> weakKey in this.cachedValues)
            {
                if (weakKey.TryGetTarget(out TKey target))
                {
                    keys.Add(target);
                    newCachedValues.Add(weakKey);
                }
            }

            this.cachedValues = newCachedValues;
            return keys;
        }

        private void RemoveCachedValue(TKey obj)
        {
            for (int i = 0; i < this.cachedValues.Count; i++)
            {
                WeakReference<TKey> weakKey = this.cachedValues[i];
                if (!weakKey.TryGetTarget(out TKey target))
                {
                    this.cachedValues.RemoveAt(i);
                    i--;
                    continue;
                }

                if (target == obj)
                {
                    this.cachedValues.RemoveAt(i);
                    break;
                }
            }
        }

        public bool RemoveObject(TKey obj)
        {
            Id id = this.GetId(obj); // will trow exception if key doesn't exist
            this.recycledIdList.Enqueue(id);
            this.RemoveCachedValue(obj);
            return this.netObjects.Remove(obj);
        }

        public bool TryGetId(TKey netObject, out ushort id)
        {
            id = 0;
            if (netObject == null) return false;
            if (!this.netObjects.TryGetValue(netObject, out Id idC)) return false;
            id = idC.Val;
            return true;
        }

        private Id GenerateNewId()
        {
            if (this.recycledIdList.Count > RecycleThreshold)
            {
                return this.recycledIdList.Dequeue();
            }

            return new Id(this.generatorId++);
        }

        private Id GetId(TKey netObject)
        {
            bool result = this.netObjects.TryGetValue(netObject, out Id id);
            if (!result)
            {
                throw new KeyNotFoundException("The given netObject is not added to the List");
            }

            return id;
        }

        private class Id
        {
            internal Id(ushort val)
            {
                this.Val = val;
            }

            internal ushort Val { get; }
        }
    }
}