using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSyncLib.Helper;
using NetSyncLib.Impl;

namespace NetSyncLib.Tests
{
    public class EmptyNetObject : NetObject
    {
        public EmptyNetObject()
        {
        }
        [NetSynchronize]
        internal int testInt = 12345;
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
