using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Gameplay.General.Morgue.Bodies{

    public enum EMorgueBodyVariant
    {
        None = -1,
        Body1 = 0,
        Body2 = 1,
        COUNT
    }

    [Serializable]
    public abstract class MorgueBodyVariant
    {
        
    }

    [Serializable]
    public class HumanMorgueBodyVariant : MorgueBodyVariant
    {
        [SerializeField] private FMeshPair _headMeshes;
        [SerializeField] private FMeshPair _torsoMeshes;
        [SerializeField] private FMeshPair _lArmMeshes;
        [SerializeField] private FMeshPair _rArmMeshes;
        [SerializeField] private FMeshPair _lLegMeshes;
        [SerializeField] private FMeshPair _rLegMeshes;

        public FMeshPair GetHeadMeshes()
        {
            return _headMeshes;
        }

        public FMeshPair GetTorsoMeshes() {  return _torsoMeshes; }
    }

    [Serializable]
    public class FMeshPair
    {
        [SerializeField]
        private Mesh _staticMesh;
        [SerializeField]
        private List<Material> _staticMeshMaterials;
        [SerializeField]
        private Mesh _skinnedMesh;
        [SerializeField]
        private List<Material> _skinnedMeshMaterials;

        public Mesh StaticMesh { get => _staticMesh; }
        public Mesh SkinnedMesh { get => _skinnedMesh; }
        public List<Material> StaticMeshMaterials { get => _staticMeshMaterials; }
        public List<Material> SkinnedMeshMaterials { get => _skinnedMeshMaterials; }
    }

    [CreateAssetMenu(menuName = "Morgue/Bodies/MorgueBodyAtlas", fileName = "MorgueBodyAtlasSO")]
    public class MorgueBodyAtlas : ScriptableObject
    {
        [SerializeField]
        private HumanMorgueBodyVariantDictionary _humanBodyVariantDictionary;

        public HumanMorgueBodyVariant GetHumanBodyVariant(EMorgueBodyVariant bodyVariant = EMorgueBodyVariant.None)
        {
            if (bodyVariant == EMorgueBodyVariant.None || bodyVariant == EMorgueBodyVariant.COUNT)
            {
                bodyVariant = (EMorgueBodyVariant)Random.Range(0, (int)EMorgueBodyVariant.COUNT);
            }

            if (_humanBodyVariantDictionary.ContainsKey(bodyVariant))
            {
                return _humanBodyVariantDictionary[bodyVariant];
            }

            return null;
        }
    }
    
}
