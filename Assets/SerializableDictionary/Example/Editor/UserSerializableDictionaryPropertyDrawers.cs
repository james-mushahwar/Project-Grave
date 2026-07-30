using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using _Scripts.Gameplay.Audio;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(ObjectColorDictionary))]
[CustomPropertyDrawer(typeof(StringColorArrayDictionary))]
[CustomPropertyDrawer(typeof(MorgueAnimTypeAnimationDictionary))]
[CustomPropertyDrawer(typeof(MorgueAnimTypeNameDictionary))]
[CustomPropertyDrawer(typeof(VirtualCameraTypeDictionary))]
[CustomPropertyDrawer(typeof(RuntimeIDVirtualCameraDictionary))]
[CustomPropertyDrawer(typeof(HumanMorgueBodyVariantDictionary))]
[CustomPropertyDrawer(typeof(AudioCueDictionary))]
[CustomPropertyDrawer(typeof(ParticleGroupDictionary))]
[CustomPropertyDrawer(typeof(RigIKTypeDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}

[CustomPropertyDrawer(typeof(ColorArrayStorage))]
public class AnySerializableDictionaryStoragePropertyDrawer: SerializableDictionaryStoragePropertyDrawer {}
