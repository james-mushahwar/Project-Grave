using _Scripts.Gameplay.ActionSequence;
using _Scripts.Gameplay.Architecture.Managers;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Playables;

namespace _Scripts.Gameplay.Architecture.DayCycle {

    public class DayNightCycle : MonoBehaviour, IManaged
    {
        public bool CanTick
        {
            get
            {
                return gameObject.activeSelf && this.isActiveAndEnabled;
            }
            set => throw new System.NotImplementedException();
        }

        public event Action _onDayStart;
        public event Action _onNightStart;

        public float dayDuration = 600f; // 10 minutes in seconds
        private float timeOfDay = 0f; // Normalized time (0 to 1 for a full day)



        [SerializeField]
        private float _timeElapseFactor = 1.0f; // multiplier for time passing

        [SerializeField]
        private PlayableDirector _dayNightTimelineForward;
        [SerializeField]
        private PlayableDirector _dayNightTimelineBackward;
        [SerializeField]
        private float _dayNightTransitionSpeed = 1.0f;

        private Light Sun
        {
            get
            {
                return LightingManager.Instance.Sun;
            }
        }
        private Light Moon
        {
            get
            {
                return LightingManager.Instance.Moon;
            }
        }

        //get the next 


        public Tuple<int, int> Time_24Hr
        {
            get
            {
                int hours = (int)timeOfDay * 24;
                int minutes = (int)(timeOfDay - ((float)hours / 24) * 60);
                return Tuple.Create(hours, minutes);
            }
        }

        public EDayTimeline TargetTimeline { get => MorgueManager.Instance.TargetTimeline; set => MorgueManager.Instance.TargetTimeline = value; }
        public EDayTimeline CurrentTimeline { get => MorgueManager.Instance.CurrentTimeline; set => MorgueManager.Instance.CurrentTimeline = value; }

        private PlayableDirector _currentPlayingTimeline;

        public void ManagedTick()
        {
            return;
            if (!CanTick)
            {
                return;
            }

            // Increment time based on real-world seconds
            timeOfDay += (Time.deltaTime / dayDuration) * _timeElapseFactor;
            if (timeOfDay >= 1f) timeOfDay -= 1f; // Loop back to 0 after a full day

            // Calculate sun and moon rotation (0° = sunrise, 180° = sunset)
            float sunAngle = timeOfDay * 360f; // Full 360° rotation over the day
            float moonAngle = sunAngle + 180f; // Moon is opposite the sun
            if (moonAngle >= 360f) moonAngle -= 360f;

            // Rotate sun and moon around X-axis
            Sun.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f); // 45° Y-axis for a tilted path
            Moon.transform.rotation = Quaternion.Euler(moonAngle, 45f, 0f);

            // Adjust light intensity based on time of day
            float sunIntensity = Mathf.Sin(timeOfDay * Mathf.PI * 2f); // Sine wave for smooth intensity
            Sun.intensity = Mathf.Clamp01(sunIntensity) * 1.5f; // Max intensity 1.5 during day
            Moon.intensity = Mathf.Clamp01(-sunIntensity) * 0.5f; // Max intensity 0.5 at night

            // Adjust skybox exposure for day/night contrast
            float exposure = Mathf.Lerp(0.2f, 1f, Sun.intensity / 1.5f); // 0.2 at night, 1 at day
            RenderSettings.skybox.SetFloat("_Exposure", exposure);

            // Optional: Adjust sun/moon colors
            Sun.color = Color.Lerp(new Color(1f, 0.5f, 0.5f), Color.white, Sun.intensity / 1.5f); // Warm at dawn/dusk
            Moon.color = new Color(0.8f, 0.8f, 1f); // Cool moonlight
        }

        public void PlayDayNightTimeline(EDayTimeline timeline = EDayTimeline.None, bool forward = true)
        {
            if (timeline == EDayTimeline.None)
            {
                timeline = MorgueManager.Instance.NextCellestialTimeline;
            }

            if (TargetTimeline == timeline || CurrentTimeline == timeline)
            {
                return;
            }

            Debug.Log("DayNightTimeline: Moving to => " + timeline.ToString());

            TargetTimeline = timeline;

            PlayableDirector timelineToPlay = forward ? _dayNightTimelineForward : _dayNightTimelineBackward;

            timelineToPlay.Play();
            timelineToPlay.playableGraph.GetRootPlayable(0).SetSpeed(_dayNightTransitionSpeed);

            _currentPlayingTimeline = timelineToPlay;
        }

        public void PauseDayNightTimeline()
        {
            _currentPlayingTimeline.playableGraph.GetRootPlayable(0).SetSpeed(0.0f);
        }

        public void OnMorningStartTriggered()
        {
            OnTimelineEventTriggered(EDayTimeline.Morning_Start);
        }
        public void OnMiddayStartTriggered()
        {
            OnTimelineEventTriggered(EDayTimeline.Midday_Start);
        }
        public void OnEveningStartTriggered()
        {
            OnTimelineEventTriggered(EDayTimeline.Evening_Start);
        }
        public void OnNightStartTriggered()
        {
            OnTimelineEventTriggered(EDayTimeline.Night_Start);
        }

        public void OnTimelineEventTriggered(EDayTimeline timelineEvent)
        {
            if (timelineEvent != TargetTimeline)
            {
                return;
            }

            CurrentTimeline = TargetTimeline;
            TargetTimeline = EDayTimeline.None;

            PauseDayNightTimeline();
        }

        public void Enable()
        {
            throw new System.NotImplementedException();
        }

        public void Disable()
        {
            throw new System.NotImplementedException();
        }
    }
}
