using _Scripts._Game.Dialogue;
using System.Collections.Generic;
using _Scripts._Game.UI.Dialogue;
using UnityEngine;
using TMPro;
using _Scripts._Game.Sequencer.Dialogue;
using System;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class DialogueManager : GameManager<DialogueManager>, IManager
    {
        private Dictionary<EDialogueType, GameObject> _textGameObjectDictionary = new Dictionary<EDialogueType, GameObject>();
        private Dictionary<EDialogueType, TMP_Text> _textBoxDictionary = new Dictionary<EDialogueType, TMP_Text>();
        private Dictionary<EDialogueType, bool> _dialoguePauseCheckDict = new Dictionary<EDialogueType, bool>(); // should each dialogue type pause the game when active?

        private BaseWriterEffect[] _writerEffects;

        private Dictionary<EDialogueEvent, List<TextSequenceable>> _dialoguesDictionary = new Dictionary<EDialogueEvent, List<TextSequenceable>>();
        private Dictionary<EDialogueEvent, List<TextSequenceable>> _dialogueSelectionPools = new Dictionary<EDialogueEvent, List<TextSequenceable>>();
        private Dictionary<EDialogueEvent, TextSequenceable> _lastPlayedDialogues = new Dictionary<EDialogueEvent, TextSequenceable>();

        private Dictionary<EDialogueType, Task> _dialogueTasks = new Dictionary<EDialogueType, Task>();

        private Dictionary<EDialogueEvent, bool> _dialoguePlayedDictionary = new Dictionary<EDialogueEvent, bool>();

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            GameObject speechGameObject = GameObject.FindGameObjectWithTag("UI_Speech");
            if (speechGameObject)
            {
                TMP_Text speechTextBox = speechGameObject.GetComponentInChildren<TMP_Text>();
                speechGameObject.SetActive(false);
                _dialoguePauseCheckDict.Add(EDialogueType.Speech, true);
                _textGameObjectDictionary.Add(EDialogueType.Speech, speechGameObject);
                if (speechTextBox)
                {
                    _textBoxDictionary.Add(EDialogueType.Speech, speechTextBox);
                }
            }

            GameObject overviewGameObject = GameObject.FindGameObjectWithTag("UI_Overview");
            if (overviewGameObject)
            {
                TMP_Text overviewTextBox = overviewGameObject.GetComponentInChildren<TMP_Text>();
                overviewGameObject.SetActive(false);
                _dialoguePauseCheckDict.Add(EDialogueType.Overview, false);
                _textGameObjectDictionary.Add(EDialogueType.Overview, overviewGameObject);
                if (overviewTextBox)
                {
                    _textBoxDictionary.Add(EDialogueType.Overview, overviewTextBox);
                }
            }

            GameObject characterOverviewGameObject = GameObject.FindGameObjectWithTag("UI_CharacterOverview");
            if (characterOverviewGameObject)
            {
                TMP_Text characterOverviewTextBox = characterOverviewGameObject.GetComponentInChildren<TMP_Text>();
                characterOverviewGameObject.SetActive(false);
                _dialoguePauseCheckDict.Add(EDialogueType.CharacterOverview, true);
                _textGameObjectDictionary.Add(EDialogueType.CharacterOverview, characterOverviewGameObject);
                if (characterOverviewTextBox)
                {
                    _textBoxDictionary.Add(EDialogueType.CharacterOverview, characterOverviewTextBox);
                }
            }

            GameObject characterWorldGameObject = GameObject.FindGameObjectWithTag("UI_CharacterWorld");
            if (characterWorldGameObject)
            {
                TMP_Text characterWorldTextBox = characterWorldGameObject.GetComponentInChildren<TMP_Text>();
                characterWorldGameObject.SetActive(false);
                _dialoguePauseCheckDict.Add(EDialogueType.CharacterWorld, false);
                _textGameObjectDictionary.Add(EDialogueType.CharacterWorld, characterWorldGameObject);
                if (characterWorldTextBox)
                {
                    _textBoxDictionary.Add(EDialogueType.CharacterWorld, characterWorldTextBox);
                }
            }

            GameObject worldGameObject = GameObject.FindGameObjectWithTag("UI_World");
            if (worldGameObject)
            {
                TMP_Text worldTextBox = worldGameObject.GetComponentInChildren<TMP_Text>();
                worldGameObject.SetActive(false);
                _dialoguePauseCheckDict.Add(EDialogueType.World, false);
                _textGameObjectDictionary.Add(EDialogueType.World, worldGameObject);
                if (worldTextBox)
                {
                    _textBoxDictionary.Add(EDialogueType.World, worldTextBox);
                }
            }

            // writer effects
            _writerEffects = GetComponents<BaseWriterEffect>();

            //dialogues
            TextSequenceable[] ts = gameObject.GetComponentsInChildren<TextSequenceable>();
            foreach(TextSequenceable seq in ts)
            {
                if (seq != null)
                {
                    if (seq.ScriptableDialogue == null)
                    {
                        Debug.LogError("Cannot register dialogue without an asset on " + seq.name, seq);
                        continue;
                    }

                    if (seq.ScriptableDialogue.DialogueEvent != EDialogueEvent.None)
                    {
                        if (_dialoguesDictionary.TryGetValue(seq.ScriptableDialogue.DialogueEvent, out List<TextSequenceable> dialogues) == false)
                        {
                            dialogues = new List<TextSequenceable>();
                            _dialoguesDictionary.Add(seq.ScriptableDialogue.DialogueEvent, dialogues);
                        }

                        dialogues.Add(seq);
                    }
                }
            }

            var values = Enum.GetValues(typeof(EDialogueEvent));
            foreach (EDialogueEvent dialogueEvent in values)
            {
                _dialoguePlayedDictionary.TryAdd(dialogueEvent, false);
            }
        }

        private void Update()
        {
            //foreach(EDialogueType dialogueType in _dialogueTasks.Keys)
            //{
            //    _dialogueTasks.TryGetValue(EDialogueType.Overview, out Task task);
            //    if (task != null)
            //    {
            //        if (task.Running == false && task.Paused == false)
            //        {
            //            _textGameObjectDictionary[dialogueType].SetActive(false);
            //            task.Stop();
            //            _dialogueTasks[dialogueType] = null;
            //        }
            //    }
            //}
            //return;

            _dialogueTasks.TryGetValue(EDialogueType.Overview, out Task overviewTask);
            if (overviewTask != null)
            {
                if (overviewTask.Running == false && overviewTask.Paused == false)
                {
                    _textGameObjectDictionary[EDialogueType.Overview].SetActive(false);
                    overviewTask.Stop();
                    _dialogueTasks[EDialogueType.Overview] = null;
                }
            }

            _dialogueTasks.TryGetValue(EDialogueType.CharacterOverview, out Task charOverviewTask);
            if (charOverviewTask != null)
            {
                if (charOverviewTask.Running == false && charOverviewTask.Paused == false)
                {
                    _textGameObjectDictionary[EDialogueType.CharacterOverview].SetActive(false);
                    charOverviewTask.Stop();
                    _dialogueTasks[EDialogueType.CharacterOverview] = null;
                }
            }
        }

        public bool IsDialogueActive()
        {
            bool anyActive = false;

            foreach (EDialogueType dialogueType in _textGameObjectDictionary.Keys)
            {
                GameObject textGO = _textGameObjectDictionary[dialogueType];
                bool pause = _dialoguePauseCheckDict[dialogueType];
                if (textGO.activeSelf && pause)
                {
                    anyActive = true;
                    break;
                }
            }

            return anyActive;
        }

        public Task PostText<T>(T text, EDialogueType dialogueType)
        {
            // recieve a post request 
            TMP_Text textBox = GetDialogueTextBox(dialogueType);
            if (textBox == null)
            {
                Debug.LogError("Cannot post dialogue: no text box is registered for " + dialogueType);
                return null;
            }

            if (text == null)
            {
                return null;
            }

            // decide what ui it's going to occupy and what writer effect to use
            if (_writerEffects == null || _writerEffects.Length == 0 || _writerEffects[0] == null)
            {
                Debug.LogError("Cannot post dialogue: no writer effect is configured.");
                return null;
            }
            if (!_textGameObjectDictionary.TryGetValue(dialogueType, out GameObject dialogueObject) || dialogueObject == null)
            {
                Debug.LogError("Cannot post dialogue: no dialogue object is registered for " + dialogueType);
                return null;
            }

            BaseWriterEffect writerEffect = _writerEffects[0]; // for now get first writer effect

            // check if dictionary already has coroutine running
            if (IsTextTaskRunning(dialogueType))
            {
                _dialogueTasks[dialogueType].Stop();
                _dialogueTasks[dialogueType] = null;
            }

            // start new Task coroutine
            string TString = (object)text as string;
            Task returnTask = null;

            if (TString != null)
            {
                returnTask = new Task(writerEffect.TypeText(TString, textBox), true);
                if (returnTask != null)
                {
                    _dialogueTasks[dialogueType] = returnTask;
                }
            }
            else
            {
                Phrase TPhrase = (object)text as Phrase;
                if (TPhrase != null)
                {
                    if (TPhrase.PlayAnimAtEnd == false && TPhrase.CharacterAnimation != null)
                    {
                        AnimationManager.Instance.AnimateCharacter(TPhrase.CharacterAnimation);
                    }

                    returnTask = new Task(writerEffect.TypeText(TPhrase, textBox), true);
                    if (returnTask != null)
                    {
                        _dialogueTasks[dialogueType] = returnTask;
                    }
                }
            } 

            dialogueObject.SetActive(true);

            return returnTask;


            bool IsTextTaskRunning(EDialogueType dialogueType)
            {
                _dialogueTasks.TryGetValue(dialogueType, out Task textTask);
                if (textTask != null)
                {
                    return textTask.Running;
                }

                return false;
            }
        }

        private TMP_Text GetDialogueTextBox(EDialogueType dialogueType)
        {
            _textBoxDictionary.TryGetValue(dialogueType, out TMP_Text textBox);
            return textBox;
        }

        public void OnDialogueFinished()
        {
            if (_textGameObjectDictionary.TryGetValue(EDialogueType.Speech, out GameObject speechObject) && speechObject != null)
            {
                speechObject.SetActive(false);
            }
            Debug.Log("DIALOGUE TASK FIN");
        }

        public bool TryPlayDialogue(EDialogueEvent dialogueEvent)
        {
            if (CanPlayDialogue(dialogueEvent) == false)
            {
                return false;
            }

            if (_dialoguesDictionary.TryGetValue(dialogueEvent, out List<TextSequenceable> dialogues) && dialogues.Count > 0)
            {
                TextSequenceable ts = GetNextDialogue(dialogueEvent, dialogues);
                if (!ts.StartTask())
                {
                    return false;
                }
                _dialoguePlayedDictionary[dialogueEvent] = true;
                return true;
            }
            if (dialogueEvent == EDialogueEvent.DayAny_EndWorkReceptionist_Speech)
            {
                Debug.LogWarning("No dialogue is registered for " + dialogueEvent);
            }
            return false;
        }

        private TextSequenceable GetNextDialogue(EDialogueEvent dialogueEvent, List<TextSequenceable> dialogues)
        {
            if (_dialogueSelectionPools.TryGetValue(dialogueEvent, out List<TextSequenceable> pool) == false || pool.Count == 0)
            {
                pool = new List<TextSequenceable>(dialogues);
                for (int i = pool.Count - 1; i > 0; i--)
                {
                    int swapIndex = UnityEngine.Random.Range(0, i + 1);
                    TextSequenceable temp = pool[i];
                    pool[i] = pool[swapIndex];
                    pool[swapIndex] = temp;
                }

                if (pool.Count > 1 &&
                    _lastPlayedDialogues.TryGetValue(dialogueEvent, out TextSequenceable lastPlayed) &&
                    pool[pool.Count - 1] == lastPlayed)
                {
                    int swapIndex = UnityEngine.Random.Range(0, pool.Count - 1);
                    TextSequenceable temp = pool[pool.Count - 1];
                    pool[pool.Count - 1] = pool[swapIndex];
                    pool[swapIndex] = temp;
                }

                _dialogueSelectionPools[dialogueEvent] = pool;
            }

            int lastIndex = pool.Count - 1;
            TextSequenceable nextDialogue = pool[lastIndex];
            pool.RemoveAt(lastIndex);
            _lastPlayedDialogues[dialogueEvent] = nextDialogue;
            return nextDialogue;
        }

        private bool CanPlayDialogue(EDialogueEvent dialogueEvent)
        {
            bool canPlay = true;

            if (dialogueEvent == EDialogueEvent.Day1_ChooseContract_Prompt)
            {
                canPlay = MorgueManager.Instance.IsTutorialDay == false && 
                    MorgueManager.Instance.DayCount == 1 && 
                    _dialoguePlayedDictionary[dialogueEvent] == false;
            }
            else if (dialogueEvent == EDialogueEvent.DayAny_EndWorkReceptionist_Speech)
            {
                canPlay = MorgueManager.Instance.IsTutorialDay == false;
            }

            return canPlay;
        }

        public void ManagedPreInGameLoad()
        {
             
        }

        public void ManagedPostInGameLoad()
        {
             
        }

        public void ManagedPreMainMenuLoad()
        {
             
        }

        public void ManagedPostMainMenuLoad()
        {
             
        }


    }
    
}
