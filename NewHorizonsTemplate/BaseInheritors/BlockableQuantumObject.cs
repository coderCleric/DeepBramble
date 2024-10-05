using System.Numerics;

namespace DeepBramble.BaseInheritors
{
    public class BlockableQuantumObject : SocketedQuantumObject
    {
        public BlockableQuantumSocket specialSocket = null;
        private bool specialSocketActive = false;
        private AudioSignal mainSignal = null;
        private AudioSignal[] signals = null;
        private AudioSignal[] altSignals = null;

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
                    BlockableQuantumSocket sockAsBlockable = sock as BlockableQuantumSocket;
                    if(sockAsBlockable != specialSocket && (!sock.IsOccupied() || (sockAsBlockable.OccupiedByScoutOnly() && IsPlayerEntangled())))
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
                if (allBlocked)
                    specialSocket.gameObject.GetComponent<VisibilityObject>()._isIlluminated = specialSocket.gameObject.GetComponent<VisibilityObject>().CheckIllumination();
            }

            //Run the original method
            bool ret = base.ChangeQuantumState(skipInstantVisibilityCheck);

            //Depending on the return value, may need to handle the outer warp volumes for the player
            FogWarpDetector playerDetector = Locator.GetPlayerDetector().GetComponent<FogWarpDetector>();
            BlockableQuantumSocket targetSock = GetCurrentSocket() as BlockableQuantumSocket;
            if (ret && IsPlayerEntangled() && playerDetector.GetOuterFogWarpVolume() != null && targetSock != null && targetSock.outerFogWarp != null)
            {
                Patches.fogRepositionHandled = true;
                playerDetector.GetOuterFogWarpVolume().WarpDetector(playerDetector, targetSock.outerFogWarp);
            }

            //Parent to the new socket
            if (ret)
                transform.parent = targetSock.transform;

            //Depending on the identity of the new socket, disable or enable the signals
            if(targetSock == specialSocket)
            {
                foreach (AudioSignal s in signals)
                    s.SetSignalActivation(false, 0);
                foreach (AudioSignal s in altSignals)
                    s.SetSignalActivation(true, 0);
            }
            else
            {
                foreach (AudioSignal s in signals)
                    s.SetSignalActivation(true, 0);
                foreach (AudioSignal s in altSignals)
                    s.SetSignalActivation(false, 0);
            }

            //Set the outer warp of the main signal
            mainSignal._outerFogWarpVolume = targetSock.outerFogWarp;

            return ret;
        }

        /**
         * Register a new signal with this one
         */
        public void RegisterSignal(AudioSignal signal)
        {
            mainSignal = signal;
            string altName = signal.name + "_alt";

            //Reparent
            signal.transform.SetParent(transform, false);

            //Get a list of all of the signals propagated from this one
            AudioSignal[] allSignals = FindObjectsOfType<AudioSignal>();
            int count = 0;
            int altCount = 0;
            foreach(AudioSignal s in allSignals)
            {
                if(s.name.Equals(signal.name) && s != signal)
                    count++;

                if(s.name.Equals(altName))
                    altCount++;
            }
            int i = 0;
            int altI = 0;
            signals = new AudioSignal[count];
            altSignals = new AudioSignal[altCount];
            foreach (AudioSignal s in allSignals)
            {
                if (s.name.Equals(signal.name) && s != signal)
                {
                    signals[i] = s;
                    i++;
                }

                if (s.name.Equals(altName))
                {
                    altSignals[altI] = s;
                    altI++;
                }
            }
        }
    }
}
