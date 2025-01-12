using System.Collections;
using System.Collections.Generic;
using _Scripts.Gameplay.Architecture.Managers;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.Gameplay.General.Gate{
    
    public class GateEntrance : MonoBehaviour
    {
        [SerializeField] private GameObject _gateGO;
        [SerializeField] private AudioSource _gateAS;
        [SerializeField] private AudioClip _gateOpenAC;
        [SerializeField] private AudioClip _gateCloseAC;

        [SerializeField] private float _distanceToTrigger;

        private float _changeStateCooldown = 0.0f;

        void Update()
        {
            if (GameStateManager.Instance.GameState != EGameState.PlayingGame)
            {
                return;
            }

            if (PlayerManager.Instance.CurrentPlayerController == null)
            {
                return;
            }


            bool canChangeState = _changeStateCooldown <= 0.0f;
            if (!canChangeState)
            {
                _changeStateCooldown -= Time.deltaTime;

                if (_changeStateCooldown < 0.0f)
                {
                    _changeStateCooldown = 0.0f;
                }
            }
            else
            {
                Vector2 gateToPlayer = _gateGO.transform.position - PlayerManager.Instance.CurrentPlayerController.transform.position;
                bool shouldBeOpen = gateToPlayer.magnitude < _distanceToTrigger;
                bool isAlreadyOpen = _gateGO.activeSelf == false;

                if (canChangeState && shouldBeOpen != isAlreadyOpen)
                {
                    _gateGO.SetActive(shouldBeOpen);

                    _gateAS.clip = shouldBeOpen ? _gateOpenAC : _gateCloseAC;
                    _gateAS.Play();

                    _changeStateCooldown = 0.5f;
                }
            }


        }
    }
    
}
