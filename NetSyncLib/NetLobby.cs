using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using NetSyncLib.Helper;
using NetSyncLib.Impl;
using NetSyncLib.NetLibInterfaces;

namespace NetSyncLib
{
    public class NetLobby : SimpleNetController
    {
        public const int EmptyPeerId = -2;

        public bool AllowJoining = true;

        public bool LockPlayers;

        public NetLobbyTeam[] Teams = new NetLobbyTeam[0];

        /// <summary>
        /// all peers joined will be placed in this team.
        /// </summary>
        public NetLobbyTeam Spectators = new NetLobbyTeam(null, -3);

        public NetLobby(int spectatorSize) : base(-3)
        {
            this.Spectators = new NetLobbyTeam("Spectators", spectatorSize);
            Console.WriteLine("Lobby created");
        }

        public NetLobby() : this(10)
        {
        }

        public void AddTeam(string teamName, int size)
        {
            NetLobbyTeam[] newTeams = new NetLobbyTeam[this.Teams.Length + 1];
            Array.Copy(this.Teams, newTeams, this.Teams.Length);
            newTeams[newTeams.Length - 1] = new NetLobbyTeam(teamName, size);
            this.Teams = newTeams;
            return;
        }

        public void RemoveTeam(int teamId)
        {
            for (int i = 0; i < this.Teams.Length; i++)
            {
                if (this.Teams[i].TeamId != teamId)
                {
                    continue;
                }

                this.Teams[i].ChangeTeamSize(0);
                this.Teams[i] = null;
                NetLobbyTeam[] newTeams = new NetLobbyTeam[this.Teams.Length - 1];
                Array.Copy(this.Teams, newTeams, i);
                for (int j = i; j < newTeams.Length; j++)
                {
                    newTeams[j] = this.Teams[j + 1];
                }

                this.Teams = newTeams;
                return;
            }
        }

        public void RemoveAllTeams()
        {
            throw new NotImplementedException();
        }

        public bool JoinLobby(int peerId)
        {
            if (!NetOrganisator.IsServer())
            {
                return false;
            }

            if (!this.AllowJoining) return false;
            if (this.CheckIfAlreadyExists(peerId)) return false;
            return this.Spectators.Join(peerId) != -1;
        }

        public bool JoinLobby(IPeer peer)
        {
            return this.JoinLobby(peer.Id);
        }

        public void ExitLobby(IPeer peer)
        {
            this.ExitLobby(peer.Id);
        }

        public void ExitLobby(int peerId)
        {
            if (!NetOrganisator.IsServer())
            {
                return;
            }

            foreach (NetLobbyTeam team in this.Teams)
            {
                team.RemovePeer(peerId);
            }

            this.Spectators.RemovePeer(peerId);
        }

        private bool CheckIfAlreadyExists(int peer)
        {
            HashSet<int> peers = new HashSet<int>();
            foreach (NetLobbyTeam team in this.Teams)
            {
                foreach (int p in team.Members)
                {
                    if (p == EmptyPeerId) continue;
                    if (!peers.Add(p))
                        throw new ApplicationException("A peer is already part of two teams in a lobby.");
                }
            }

            foreach (int p in this.Spectators.Members)
            {
                if (p == EmptyPeerId) continue;
                if (!peers.Add(p)) throw new ApplicationException("A peer is already part of two teams in a lobby.");
            }

            return peers.Contains(peer);
        }

        public bool SwitchTeam(IPeer peer, NetLobbyTeam newTeam, int newPosition = -1)
        {
            return this.SwitchTeam(peer.Id, newTeam, newPosition);
        }

