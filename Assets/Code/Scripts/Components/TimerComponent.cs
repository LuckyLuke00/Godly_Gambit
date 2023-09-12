using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class TimerComponent : MonoBehaviour
    {
        [SerializeField] private int _minutes = 3;
        [SerializeField] private int _seconds = 0;
        [SerializeField] private Text _timerTxt;
        [SerializeField] private Image _timerImage;

        [SerializeField, Tooltip("The timer will start shacking and changes collor if this ammount of seconds are left")]
        private int _warningSeconds = 10;

        [SerializeField] private float _shakeAmount = 10f;
        [SerializeField] private float _shakeSpeed = 50f;
        [SerializeField] private Color _timerShakeColor = Color.red;

        [Space(3)]
        [SerializeField, Tooltip("Time per player extra")] private int _timePerPlayerExtra;

        private bool _timerOn = false;
        private float _totalSeconds;
        private Vector2 _timerTxtPos;
        private Vector2 _timerImagePos;
        private Color _timerBaseColor = Color.white;

        private int _oldMaxSeconds;

        public delegate void TimeIsUp();

        public static event TimeIsUp OnTimeIsUp;

        public void StartTimer()
        {
            _timerOn = true;
        }

        public void StopTimer()
        {
            _timerOn = false;
        }

        private void Awake()
        {
            if (_timerTxt == null)
            {
                Debug.Log("No timer Text Found");
            }
            else
            {
                _timerTxtPos = _timerTxt.rectTransform.anchoredPosition;
            }
            if (_timerImage != null)
            {
                _timerImagePos = _timerImage.rectTransform.anchoredPosition;
            }

            // Try to get the timer base color
            if (_timerTxt != null)
            {
                _timerBaseColor = _timerTxt.color;
            }
            else if (_timerImage != null)
            {
                _timerBaseColor = _timerImage.color;
            }

            _oldMaxSeconds = _seconds;
        }

        private void OnEnable()
        {
            PlayerManager.OnNewPlayerJoined += TimerScaling;
        }

        private void OnDisable()
        {
            PlayerManager.OnNewPlayerJoined -= TimerScaling;
        }

        private void Update()
        {
            if (_timerOn)
            {
                if (_totalSeconds > 0)
                {
                    _totalSeconds -= Time.deltaTime;
                    UpdateTimer(_totalSeconds);
                }
                else
                {
                    OnTimeIsUp?.Invoke();
                    _totalSeconds = 0;
                    _timerOn = false;
                    ResetTimer();
                }
            }
        }

        private void UpdateTimer(float currentTime)
        {
            currentTime += 1;

            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);

            if (_totalSeconds <= _warningSeconds)
            {
                float shakeAngle = Mathf.Sin(Time.time * _shakeSpeed) * _shakeAmount;
                Quaternion shakeRotation = Quaternion.Euler(0f, 0f, shakeAngle);

                _timerTxt.rectTransform.localRotation = shakeRotation;
                _timerTxt.color = _timerShakeColor;

                if (_timerImage)
                {
                    _timerImage.rectTransform.localRotation = Quaternion.Inverse(shakeRotation);
                    _timerImage.color = _timerShakeColor;
                }
            }
            else
            {
                _timerTxt.rectTransform.localRotation = Quaternion.identity;
                _timerTxt.color = _timerBaseColor;

                if (_timerImage)
                {
                    _timerImage.rectTransform.localRotation = Quaternion.identity;
                    _timerImage.color = _timerBaseColor;
                }
            }

            _timerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void ResetTimer()
        {
            _timerOn = true;
            _totalSeconds = _minutes * 60 + _seconds;

            _timerTxt.rectTransform.localPosition = _timerTxtPos;
            _timerTxt.rectTransform.localRotation = Quaternion.identity;
            _timerTxt.color = _timerBaseColor;

            if (!_timerImage) return;
            _timerImage.rectTransform.localPosition = _timerImagePos;
            _timerImage.rectTransform.localRotation = Quaternion.identity;
            _timerImage.color = _timerBaseColor;
        }

        private void TimerScaling()
        {
            StartCoroutine(DelayedDifficultyScaling());
        }

        private IEnumerator DelayedDifficultyScaling()
        {
            yield return null;

            var playerManager = FindObjectOfType<PlayerManager>();
            _seconds = _oldMaxSeconds + (_timePerPlayerExtra * (playerManager.PlayerCount - 1));
            StartTimer startTimer = FindObjectOfType<StartTimer>();
            if (startTimer != null)
            {
                if (!startTimer.IsCountingDown)
                {
                    ResetTimer();
                }
            }            
        }
    }
}
