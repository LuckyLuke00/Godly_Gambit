using UnityEngine;

namespace GodlyGambit
{
    public class LightFlickering : MonoBehaviour
    {
        [SerializeField] private float _flickerIntensity = .2f;
        [SerializeField] private float _flickersPerSecond = 3f;
        [SerializeField] private float _speedRandomness = 1f;

        private float _time = .0f;
        private float _startingIntensity = .0f;
        private Light _flickeringLight = null;

        private void Awake()
        {
            if (!TryGetComponent(out _flickeringLight))
            {
                Debug.LogError("LightFlickering script requires a Light component on the same GameObject.");
            }

            _startingIntensity = _flickeringLight.intensity;
        }

        // Update is called once per frame
        private void Update()
        {
            _time += Time.deltaTime * (1 - Random.Range(-_speedRandomness, _speedRandomness)) * Mathf.PI;
            _flickeringLight.intensity = _startingIntensity + Mathf.Sin(_time * _flickersPerSecond) * _flickerIntensity;
        }
    }
}