        public bool SwitchTeam(int peerId, NetLobbyTeam newTeam, int newPosition = -1)
        {
            if (!NetOrganisator.IsServer())
            {
                if (NetOrganisator.NetPeerId == peerId)
                {
                    DataWriter writer = new DataWriter();
                    writer.Put(newTeam.TeamId);
                    writer.Put(newPosition);
                    this.TrySendNetControllerUpdate(writer);
                    return true;
                }

                return false;
            }

            if (this.LockPlayers) return false;

            NetLobbyTeam oldTeam = null;
            int oldPos = -1;

            oldPos = this.Spectators.Contains(peerId);
            if (oldPos != -1)
            {
                oldTeam = this.Spectators;
            }

            if (oldTeam == null)
            {
                foreach (NetLobbyTeam team in this.Teams)
                {
                    oldPos = team.Contains(peerId);
                    if (oldPos != -1)
                    {
                        oldTeam = team;
                        break;
                    }
                }
            }

            if (oldPos == newPosition && oldTeam == newTeam)
            {
                return true;
            }

            if (newTeam.Join(peerId, newPosition) != -1)
            {
                oldTeam?.RemoveAtPosition(oldPos);
                this.NetServerSendUpdate();
                return true;
            }

            if (oldTeam == null)
            {
                this.Spectators.Join(peerId);
                this.NetServerSendUpdate();
            }

            return false;
        }

        public void ChangeSizeOfTeam(NetLobbyTeam team, int newSize)
        {
            if (!NetOrganisator.IsServer())
            {
                return;
            }

            if (this.LockPlayers) return;
            List<int> peers = team.ChangeTeamSize(newSize);
            foreach (int peer in peers)
            {
                if (this.Spectators.Join(peer) == -1)
                {
                    this.Spectators.ChangeTeamSize(this.Spectators.MaxSize + 1);
                    this.Spectators.Join(peer);
                }
            }

            this.NetServerSendUpdate();
        }

        public override void Serialize(DataWriter writer)
        {
            writer.Put(this.Spectators.TeamId);
            this.Spectators.Serialize(writer);
            foreach (NetLobbyTeam team in this.Teams)
            {
                writer.Put(true);
                writer.Put(team.TeamId);
                team.Serialize(writer);
            }

            writer.Put(false);
        }

        public override void Deserialize(DataReader reader)
        {
            this.Spectators = new NetLobbyTeam(reader.GetInt(), reader);
            List<NetLobbyTeam> teams = new List<NetLobbyTeam>();
            while (reader.GetBool())
            {
                NetLobbyTeam team = new NetLobbyTeam(reader.GetInt(), reader);
                teams.Add(team);
            }

            this.Teams = teams.ToArray();
        }

        private bool resendAll = true;

        // private List<(int, int)> teamMemberUpdates;
        // private List<(int, string)> teamNameUpdates;
        public override void NetServerSendUpdate(IEnumerable<IPeer> sendTo = null)
        {
            this.PrintLobby();
            DataWriter writer = new DataWriter();
            writer.Put(this.resendAll);
            if (this.resendAll)
            {
                this.Serialize(writer);
            }
            else
            {
                throw new NotImplementedException();
            }

            this.TrySendNetUpdate(writer, NetSyncDeliveryMethod.ReliableOrdered, sendTo);

            // this.teamMemberUpdates.Clear();
            // this.teamNameUpdates.Clear();
        }

