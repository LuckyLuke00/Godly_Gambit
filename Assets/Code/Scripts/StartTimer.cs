using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class StartTimer : MonoBehaviour
    {
        [SerializeField] private float _countdownDuration = 3f;
        [SerializeField] private Text _timerTxt;
        [SerializeField] private string _timerFinalText = "GO!";

        private float _currentCountdown;
        private bool _isCountingDown;
        private PlayerInput[] _playerInputs;

        public bool IsCountingDown { get { return _isCountingDown; } }

        private void Start()
        {
            _currentCountdown = _countdownDuration;
            _isCountingDown = true;
            _playerInputs = FindObjectsOfType<PlayerInput>();
            DisablePlayerInputs();
        }

        private void OnEnable()
        {
            PlayerManager.OnNewPlayerJoined += onPlayerJoin;
        }

        private void OnDisable()
        {
            PlayerManager.OnNewPlayerJoined -= onPlayerJoin;
        }

        private void Update()
        {
            if (_isCountingDown)
            {
                _currentCountdown -= Time.deltaTime;
                _timerTxt.text = _currentCountdown.ToString("0");

                if (_currentCountdown < 1)
                {
                    _timerTxt.text = _timerFinalText;
                                                            
                    TimerComponent timerComponent = FindObjectOfType<TimerComponent>();
                    if (timerComponent != null)
                    {
                        timerComponent.ResetTimer();
                    }

                    EnablePlayerInputs();
                }

                if (_currentCountdown <= 0f)
                {
                    _isCountingDown = false;
                    _timerTxt.enabled = false;
                }
            }
        }

        private void onPlayerJoin()
        {
            if (_isCountingDown)
                StartCoroutine(OnPlayerJoinDelayed());
        }

        private IEnumerator OnPlayerJoinDelayed()
        {
            yield return null;
            yield return null;
            _playerInputs = FindObjectsOfType<PlayerInput>();
            DisablePlayerInputs();
        }

        private void DisablePlayerInputs()
        {
            foreach (var playerInput in _playerInputs)
            {
                playerInput.DeactivateInput();
            }
        }

        private void EnablePlayerInputs()
        {
            foreach (var playerInput in _playerInputs)
            {
                playerInput.ActivateInput();
            }
        }
    }
}
