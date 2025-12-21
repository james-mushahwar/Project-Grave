using System.Collections;
using System.Collections.Generic;
using _Scripts._Game.General;
using UnityEngine;
using _Scripts.Org;

namespace _Scripts._Game.Localisation{

    public enum ELanguage
    {
        English,
        French
    }

    public class LocalisationSystem : Singleton<LocalisationSystem>
    {
        [SerializeField]
        private ELanguage _language;

        public ELanguage Language { get => _language; set => _language = value; }

        //langauge dictionaries
        private static Dictionary<string, string> _localisedEN;
        private static Dictionary<string, string> _localisedFR;

        private static bool _isInit;

        public void OnEnable()
        {
            if (_isInit)
            {
                return;
            }

            Initialise();
        }

        public void Initialise()
        {
            CSVLoader csvLoader = new CSVLoader();

            csvLoader.LoadCSV();

            _localisedEN = csvLoader.GetDictionaryValues("en");
            _localisedFR = csvLoader.GetDictionaryValues("fr");

            _isInit = true;
        }

        public string GetLocalisedValue(string key)
        {
            if (!_isInit)
            {
                Initialise();
            }

            string value = key;

            switch(_language)
            {
                case ELanguage.English:
                    _localisedEN.TryGetValue(key, out value);
                    break;
                case ELanguage.French:
                    _localisedFR.TryGetValue(key, out value);
                    break;
                default:
                    break;
            }

            return value;
        }
    }
    
}
