using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class DamageComponent : MonoBehaviour
    {
        #region Variables

        private const string FRIENDLY_TAG = "Fighter";
        private const string ENEMY_TAG = "Enemy";

        [SerializeField] private bool _destroyOnHit = false;
        [SerializeField] private bool _ignoreEnemies = false;
        [SerializeField] private bool _ignoreFriendlies = false; // TODO: Prevent player from shooting himself.
        [SerializeField] private float _damageFalloff = 2f;
        [SerializeField] private float _maxHits = 3f;
        [SerializeField] private uint _damage = 10;

        [Header("Sound settings")]
        [SerializeField] private AudioSource _hitSound;

        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        [SerializeField, Tooltip("This oly works when there is a sound, these objects get disabeld when the sound is playing.")]
        private List<GameObject> _objectsToDisable;

        // The collider of the object that will receive damage
        private Collider _receiverCollider = null;

        private HealthComponent _receiverHealthComponent = null;

        #endregion Variables

        public uint Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the other collider is a trigger, if so ignore it.
            // (Prevent destroying the arrow when an enemy is doing damage)
            if (other.isTrigger) return;

            if (_ignoreFriendlies && other.CompareTag(FRIENDLY_TAG)) return;
            if (_ignoreEnemies && other.CompareTag(ENEMY_TAG)) return;

            if (!other.TryGetComponent(out _receiverHealthComponent) && !_hitSound.isPlaying)
            {
                if (_destroyOnHit) Destroy(gameObject);
                return;
            }

            _receiverCollider = other;

            DoDamage();
        }

        private void OnTriggerExit(Collider other)
        {
            // If the other collider is not the receiver collider, ignore it.
            if (other != _receiverCollider) return;

            _receiverCollider = null;
            _receiverHealthComponent = null;
        }

        public void DoDamage()
        {
            if (!_receiverCollider || !_receiverHealthComponent) return;

            var hitPoint = _receiverCollider.ClosestPointOnBounds(transform.position);
            var hitDirection = (hitPoint - transform.position).normalized;
            _receiverHealthComponent?.Damage(_damage, hitPoint, hitDirection);

            // Damage was applied, so reduce the damage for the next hit.
            if (_damageFalloff > 0)
            {
                _damage /= (uint)_damageFalloff;
                --_maxHits;
            }

            if (_hitSound && !_hitSound.isPlaying)
            {
                float randomPitch = Random.Range(_minPitch, _maxPitch);
                _hitSound.pitch = randomPitch;

                _hitSound.Play();
                foreach (GameObject obj in _objectsToDisable)
                {
                    if (obj != null) obj.SetActive(false);
                }

                if (_destroyOnHit || _maxHits < 1) Destroy(gameObject, _hitSound.clip.length - 0.5f);
            }
            else if (!_hitSound && (_destroyOnHit || _maxHits < 1))
            {
                Destroy(gameObject);
            }
        }
    }
}
