using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Game.Dialogue{
    
    [CreateAssetMenu(menuName ="Dialogue/Conversation")]
    public class ScriptableConversation : ScriptableObject
    {
        [SerializeField] private ScriptableDialogue[] _dialogue;
    
    }
    
}
