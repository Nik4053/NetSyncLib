using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSyncLib.Extras
{
    class RoundBased
    {
        private int _roundNumber;
        //TODO
        public int RoundNumber { get => _roundNumber; private set { } }

        public delegate void OnNewRound(int roundNumber);
        public event OnNewRound NewRoundEvent;


        public void TryStartNewRound()
        {

        }

        public void StartNextRound()
        {
            RoundNumber++;

        }

    }
}
