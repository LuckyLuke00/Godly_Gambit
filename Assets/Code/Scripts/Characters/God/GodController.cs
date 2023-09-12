using UnityEngine;

namespace GodlyGambit
{
    public class GodController : MonoBehaviour
    {
        [SerializeField] private bool _lookAtCursor = true;
        [SerializeField] private float _lookSpeed = 10f;
        [SerializeField] private float _maxTotalRotation = 45f;
        [SerializeField] private Transform _visualToRotate = null;

        private CursorController _cursorController = null;
        private Quaternion _startRotation = Quaternion.identity;

        #region Animation

        private Animator _animator = null;

        #endregion Animation

        private void Awake()
        {
            _cursorController = GetComponentInChildren<CursorController>();
            if (!_cursorController) Debug.LogError("GodController: No CursorController found!");

            if (!_visualToRotate)
            {
                Debug.LogWarning("GodController: No visual to rotate found! Using transform instead.");
                _visualToRotate = transform;
            }

            _animator = GetComponentInChildren<Animator>();
            if (!_animator) Debug.LogError("GodController: No animator found!");

            _startRotation = _visualToRotate.rotation;
        }

        private void Update()
        {
            if (_lookAtCursor) LookAt(_cursorController.Position);
        }

        private void OnEnable()
        {
            SpawnSoldiersAbility.OnPlaceSoldiers += PlaySoldierAnimation;
            SpawnMeteoriteAbility.OnPlaceMeteorite += PlayMeteoriteAnimation;
        }

        private void OnDisable()
        {
            SpawnSoldiersAbility.OnPlaceSoldiers -= PlaySoldierAnimation;
            SpawnMeteoriteAbility.OnPlaceMeteorite -= PlayMeteoriteAnimation;
        }

        private void PlaySoldierAnimation()
        {
            _animator.ResetTrigger("TriggerMeteor");
            _animator.SetTrigger("TriggerSoldiers");
        }

        private void PlayMeteoriteAnimation()
        {
            _animator.ResetTrigger("TriggerSoldiers");
            _animator.SetTrigger("TriggerMeteor");
        }

        private void LookAt(Vector3 pos)
        {
            // Get the direction to the target
            Vector3 dir = pos - _visualToRotate.position;
            dir.y = 0f;

            // Get the rotation to the target
            Quaternion targetRotation = Quaternion.LookRotation(dir);

            // Calculate the angle difference from the start rotation
            float angle = Quaternion.Angle(_startRotation, targetRotation);

            // Clamp the angle within the max total rotation
            float clampedAngle = Mathf.Clamp(angle, -_maxTotalRotation, _maxTotalRotation);

            // Calculate the target rotation with the clamped angle
            targetRotation = Quaternion.RotateTowards(_startRotation, targetRotation, clampedAngle);

            // Slerp the rotation
            Quaternion newRotation = Quaternion.Slerp(_visualToRotate.rotation, targetRotation, _lookSpeed * Time.deltaTime);

            // Apply the rotation
            _visualToRotate.rotation = newRotation;
        }

        public void Kill()
        {
            Destroy(gameObject);
        }
    }
}
