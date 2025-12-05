using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts._Game.Sequencer;
using System;
using _Scripts._Game.Sequencer.Triggers;
using _Scripts.Gameplay.Architecture.Managers;

namespace _Scripts._Game.Sequencer{
    
    [Serializable]  
    public class SequenceSettings
    {
        //Player
        public bool _freezePlayer = false;
        public bool _disableDispossessEnemy = false;

        //General
        public bool _canAlwaysRun = false;
        public bool _canLoop = false;
    }

    public class SequencerManager : GameManager<SequencerManager>, IManager
    {
        private List<Sequenceable> _activeSequences = new List<Sequenceable>();
        private int _freezePlayerStack;
        private int _disableDispossesActionStack;
        private Dictionary<string, SequenceSettings> _sequenceSettings = new Dictionary<string, SequenceSettings>();

        public bool IsSequenceActive
        {
            get => _activeSequences.Count > 0;
        }

        public bool FreezeMovement
        {
            get { return _freezePlayerStack > 0; }
        }
        public bool DisableDispossessAction
        {
            get { return _disableDispossesActionStack > 0; }
        }

        // Start is called before the first frame update
        public void ManagedPreInGameLoad()
        {
            _freezePlayerStack = 0;
            _activeSequences.Clear();
            _sequenceSettings.Clear();
        }

        public void ManagedPostInGameLoad()
        {
            
        }

        public void ManagedPreMainMenuLoad()
        {
            if (_freezePlayerStack != 0)
            {
                //unfreeze player 
                _freezePlayerStack = 0;
            }

            if (_disableDispossesActionStack != 0)
            {
                _disableDispossesActionStack = 0;
            }
            _activeSequences.Clear();
            _sequenceSettings.Clear();
        }

        public void ManagedPostMainMenuLoad()
        {
            
        }

        public void ManagedTick()
        {
            List<Sequenceable> activeSequencesCopy = new List<Sequenceable>();
            activeSequencesCopy.AddRange(_activeSequences);

            foreach (var sequence in activeSequencesCopy)
            {
                if (sequence.IsStarted() == false)
                {
                    sequence.Begin();
                }

                sequence.Tick();
            }

            for (int i = _activeSequences.Count - 1; i >= 0; i--)
            {
                Sequenceable sequence = _activeSequences[i];

                bool found = _sequenceSettings.TryGetValue(sequence.RuntimeID, out SequenceSettings settings);


                if (sequence.IsComplete() || sequence == null)
                {
                    sequence?.Stop();

                    if (settings._canLoop && sequence != null)
                    {
                        sequence?.Begin();
                        continue;
                    }

                    // resolve settings
                    if (found)
                    {
                        if (settings._freezePlayer)
                        {
                            _freezePlayerStack--;
                            if (_freezePlayerStack == 0)
                            {
                                // unfreeze player 
                            }
                        }

                        if (settings._disableDispossessEnemy)
                        {
                            _disableDispossesActionStack--;
                        }
                    }
                    _sequenceSettings.Remove(sequence.RuntimeID);
                    _activeSequences.RemoveAt(i);
                }
            }
        }

        public bool TryRegisterSequence(Sequenceable seq, SequenceSettings seqSettings)
        {
            bool register = false;
            string runtimeID = seq.RuntimeID;

            if (!_sequenceSettings.ContainsKey(runtimeID))
            {
                _activeSequences.Add(seq);

                if (seqSettings._freezePlayer)
                {
                    _freezePlayerStack++;
                    if (_freezePlayerStack == 1)
                    {
                        //freeze player
                    }
                }

                if (seqSettings._disableDispossessEnemy)
                {
                    _disableDispossesActionStack++;
                }

                _sequenceSettings.Add(runtimeID, seqSettings);

                register = true;
            }

            return register;
        }

        public bool TryUnregisterSequence(Sequenceable seq, SequenceSettings seqSettings)
        {
            bool unregister = false;
            string runtimeID = seq.RuntimeID;

            if (_sequenceSettings.ContainsKey(runtimeID))
            {
                if (seqSettings == null)
                {
                    _sequenceSettings.TryGetValue(runtimeID, out seqSettings);
                }

                _activeSequences.Remove(seq);

                if (seqSettings._freezePlayer)
                {
                    _freezePlayerStack--;
                    if (_freezePlayerStack == 0)
                    {
                        //unfreeze player
                    }
                }
                if (seqSettings._disableDispossessEnemy)
                {
                    _disableDispossesActionStack--;
                }

                _sequenceSettings.Remove(runtimeID);

                unregister = true;
            }

            return unregister;
        }
    }
}