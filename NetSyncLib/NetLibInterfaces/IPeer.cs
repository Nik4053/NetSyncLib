using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSyncLib.NetLibInterfaces
{
    interface IPeer
    {
        string Name { get; }
        void Send<T>(T value);
        void Recieve();
    }
}
