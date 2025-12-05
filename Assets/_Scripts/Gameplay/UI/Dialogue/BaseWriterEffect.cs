using _Scripts._Game.Dialogue;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Scripts._Game.UI.Dialogue{
    
    public abstract class BaseWriterEffect : MonoBehaviour
    {
        //public abstract IEnumerator Run(string textToType, TMP_Text textLabel);
        public abstract IEnumerator TypeText(string textToType, TMP_Text textLabel);

        // pass phrases in
        //public abstract IEnumerator Run(Phrase phrase, TMP_Text textLabel);
        public abstract IEnumerator TypeText(Phrase phrase, TMP_Text textLabel);
    }
    
}
