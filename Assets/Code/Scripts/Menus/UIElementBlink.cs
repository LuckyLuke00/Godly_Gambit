using UnityEngine;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class UIElementBlink : MonoBehaviour
    {
        // UI element to blink
        [SerializeField] private Graphic[] _graphic = null;

        [SerializeField] private float _blinkSpeed = .5f;
        [SerializeField, Range(0, 1)] private float _minAlpha = 0.2f;

        private void Awake()
        {
            if (_graphic.Length < 1) Debug.LogError("UIElementBlink: No Graphic found!");
        }

        private void Update()
        {
            if (_graphic.Length < 1) return;

            foreach (var graphic in _graphic)
            {
                var color = graphic.color;
                color.a = Mathf.PingPong(Time.time * _blinkSpeed, 1 - _minAlpha) + _minAlpha;

                graphic.color = color;
            }
        }
    }
}
