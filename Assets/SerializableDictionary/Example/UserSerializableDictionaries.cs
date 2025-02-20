using System.Collections;
using System.Collections.Generic;
using System;
using _Scripts.Gameplay.Architecture.Managers;
using Cinemachine;
using UnityEngine;
using _Scripts.Gameplay.General.Morgue.Bodies;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> {}

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> {}

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> {}

//Project Grave Dictionaries 
[Serializable]
public class MorgueAnimTypeAnimationDictionary : SerializableDictionary<EMorgueAnimType, Animation> {}

[Serializable]
public class VirtualCameraTypeDictionary : SerializableDictionary<EVirtualCameraType, CinemachineVirtualCamera> { }

[Serializable]
public class HumanMorgueBodyVariantDictionary : SerializableDictionary<EMorgueBodyVariant, HumanMorgueBodyVariant> { }