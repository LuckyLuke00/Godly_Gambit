using UnityEngine;

namespace GodlyGambit
{
    public class GodCursorHelper : MonoBehaviour
    {
        [Header("Inner Circle")]
        [SerializeField] private float _innerRotationSpeed = 25f;

        [SerializeField] private GameObject _innerCircle = null;

        [Header("Outer Circle")]
        [SerializeField] private float _outterRotationSpeed = 25f;

        [SerializeField] private GameObject _outterCircle = null;

        private SpriteRenderer _innerRenderer = null;
        private SpriteRenderer _outterRenderer = null;
        private Light _light = null;

        private void Awake()
        {
            if (!_innerCircle || !_innerCircle.TryGetComponent(out _innerRenderer))
            {
                Debug.LogError("GodCursorHelper: Inner circle is not set!");
            }

            if (!_outterCircle || !_outterCircle.TryGetComponent(out _outterRenderer))
            {
                Debug.LogError("GodCursorHelper: Outter circle is not set!");
            }

            // Get the light component
            _light = GetComponentInChildren<Light>();
            if (!_light)
            {
                Debug.LogWarning("GodCursorHelper: Light is not set!");
            }
        }

        private void Update()
        {
            // Rotate the inner circle
            _innerCircle.transform.Rotate(Vector3.forward, _innerRotationSpeed * Time.deltaTime);

            // Rotate the outter circle
            _outterCircle.transform.Rotate(Vector3.forward, _outterRotationSpeed * Time.deltaTime);
        }

        public void SetInnerCircleSprite(Sprite sprite)
        {
            if (!sprite) return;
            _innerRenderer.sprite = sprite;
        }

        public void SetOutterCircleSprite(Sprite sprite)
        {
            if (!sprite) return;
            _outterRenderer.sprite = sprite;
        }

        public void DisableCircles()
        {
            _innerRenderer.enabled = false;
            _outterRenderer.enabled = false;
        }

        public void EnableCircles()
        {
            _innerRenderer.enabled = true;
            _outterRenderer.enabled = true;
        }

        public void SetLightColor(Color color)
        {
            if (!_light) return;
            _light.color = color;
        }

        public void EnableLight(bool enabled)
        {
            if (!_light) return;
            _light.enabled = enabled;
        }
    }
}
