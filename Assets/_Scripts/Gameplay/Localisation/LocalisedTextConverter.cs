using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Scripts._Game.Localisation{

    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalisedTextConverter : MonoBehaviour
    {
        private TextMeshProUGUI _textMesh;

        [SerializeField]
        private string _localiseKey;

        // Start is called before the first frame update
        void OnEnable()
        {
            if (LocalisationSystem.Instance == null)
            {
                return;
            }

            _textMesh = GetComponent<TextMeshProUGUI>();
            if (_textMesh != null && _localiseKey == "")
            {
                _localiseKey = _textMesh.text;
            }
            string value = LocalisationSystem.Instance.GetLocalisedValue(_localiseKey);
            _textMesh.text = value;
        }
    }
    
}
