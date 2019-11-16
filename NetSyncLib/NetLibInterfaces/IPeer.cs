using NetSyncLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSyncLib.NetLibInterfaces
{
    public interface IPeer
    {
        //string EndPoint { get; }
        ushort Id { get; }
        string Name { get; }
        void Send<T>(T value, NetSyncDeliveryMethod deliveryMethod);
        void Recieve();
    }
}
