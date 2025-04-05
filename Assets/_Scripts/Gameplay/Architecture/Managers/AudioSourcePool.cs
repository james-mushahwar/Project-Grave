using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class AudioSourcePool : PoolComponentManager<AudioSource>
    {
        protected override void Awake()
        {
            base.Awake();

            CreateAudioSourcePool();
        }

        private void CreateAudioSourcePool()
        {
            for (int i = 0; i < m_PoolCount; ++i)
            {
                GameObject newGO = new GameObject(gameObject.name + i);
                newGO.transform.parent = this.gameObject.transform;

                AudioSource comp = newGO.AddComponent(typeof(AudioSource)) as AudioSource;
                comp.volume = 1.0f;
                comp.maxDistance = 35.0f;
                comp.outputAudioMixerGroup = AudioManager.Instance.SFXMixerGroup;
                comp.gameObject.SetActive(false);
                m_Pool.Push(comp);

                AudioManager.Instance.RegisterPooledAudioSource(comp);
            }

            foreach (AudioSource aSource in m_Pool)
            {
                aSource.playOnAwake = false;
            }
        }

        public void CleanAudioSourcePool()
        {
            CheckPools();
            return;

            // try recollecting all audio sources
            foreach (AudioSource aSource in m_Pool.ToArray())
            {
                if (aSource != null)
                {
                    aSource.gameObject.transform.parent = gameObject.transform;
                }
                else
                {
                    Debug.LogWarning("Pooled audio source gone missing or destroyed!");
                }
            }

            return;

            foreach (AudioSource aSource in m_Pool.ToArray())
            {
                if (aSource != null)
                {
                    Destroy(aSource.gameObject);
                }
            }
            m_Pool.Clear();
            CreateAudioSourcePool();
        }

        protected override bool IsActive(AudioSource component)
        {
            bool isPlaying = component.isPlaying;
            bool isHandled = false;

            if (!isPlaying)
            {
                // check looping handle
                isHandled = AudioManager.Instance.AudioSourceIsHandled(component);

            }

            if (!isHandled && !isPlaying)
            {
                AudioManager.Instance.CleanConcurrency(component);
            }

            bool isActive = isPlaying || isHandled;

            if (!isActive)
            {

            }

            return isActive;
        }

        public AudioSource GetAudioSource()
        {
            return GetPooledComponent();
        }
    }
    
}
