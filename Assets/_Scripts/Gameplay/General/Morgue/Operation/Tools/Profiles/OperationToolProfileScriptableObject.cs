using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles{

    [CreateAssetMenu(menuName = "Tool/ToolProfile/OperationToolProfile", fileName = "OperationToolProfileSO")]
    public class OperationToolProfileScriptableObject : ToolProfileScriptableObject
    {
        [Header("NEW")]
        [SerializeField]
        private List<FMomentumZone> _momentumZones;
        [SerializeField]
        private float _buildingMomentumSpeedFactor;
        [SerializeField]
        private float _freeFlowSpeedFactor;
        [SerializeField]
        private Vector2 _minMaxForwardSpeed;
        [SerializeField]
        private AnimationCurve _momentumToSpeedCurve;
        [SerializeField]
        private Vector2 _minMaxBackwardSpeed;
        [SerializeField]
        private Vector2 _minMaxAnimationDisplacement;

        public List<FMomentumZone> MomentumZones { get => _momentumZones; set => _momentumZones = value; }
        public float BuildingMomentumSpeedFactor { get => _buildingMomentumSpeedFactor; set => _buildingMomentumSpeedFactor = value; }
        public float FreeFlowSpeedFactor { get => _freeFlowSpeedFactor; set => _freeFlowSpeedFactor = value; }
        public Vector2 MinMaxForwardSpeed { get => _minMaxForwardSpeed; set => _minMaxForwardSpeed = value; }
        public AnimationCurve MomentumToSpeedCurve { get => _momentumToSpeedCurve; set => _momentumToSpeedCurve = value; }
        public Vector2 MinMaxBackwardSpeed { get => _minMaxBackwardSpeed; set => _minMaxBackwardSpeed = value; }
        public Vector2 MinMaxAnimationDisplacement { get => _minMaxAnimationDisplacement; set => _minMaxAnimationDisplacement = value; }
    }

    internal interface IMomentumTool
    {
        public float GetMomentumZoneTiming(int index);
        public float GetMomentumZoneSpeedFactor(int index);
        public int GetBuildMomentumCount();

    }

    internal interface ISpeedTool
    {
        public float GetSpeedFactor(IToolUser toolUSer);
        public float GetBuildingMomentumSpeed();
        public float GetFreeFlowSpeed();
    }

    public interface IToolUser
    {
        public EOperationMinigameState GetToolUserState();
        public int GetBuildMomentumCounts();
    }
}