        public override void NetClientReceiveUpdate(DataReader reader)
        {
            if (reader.GetBool())
            {
                this.Deserialize(reader);
                this.PrintLobby();
                return;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void PrintLobby()
        {
            Console.WriteLine("Lobby update-------------------------------------------");
            this.PrintTeam(this.Spectators);
            foreach (NetLobbyTeam team in this.Teams)
            {
                this.PrintTeam(team);
            }

            Console.WriteLine();
        }

        private void PrintTeam(NetLobbyTeam team)
        {
            Console.WriteLine("\n" + team.Name + $", Id: {team.TeamId}: ");
            foreach (int member in team.Members)
            {
                Console.Write(member + "|");
            }
        }

        private string GetLobbyStatus()
        {
            string st = "Lobby:";
            st += this.GetTeamStatus(this.Spectators);
            foreach (NetLobbyTeam team in this.Teams)
            {
                st += this.GetTeamStatus(team);
            }

            return st;
        }

        private string GetTeamStatus(NetLobbyTeam team)
        {
            string st = "\n" + team.Name + $", Id: {team.TeamId}: ";
            foreach (int member in team.Members)
            {
                st += member + "|";
            }

            return st;
        }

        protected override void OnReceiveStatus(DataReader reader, IPeer sender)
        {
            this.SwitchTeam(sender, this.GetTeamWithId(reader.GetInt()), reader.GetInt());
        }

        public NetLobbyTeam GetTeamWithId(int id)
        {
            if (this.Spectators.TeamId == id) return this.Spectators;
            foreach (NetLobbyTeam team in this.Teams)
            {
                if (team.TeamId == id) return team;
            }

            return null;
        }

        protected override void OnSendStatus()
        {
        }

        public override string ToString()
        {
            return base.ToString() + this.GetLobbyStatus();
        }

        public class NetLobbyTeam : INetSerializable
        {
            private static int idGen = 0;
            public readonly int TeamId = idGen++;
            public string Name = "UnknownTeam";
            public int[] Members;

            internal NetLobbyTeam(string name, int size)
            {
                this.Name = name;
                this.Members = new int[0];
                this.ChangeTeamSize(size);
            }

            internal NetLobbyTeam(int teamId, DataReader deserializer)
            {
                this.TeamId = teamId;
                this.Deserialize(deserializer);
            }

            /// <summary>
            /// Gets the max team size. size smaller than 0 will be set to 0.
            /// </summary>
            public int MaxSize => this.Members.Length;

            /// <summary>
            /// Joins this team. If position is set will join team at given position. If not will join at next free position. Returns false if join failed.
            /// </summary>
            /// <param name="peerId"></param>
            /// <param name="position"></param>
            /// <returns>Returns-1 if join failed.</returns>
            internal int Join(int peerId, int position = -1)
            {
                if (position != -1)
                {
                    if (this.Members[position] == EmptyPeerId)
                    {
                        this.Members[position] = peerId;
                        return position;
                    }

                    return -1;
                }

                for (int i = 0; i < this.MaxSize; i++)
                {
                    if (this.Members[i] == EmptyPeerId)
                    {
                        this.Members[i] = peerId;
                        return i;
                    }
                }

                return -1;
            }

            internal bool RemovePeer(int peerId)
            {
                for (int i = 0; i < this.MaxSize; i++)
                {
                    if (this.Members[i] == peerId)
                    {
                        this.RemoveAtPosition(i);
                        return true;
                    }
                }

                return false;
            }

            internal int RemoveAtPosition(int position)
            {
                int peer = this.Members[position];
                this.Members[position] = EmptyPeerId;
                return peer;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="newSize"></param>
            /// <returns>Peers that have to be removed because of space.</returns>
            internal List<int> ChangeTeamSize(int newSize)
            {
                if (newSize < 0) newSize = 0;
                int[] newTeam = new int[newSize].Populate(EmptyPeerId);
                List<int> toRemove = new List<int>();
                for (int i = 0; i < this.Members.Length; i++)
                {
                    if (i >= newTeam.Length)
                    {
                        int peer = this.Members[i];
                        if (peer != EmptyPeerId)
                            toRemove.Add(peer);

                        continue;
                    }

                    newTeam[i] = this.Members[i];
                }

                this.Members = newTeam;
                return toRemove;
            }

            public void Serialize(DataWriter writer)
            {
                writer.Put(this.Name);
                writer.PutArray(this.Members);
            }

            public void Deserialize(DataReader reader)
            {
                this.Name = reader.GetString();
                this.Members = reader.GetIntArray();
            }

            /// <summary>
            /// Returns the position of the peer with the given id. returns -1 if it wasn't found.
            /// </summary>
            /// <param name="peerId"></param>
            /// <returns></returns>
            public int Contains(int peerId)
            {
                for (int i = 0; i < this.Members.Length; i++)
                {
                    if (this.Members[i] == peerId) return i;
                }

                return -1;
            }
        }
    }
}