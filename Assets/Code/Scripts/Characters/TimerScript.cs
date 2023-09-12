using UnityEngine;

using System.Collections;


namespace GodlyGambit
{
    public class TimerScript : MonoBehaviour
    {        
        private float _timeAccumulated;

        public float GetAccumulatedTime { get { return _timeAccumulated; } }

        private bool _isFighter = false;

        private void Awake()
        {
            CheckIfFighter();
        }

        private void OnEnable()
        {
            PlayerManager.OnGameStarted += CheckIfFighter;
            PlayerSwapper.OnPlayerSwapped += CheckIfFighter;
        }

        private void OnDisable()
        {
            PlayerManager.OnGameStarted -= CheckIfFighter;
            PlayerSwapper.OnPlayerSwapped -= CheckIfFighter;
        }

        private void Update()
        {
            // TODO: Avoid using GetComponentInChildren every frame
            if (_isFighter)
            {
                // Accumulate time
                _timeAccumulated += Time.deltaTime;
            }
        }

        private void CheckIfFighter()
        {
            StartCoroutine(DelayedCheckIfFighter());
        }

        private IEnumerator DelayedCheckIfFighter()
        {
            yield return null; // Wait for the current frame to finish
           
            // Check if the object is a fighter
            _isFighter = gameObject.GetComponentInChildren<FighterController>() != null;
        }
    }
}
