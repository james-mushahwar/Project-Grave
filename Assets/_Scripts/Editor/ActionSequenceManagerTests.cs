using System.Reflection;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Gameplay.Settings;
using _Scripts.Org;
using Cinemachine;
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

    [Test]
    public void DayLoopSleepCannotInterruptTutorial()
    {
        var settings = new ActionSequenceSettings();
        SetField(settings, "_actionSequenceEvent", EActionSequenceEvent.DayAny_GoToSleep);
        var sleep = new TestSequence(_objects.AddComponent<RuntimeID>(), settings);
        _manager.TryRegisterActionSequence(sleep);

        SetField(_manager, "_previousMajorActionSequence", EActionSequenceEvent.DayAny_WakeUp);
        Assert.IsFalse(_manager.TryPlayActionSequence(EActionSequenceEvent.DayAny_GoToSleep));
        Assert.AreEqual(EActionSequenceEvent.DayAny_WakeUp, _manager.PreviousMajorActionSequence);

        SetField(MorgueManager.Instance, "_workDoneForDay", true);
        Assert.IsFalse(_manager.TryPlayActionSequence(EActionSequenceEvent.DayAny_GoToSleep));

        SetField(_manager, "_previousMajorActionSequence", EActionSequenceEvent.Day0_PickupSaw);
        Assert.IsTrue(_manager.CanPlayActionSequence(EActionSequenceEvent.Day0_MeetReceptionist, null));
        Assert.IsTrue(_manager.CanPlayActionSequence(EActionSequenceEvent.Day0_GoToSleep, null));
    }

    [Test]
    public void DayLoopSleepRequiresCompletedWorkAfterTutorial()
    {
        SetField(MorgueManager.Instance, "_dayCount", 1);
        Assert.IsFalse(_manager.CanPlayActionSequence(EActionSequenceEvent.DayAny_GoToSleep, null));

        SetField(MorgueManager.Instance, "_workDoneForDay", true);
        Assert.IsTrue(_manager.CanPlayActionSequence(EActionSequenceEvent.DayAny_GoToSleep, null));
    }

    [Test]
    public void PlayerLookDoesNotRotateSharedTimelineCameraAfterSequence()
    {
        var playerObject = new GameObject("Player");
        playerObject.transform.SetParent(_objects.transform);
        var player = playerObject.AddComponent<PlayerController>();
        var playerCameraObject = new GameObject("Player camera");
        playerCameraObject.transform.SetParent(player.transform);
        var playerCamera = playerCameraObject.AddComponent<CinemachineVirtualCamera>();
        var timelineCameraObject = new GameObject("Shared timeline camera");
        timelineCameraObject.transform.SetParent(_objects.transform);
        var timelineCamera = timelineCameraObject.AddComponent<CinemachineVirtualCamera>();
        timelineCamera.transform.rotation = Quaternion.Euler(-9f, 87f, 0f);

        object firstPersonCamera = new FVirtualCamera();
        SetField(firstPersonCamera, "_virtualCamera", playerCamera);
        SetField(player, "_firstPersonVCam", firstPersonCamera);
        var rotateView = typeof(PlayerController).GetMethod("RotateView", BindingFlags.Instance | BindingFlags.NonPublic);
        Quaternion timelineRotation = timelineCamera.transform.rotation;
        Quaternion playerRotation = player.transform.rotation;

        rotateView.Invoke(player, new object[] { timelineCamera, 10f, 5f });
        Assert.Less(Quaternion.Angle(timelineRotation, timelineCamera.transform.rotation), 0.001f);
        Assert.Less(Quaternion.Angle(playerRotation, player.transform.rotation), 0.001f);

        rotateView.Invoke(player, new object[] { playerCamera, 10f, 5f });
        Assert.That(Quaternion.Angle(playerRotation, player.transform.rotation), Is.EqualTo(10f).Within(0.01f));
        rotateView.Invoke(player, new object[] { playerCamera, 0f, 0f });
        Assert.That(Quaternion.Angle(Quaternion.identity, playerCamera.transform.localRotation), Is.EqualTo(5f).Within(0.01f));
        Assert.Less(Quaternion.Angle(timelineRotation, timelineCamera.transform.rotation), 0.001f);
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
