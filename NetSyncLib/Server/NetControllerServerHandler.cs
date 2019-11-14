using System;
using System.Collections.Generic;

namespace NetSyncLib.Server
{
    public class NetControllerServerHandler
    {
        private readonly Dictionary<int, List<WeakReference<INetController>>> netControllers = new Dictionary<int, List<WeakReference<INetController>>>();

        public void AddController(INetController netController, int ownerId = 0)
        {
            if (!this.netControllers.ContainsKey(ownerId))
            {
                this.netControllers.Add(ownerId, new List<WeakReference<INetController>>());
            }

            this.netControllers[ownerId].Add(netController.WeakReference);
        }

        public void ChangeIdOfOwner(INetController netController, int newOwner)
        {
            this.netControllers[netController.OwnerId].Remove(netController.WeakReference);
            this.netControllers[newOwner].Add(netController.WeakReference);
        }

        public void RemoveController(INetController netController)
        {
            this.netControllers[netController.OwnerId].Remove(netController.WeakReference);
        }

        public void RemoveLostControllersOfOwner(int ownerId)
        {
            this.netControllers[ownerId].RemoveAll(reference => !reference.TryGetTarget(out _));
        }

        public INetController GetController(int ownerId, ushort netId)
        {
            if (ownerId == -3)
            {
                INetController controller = null;
                foreach (int key in this.netControllers.Keys)
                {
                    controller = this.GetControllerWithId(key, netId);
                    if (controller != null)
                    {
                        return controller;
                    }
                }

                return null;
            }

            return this.GetControllerWithId(ownerId, netId);
        }

        private INetController GetControllerWithId(int ownerId, ushort netId)
        {
            List<WeakReference<INetController>> references = this.netControllers[ownerId];
            for (int i = 0; i < references.Count; i++)
            {
                WeakReference<INetController> netReference = references[i];
                if (!netReference.TryGetTarget(out INetController target))
                {
                    references.RemoveAt(i);
                    i--;
                    continue;
                }

                if (NetOrganisator.ServerNetObjectHandler.TryGetId(target, out ushort id))
                {
                    if (id == netId)
                    {
                        return target;
                    }
                }
            }

            return null;
        }
    }
}