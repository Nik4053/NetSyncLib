using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSyncLib
{
    public class DelayedReaderHandler
    {
        private static ConcurrentQueue<byte[]> puffer =  new ConcurrentQueue<byte[]>();
        public static byte[] Read()
        {
            byte[] data;
            while (!puffer.TryDequeue(out data))
            {
                System.Threading.Thread.Sleep(10);
            }

            return data;
        }

        public static void ReaderHandler(byte[] data)
        {
            puffer.Enqueue(data);
        }

    }
}
