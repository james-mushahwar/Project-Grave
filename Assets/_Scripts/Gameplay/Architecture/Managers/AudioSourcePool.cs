using System.Collections;
using System.Collections.Generic;
using _Scripts.CautionaryTalesScripts;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{
    
    public class AudioSourcePool : PoolComponentManager<CTAudioSource>
    {
        [SerializeField] private GameObject _audioSourcePrefab;

        protected override void Awake()
        {
            base.Awake();

            CreateAudioSourcePool();
        }

        private void CreateAudioSourcePool()
        {
            for (int i = 0; i < m_PoolCount; ++i)
            {
                GameObject newGO = GameObject.Instantiate(_audioSourcePrefab);
                newGO.transform.parent = this.gameObject.transform;

                CTAudioSource comp = newGO.GetComponent(typeof(CTAudioSource)) as CTAudioSource;
                if (comp != null)
                {
                    comp.Source.volume = 1.0f;
                    comp.Source.maxDistance = 35.0f;
                    comp.Source.outputAudioMixerGroup = AudioManager.Instance.SFXMixerGroup;
                    comp.gameObject.SetActive(false);
                    m_Pool.Push(comp);

                    AudioManager.Instance.RegisterPooledAudioSource(comp.Source);
                }
            }


            for (int i = 0; i < m_PoolCount; ++i)
            {
                //GameObject newGO = new GameObject(gameObject.name + i);
                //newGO.transform.parent = this.gameObject.transform;

                //AudioSource comp = newGO.AddComponent(typeof(AudioSource)) as AudioSource;
                //comp.volume = 1.0f;
                //comp.maxDistance = 35.0f;
                //comp.outputAudioMixerGroup = AudioManager.Instance.SFXMixerGroup;
                //comp.gameObject.SetActive(false);
                //m_Pool.Push(comp);

                //AudioManager.Instance.RegisterPooledAudioSource(comp);
            }

            foreach (CTAudioSource aSource in m_Pool)
            {
                aSource.Source.playOnAwake = false;
            }
        }

        public void CleanAudioSourcePool()
        {
            CheckPools();
            return;

            // try recollecting all audio sources
            foreach (CTAudioSource aSource in m_Pool.ToArray())
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

            foreach (CTAudioSource aSource in m_Pool.ToArray())
            {
                if (aSource != null)
                {
                    Destroy(aSource.gameObject);
                }
            }
            m_Pool.Clear();
            CreateAudioSourcePool();
        }

        protected override bool IsActive(CTAudioSource component)
        {
            bool isPlaying = component.Source.isPlaying;
            bool isHandled = false;

            if (!isPlaying)
            {
                // check looping handle
                isHandled = AudioManager.Instance.AudioSourceIsHandled(component.Source);

            }

            if (!isHandled && !isPlaying)
            {
                AudioManager.Instance.CleanConcurrency(component.Source);
            }

            bool isActive = isPlaying || isHandled;

            if (!isActive)
            {

            }

            return isActive;
        }

        public AudioSource GetAudioSource()
        {
            CTAudioSource ctAudio = GetPooledComponent();
            if (ctAudio != null)
            {
                return ctAudio.Source;
            }

            return null;
        }

        public CTAudioSource GetCTAudioSource()
        {
            return GetPooledComponent();
        }
    }
    
}
