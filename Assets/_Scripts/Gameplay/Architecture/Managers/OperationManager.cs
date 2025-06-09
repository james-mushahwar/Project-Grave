using _Scripts.Gameplay.General.Morgue.Operation.OperationState;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Animate.Player;
using _Scripts.Gameplay.General.Morgue.Bodies;
using _Scripts.Gameplay.Player.Controller;
using UnityEngine;
using _Scripts.Gameplay.General.Morgue.Operation.OperationSite;
using _Scripts.Gameplay.General.Morgue.Operation.Tools;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    //keeps track of all operation states active in the game. Keeps track of all operation tool profiles(?)

    public class OperationManager : GameManager<OperationManager>, IManager
    {
        private Dictionary<string, OperationState[]> _runtimeOperationStates; // id is body part, array of states per body part

        private List<OperationSite> _overviewOperationSites = new List<OperationSite>();
        public List<OperationSite> OverviewOperationSites
        {
            get
            {
                return _overviewOperationSites;
            }
        }
        public int _operationSitesIndex = 0;

        public OperationSite CurrentOperationSite
        {
            get
            {
                if (_operationSitesIndex < 0 || _operationSitesIndex >= _overviewOperationSites.Count)
                {
                    return null;
                }

                return _overviewOperationSites[_operationSitesIndex];
            }
        }

        private List<OperationState> _overviewOperationStates = new List<OperationState>();
        public List<OperationState> OverviewOperationStates
        {
            get
            {
                return _overviewOperationStates;
            }
        }
        private int _operationStatesIndex = 0;

        public OperationState CurrentOperationState
        {
            get
            {
                if (_operationStatesIndex < 0 || _operationStatesIndex >= _overviewOperationStates.Count)
                {
                    return null;
                }

                return _overviewOperationStates[_operationStatesIndex];
            }
        }

        [Header("VFX")] 
        [SerializeField] private ParticleSystem _enterPerfectZonePS;
        //[SerializeField] private ParticleSystem _exitPerfectZonePS;

        // as gamestate is being generated
        public virtual void ManagedPreInitialiseGameState() { }
        // after gamestate is generated
        public virtual void ManagedPostInitialiseGameState() { }
        // before main menu loads
        public virtual void ManagedPreMainMenuLoad() { }
        // after main menu loads
        public virtual void ManagedPostMainMenuLoad() { }
        // before world (level, area, zone) starts loading
        public virtual void ManagedPreInGameLoad() { }
        // after world (level, area, zone) finished loading
        public virtual void ManagedPostInGameLoad() { }
        // save states are restored
        public virtual void ManagedRestoreSave() { }
        // after save states are restored
        public virtual void ManagedPostRestoreSave() { }
        // before play begins 
        public virtual void ManagedPrePlayGame() { }
        // tick for playing game 
        public virtual void ManagedTick()
        {
            if (IsInAnyOperatingMode())
            {
                Debug.Log("OPeration site is " + CurrentOperationSite + " and OPeration state is: " + CurrentOperationState);
            }
        }
        // late update tick for playing game 
        public virtual void ManagedLateTick() { }
        // late update tick for playing game 
        public virtual void ManagedFixedTick() { }
        // before world (level, area, zone) starts unloading
        public virtual void ManagedPreTearddownGame() { }
        // after world (level, area, zone) unloading
        public virtual void ManagedPostTearddownGame() { }

        public bool RegisterOperationState(string operationID, OperationState operationState, int states)
        {
            if (_runtimeOperationStates.ContainsKey(operationID))
            {
                Debug.LogError("Key already added for this ID: " + operationID);
                return false;
            }

            OperationState[] stateArray = new OperationState[states];

            bool added = _runtimeOperationStates.TryAdd(operationID, stateArray);

            return added;
        }

        // Operation States
        // meaning has the operation been unlocked yet/ do we have the tool unlocked yet?
        public bool IsOperationAvailable(OperationState opState)
        {
            return true;
        }

        // meaning can the operation be performed in its current state, e.g.:
        // YES: Dismember body part because it's attached to body
        // NO:  Reattach arm not feasible since arm is attached to body
        // NO:  Stitching arm is not feasible since arm is fully attached to body
        // YES: Stitching arm is feasible because arm is not fully attached
        public bool IsOperationFeasible(OperationState opState)
        {
            return opState.IsFeasible();
        }

        // Operation modes
        public bool IsInOperationOverview(EPlayerControllerState controllerState, EOperationType opType)
        {
            return controllerState == EPlayerControllerState.Operating && opType == EOperationType.NONE;
        }
        public bool IsOperating(EPlayerControllerState controllerState, EOperationType opType)
        {
            return controllerState == EPlayerControllerState.Operating && opType != EOperationType.NONE;
        }
        public bool IsInAnyOperatingMode(EPlayerControllerState controllerState, EOperationType opType)
        {
            return IsInOperationOverview(controllerState, opType) || IsOperating(controllerState, opType);
        }

        public bool IsInOperationOverview(PlayerController pc = null)
        {
            if (pc == null)
            {
                pc = PlayerManager.Instance.CurrentPlayerController;
            }

            if (pc == null)
            {
                return false;
            }

            return IsInOperationOverview(pc.PlayerControllerState, pc.OperationType);
        }

        public bool IsOperating(PlayerController pc = null)
        {
            if (pc == null)
            {
                pc = PlayerManager.Instance.CurrentPlayerController;
            }

            if (pc == null)
            {
                return false;
            }

            return IsOperating(pc.PlayerControllerState, pc.OperationType);
        }

        public bool IsInAnyOperatingMode(PlayerController pc = null)
        {
            if (pc == null)
            {
                pc = PlayerManager.Instance.CurrentPlayerController;
            }

            if (pc == null)
            {
                return false;
            }

            return IsInAnyOperatingMode(pc.PlayerControllerState, pc.OperationType);
        }

        //Operation views
        public void OnStartBodyPartOperationOverview(BodyPartMorgueActor bodyPart)
        {
            if (bodyPart == null)
            {
                return;
            }

            List<OperationSite> bodyPartOpSites = new List<OperationSite>();

            bodyPartOpSites = bodyPart.OperationSites;

            if (bodyPartOpSites == null)
            {
                return;
            }

            _operationSitesIndex = 0;

            _overviewOperationSites.Clear();

            for (int i = bodyPartOpSites.Count - 1; i >= 0; i--)
            {
                OperationSite opSite = bodyPartOpSites[i];
                _overviewOperationSites.Add(opSite);

                if (opSite == CurrentOperationSite)
                { 
                    SortOperationStates(opSite);
                }
            }

        }

        public void SortOperationStates()
        {
            OperationSite opSite = CurrentOperationSite;

            if (opSite == null)
            {
                return;
            }

            SortOperationStates(opSite);
        }
        public void SortOperationStates(OperationSite opSite)
        {
            _operationStatesIndex = 0;
            _overviewOperationStates.Clear();
            //tbd

            List<OperationState> opStates = opSite.GetOperationStates();
            for (int j = 0; j < opStates.Count; j++)
            {
                OperationState opState = opStates[j];

                bool ignoreState = IsOperationAvailable(opState) == false;

                if (ignoreState)
                {
                    continue;
                }

                bool isFeasibleOperation = IsOperationFeasible(opState);

                if (!isFeasibleOperation)
                {
                    continue;
                }

                _overviewOperationStates.Add(opState);
            }
        }

        public void ScrollOperationSite(bool forward)
        {
            _operationSitesIndex += (forward ? 1 : -1);

            if (_operationSitesIndex >= _overviewOperationSites.Count)
            {
                _operationSitesIndex = 0;
            }
            else if (_operationSitesIndex < 0)
            {
                _operationSitesIndex = _overviewOperationSites.Count - 1;
            }

            _operationStatesIndex = 0;
            SortOperationStates();
        }

        public void ScrollOperationState(bool forward)
        {
            _operationStatesIndex += (forward ? 1 : -1);

            if (_operationStatesIndex >= _overviewOperationStates.Count)
            {
                _operationStatesIndex = 0;
            }
            else if (_operationStatesIndex < 0)
            {
                _operationStatesIndex = _overviewOperationStates.Count - 1;
            }
        }

        public EOperationType GetToolOperationType(MorgueToolActor opTool)
        {
            EOperationType opType = EOperationType.NONE;

            if (opTool as OperationAttachmentMorgueTool)
            {
                opType = EOperationType.Attaching;
            }
            else if (opTool as OperationCuttingMorgueTool)
            {
                opType = EOperationType.Dismember;
            }
            else if (opTool as OperationDismemberMorgueTool)
            {
                opType = EOperationType.Dismember;
            }

            return opType;
        }

        public void TriggerPerfectZone(bool on)
        {
            if (on)
            {
                PlayerController pc = PlayerManager.Instance.CurrentPlayerController;
                PlayerCharacterAnimator animator = pc.PlayerCharacterAnimator;

                _enterPerfectZonePS.transform.SetParent(pc.EquippedOperatingTool.transform, false);
                _enterPerfectZonePS.Play();
            }
            else
            {
                _enterPerfectZonePS.transform.SetParent(transform, false);
                _enterPerfectZonePS.Stop();
            }
        }
    }

}
