using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.General.Morgue.Operation.Tools.Profiles{

    [CreateAssetMenu(menuName = "Tool/ToolProfile/OperationToolProfile", fileName = "OperationToolProfileSO")]
    public class OperationToolProfileScriptableObject : ToolProfileScriptableObject
    {
        [Header("NEW")]
        [SerializeField]
        private FMomentumZone _momentumZone;
        [SerializeField]
        private float speedFactor;
        [SerializeField]
        private Vector2 _minMaxForwardSpeed;
        [SerializeField]
        private AnimationCurve _momentumToSpeedCurve;
        [SerializeField]
        private Vector2 _minMaxBackwardSpeed;
        [SerializeField]
        private Vector2 _minMaxAnimationDisplacement;

        public FMomentumZone MomentumZone { get => _momentumZone; set => _momentumZone = value; }
        public float SpeedFactor { get => speedFactor; set => speedFactor = value; }
        public Vector2 MinMaxForwardSpeed { get => _minMaxForwardSpeed; set => _minMaxForwardSpeed = value; }
        public AnimationCurve MomentumToSpeedCurve { get => _momentumToSpeedCurve; set => _momentumToSpeedCurve = value; }
        public Vector2 MinMaxBackwardSpeed { get => _minMaxBackwardSpeed; set => _minMaxBackwardSpeed = value; }
        public Vector2 MinMaxAnimationDisplacement { get => _minMaxAnimationDisplacement; set => _minMaxAnimationDisplacement = value; }
    }

    internal interface IMomentumTool
    {
        public float GetMomentumZone(int index);
        public int GetBuildMomentumCount();
    }

    internal interface ISpeedTool
    {
        public float GetSpeedFactor();
    }
}
