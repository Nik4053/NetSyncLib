using NetSyncLib.Helper;
using NetSyncLib.NetLibInterfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSyncLib
{
    public class DelayedReaderHandler
    {
        private static ConcurrentQueue<byte[]> puffer = new ConcurrentQueue<byte[]>();
        
        /// <summary>
        /// Blocking read. Blocks until there is something to read. Needs to be called once for each entry and will block if none are found.
        /// </summary>
        /// <param name="millisecond">The amount of time to wait after each unsuccessful call to read from puffer in milliseconds.</param>
        /// <returns>Data read. Only returns the first entry. </returns>
        public static byte[] Read(int millisecond=10)
        {
            byte[] data;
            while (!puffer.TryDequeue(out data))
            {
                System.Threading.Thread.Sleep(millisecond);
            }

            return data;
        }

        public static void ReaderHandler(byte[] data, NetSyncDeliveryMethod deliveryMethod, IEnumerable<IPeer> peers)
        {
            puffer.Enqueue(data);
        }

    }
}
