using UnityEngine;

namespace GodlyGambit
{
    public class DeathEffectController : MonoBehaviour
    {
        [SerializeField] private float _deathEffectDuration = 4f;
        [SerializeField] private float _deathStartSinking = 1f;
        [SerializeField] private float _sinkSpeed = 2.5f;

        private float _deathTimer = 0f;
        private Transform _deadObjectTranform = null;

        #region Events

        public delegate void DeathEffectCompleteHandler();

        public event DeathEffectCompleteHandler OnDeathEffectComplete;

        #endregion Events

        private void Update()
        {
            AnimateDeath();
        }

        private void AnimateDeath()
        {
            if (_deadObjectTranform == null) return;

            if (_deathTimer >= _deathEffectDuration)
            {
                OnDeathEffectComplete?.Invoke();
                return;
            }

            _deathTimer += Time.deltaTime;

            if (_deathTimer >= _deathStartSinking)
            {
                _deadObjectTranform.Translate(Vector3.down * _sinkSpeed * Time.deltaTime);
            }
        }

        public void SetDeadObject(GameObject gameObject)
        {
            _deadObjectTranform = gameObject.transform;
        }
    }
}
