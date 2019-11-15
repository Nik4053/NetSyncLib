using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using NetSyncLib.Impl;

namespace NetSyncLib.Tests
{
    public class EmptyNetObject : NetObject
    {
        public EmptyNetObject()
        {
        }

        ~EmptyNetObject()
        {
            //ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}
