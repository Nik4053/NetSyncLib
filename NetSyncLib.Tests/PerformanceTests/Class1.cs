using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NetSyncLib.Tests.PerformanceTests
{
    public enum MapperMode { byKey, byValue };
    public enum MapStrategy { fitToFirst, fitToSecond, makeBalanced, removeUnbalanced };
    public enum DictionarySet { first, second };

    public class DLD<K, V> // Double Linked Dictionary  
    {
        private Dictionary<K, V> map = new Dictionary<K, V>();
        private Dictionary<V, K> reverseMap = new Dictionary<V, K>();
        private object Locker = new object();
        private bool accessed = false; // Direct acces to Dictionaries            

        public V GetValue(K key)
        {
            lock (Locker)
            {
                try
                {
                    return map[key];
                }
                catch
                {
                    return default;
                }
            }
        }

        public K GetKey(V value)
        {
            lock (Locker)
            {
                try
                {
                    return reverseMap[value];
                }
                catch
                {
                    return default;
                }
            }
        }


        public void AddToInner(DictionarySet D, K DKey, V DValue)
        {
            if (D == DictionarySet.first)
            {
                lock (Locker)
                {
                    map.Add(DKey, DValue);
                    accessed = true;
                }
            }
            else
                if (D == DictionarySet.second)
            {
                lock (Locker)
                {
                    reverseMap.Add(DValue, DKey);
                    accessed = true;
                }
            }
        }

        public void RemoveFromInner<T>(DictionarySet D, T pointer) where T : K, V
        {
            if (D == DictionarySet.first && typeof(T) == typeof(K))
            {
                lock (Locker)
                {
                    map.Remove(pointer);
                    accessed = true;
                }
            }
            else
                if (D == DictionarySet.second && typeof(T) == typeof(V))
            {
                lock (Locker)
                {
                    reverseMap.Remove(pointer);
                    accessed = true;
                }
            }
        }

        public int CountInner(DictionarySet D)
        {
            lock (Locker)
            {
                if (D == DictionarySet.first)
                    return map.Count;
                else
                    if (D == DictionarySet.second)
                    return reverseMap.Count;
                else
                    return 0;
            }
        }

        public bool GetConsistencyStatus()
        {  // Key <-> Value  

            bool disbalanced = false;
            if (accessed)
            {
                if (map.Count != reverseMap.Count)
                    disbalanced = true;
                else
                {
                    lock (Locker)
                    {
                        foreach (KeyValuePair<K, V> entry in map)
                        {
                            if (EqualityComparer<K>.Default.Equals(entry.Key, reverseMap[entry.Value]))
                            {
                                continue;
                            }
                            else
                            {
                                disbalanced = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
                disbalanced = false;

            if (disbalanced)
                return false;
            else
                return true;
        }

        public void Add(K key, V value)
        {
            lock (Locker)
            {
                map.Add(key, value);
                reverseMap.Add(value, key);
            }
        }

        public void Remove<GenType>(GenType data, MapperMode mode) where GenType : K, V
        {
            if (mode == MapperMode.byKey && typeof(GenType) == typeof(K))
            {
                lock (Locker)
                {
                    var tempValue = map[data];
                    map.Remove(data);
                    reverseMap.Remove(tempValue);
                }
            }
            else
            if (mode == MapperMode.byValue && typeof(GenType) == typeof(V))
            {
                lock (Locker)
                {
                    var tempKey = reverseMap[data];
                    reverseMap.Remove(data);
                    map.Remove(tempKey);
                }
            }
        }

        public void DLDMapping(MapStrategy mStrategy)
        {
            if (!GetConsistencyStatus())
            {
                switch (mStrategy)
                {
                    case MapStrategy.fitToFirst: //  1 -> 2  
                        lock (Locker)
                        {
                            {
                                foreach (KeyValuePair<V, K> entry in reverseMap)
                                {
                                    if (reverseMap.ContainsKey(entry.Key) &&
                                           !map.ContainsKey(entry.Value))

                                        map.Add(entry.Value, entry.Key);
                                }

                            }
                        }
                        break;

                    case MapStrategy.fitToSecond: // 1 <- 2  
                        {
                            lock (Locker)
                            {
                                foreach (KeyValuePair<K, V> entry in map)
                                {
                                    if (map.ContainsKey(entry.Key) &&
                                          !reverseMap.ContainsKey(entry.Value))

                                        reverseMap.Add(entry.Value, entry.Key);
                                }
                            }
                            break;
                        }

                    case MapStrategy.makeBalanced: //  1 <-> 2  

                        {
                            lock (Locker)
                            {
                                foreach (KeyValuePair<K, V> entry in map)
                                {
                                    if (map.ContainsKey(entry.Key) &&
                                         !reverseMap.ContainsKey(entry.Value))

                                        reverseMap.Add(entry.Value, entry.Key);
                                }
                                foreach (KeyValuePair<V, K> entry in reverseMap)
                                {
                                    if (reverseMap.ContainsKey(entry.Key) &&
                                          !map.ContainsKey(entry.Value))

                                        map.Add(entry.Value, entry.Key);
                                }
                            }
                        }
                        break;

                    case MapStrategy.removeUnbalanced:// 1 >-< 2  
                        {
                            lock (Locker)
                            {
                                foreach (KeyValuePair<K, V> entry in map)
                                {
                                    if (map.ContainsKey(entry.Key) &&
                                         !reverseMap.ContainsKey(entry.Value))
                                        reverseMap.Remove(entry.Value);
                                }
                                foreach (KeyValuePair<V, K> entry in reverseMap)
                                {
                                    if (reverseMap.ContainsKey(entry.Key) &&
                                          !map.ContainsKey(entry.Value))

                                        map.Remove(entry.Value);
                                }

                            }
                        }
                        break;
                    default:
                        break;
                }

            }
            if (GetConsistencyStatus())
                accessed = false;

        }

    }
}