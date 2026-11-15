using System.Reflection;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.Settings;
using _Scripts.Org;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ActionSequenceManagerTests
{
    private GameObject _objects;
    private MasterProjectSettings _settings;
    private ActionSequenceManager _manager;
    private TestSequence _sequence;
    private const EActionSequenceEvent SequenceEvent = EActionSequenceEvent.Day0_CloseGate;

    [SetUp]
    public void SetUp()
    {
        _objects = new GameObject("ActionSequenceManagerTests");
        var gameState = _objects.AddComponent<GameStateManager>();
        _objects.AddComponent<MorgueManager>();
        _manager = _objects.AddComponent<ActionSequenceManager>();
        _settings = ScriptableObject.CreateInstance<MasterProjectSettings>();
        SetField(gameState, "_masterProjectSettings", _settings);

        var sequenceSettings = new ActionSequenceSettings();
        SetField(sequenceSettings, "_actionSequenceEvent", SequenceEvent);
        _sequence = new TestSequence(_objects.AddComponent<RuntimeID>(), sequenceSettings);
        _manager.TryRegisterActionSequence(_sequence);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_objects);
        Object.DestroyImmediate(_settings);
    }

    [Test]
    public void RepeatedStartDoesNotLeaveSequencePlayingAcrossDays()
    {
        for (int day = 0; day < 3; day++)
        {
            Assert.IsTrue(_manager.TryPlayActionSequence(SequenceEvent));
            Assert.IsFalse(_manager.TryPlayActionSequence(SequenceEvent));
            _sequence.OnCompleted();
            Assert.IsFalse(_manager.IsPlayingActionSequence(SequenceEvent));
            Assert.IsFalse(_manager.LockPlay);
        }
    }

    [Test]
    public void FailedStartDoesNotLeaveSequencePlaying()
    {
        _sequence.FailPlay = true;
        LogAssert.Expect(LogType.Warning, "Failed to play action sequence " + SequenceEvent);
        Assert.IsFalse(_manager.TryPlayActionSequence(SequenceEvent));
        Assert.IsFalse(_manager.IsPlayingActionSequence(SequenceEvent));
        Assert.IsFalse(_manager.LockPlay);

        _sequence.FailPlay = false;
        Assert.IsTrue(_manager.TryPlayActionSequence(SequenceEvent));
        _sequence.OnCompleted();
        Assert.IsFalse(_manager.IsPlayingActionSequence(SequenceEvent));
    }

    private static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private class TestSequence : IActionSequence
    {
        public bool FailPlay;
        private bool _playing;
        public ActionSequenceSettings ActionSequenceSettings { get; }
        public RuntimeID RuntimeId { get; }

        public TestSequence(RuntimeID runtimeId, ActionSequenceSettings settings)
        {
            RuntimeId = runtimeId;
            ActionSequenceSettings = settings;
        }

        public bool Play()
        {
            if (FailPlay || _playing) return false;
            _playing = true;
            return true;
        }

        public bool Stop() { _playing = false; return true; }
        public bool Pause() { return false; }
        public bool IsPlaying() { return _playing; }
        public bool CanPlay() { return !_playing; }
        public void OnStarted() { }
        public void OnPaused() { }
        public void OnCompleted()
        {
            _playing = false;
            ActionSequenceManager.Instance.OnActionSequenceCompleted(ActionSequenceSettings.ActionSequenceEvent);
        }
    }
}
