using UnityEngine;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class FighterDashCircle : MonoBehaviour
    {
        [SerializeField] private bool _disableOnAwake = true;
        [SerializeField] private bool _disableOnFull = true;
        [SerializeField] private Image _dashCircle = null;

        private void Awake()
        {
            _dashCircle.enabled = _disableOnAwake;
            if (_dashCircle == null)
            {
                Debug.LogError("FighterDashCircle: _dashCircle is null");
            }
        }

        public void SetDashCirclePercent(float value)
        {
            _dashCircle.fillAmount = Mathf.Clamp01(value);

            // If the value is 1, hide the dash circle
            _dashCircle.enabled = _disableOnFull && value < 1f;
        }
    }
}
