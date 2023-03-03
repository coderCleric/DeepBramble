using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    public class BlockableQuantumObject : SocketedQuantumObject
    {
        public BlockableQuantumSocket specialSocket = null;
        private bool specialSocketActive = false;

        /**
         * Need to do stuff to prevent a null ref in the standard awake
         */
        public new void Awake()
        {
            _checkIllumination = true;
            _alignWithGravity = false;
            _alignWithSocket = true;
            _socketRoot = transform.parent.gameObject;
            _sockets = new QuantumSocket[0];
            base.Awake();
        }

        /**
         * When we go to another socket, check if all others are blocked. If they are, add the special one
         */
        public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
        {
            //Manipulate the list
            if(specialSocket != null)
            {
                //Check if any standard sockets are unblocked
                bool allBlocked = true;
                foreach(QuantumSocket sock in _socketList)
                {
                    if((sock as BlockableQuantumSocket) != specialSocket && !sock.IsOccupied())
                    {
                        allBlocked = false;
                        break;
                    }
                }

                //Decide what to do based on blockage
                if(specialSocketActive && !allBlocked)
                {
                    specialSocketActive = false;
                    _socketList.Remove(specialSocket);
                }
                if(!specialSocketActive && allBlocked)
                {
                    specialSocketActive = true;
                    _socketList.Add(specialSocket);
                }
            }

            return base.ChangeQuantumState(skipInstantVisibilityCheck);
        }
    }
}
