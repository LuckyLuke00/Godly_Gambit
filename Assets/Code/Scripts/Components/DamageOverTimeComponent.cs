using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class DamageOverTimeComponent : MonoBehaviour
    {
        #region Variabeles

        private const string FRIENDLY_TAG = "Fighter";
        private const string ENEMY_TAG = "Enemy";

        [SerializeField] private bool _dammageImmediately = true;
        [SerializeField] private bool _ignoreEnemies = false;
        [SerializeField] private bool _ignoreFriendlies = false;
        [SerializeField] private uint _damage = 10;
        [SerializeField, Tooltip("in seconds")] private float _intervalTimeS = 2;

        private readonly List<HealthComponent> _characters = new List<HealthComponent>();

        #endregion Variabeles

        #region Events

        // Create an event that gets triggered on an attack
        public delegate void AttackEventHandler(GameObject sender);

        public static event AttackEventHandler OnAttack;

        public delegate void CanAttackEventHandler();

        public event CanAttackEventHandler OnCanAttack;

        #endregion Events

        private void Awake()
        {
            InvokeRepeating("DoDamageOverTime", _intervalTimeS, _intervalTimeS);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_ignoreFriendlies && other.CompareTag(FRIENDLY_TAG)) return;
            if (_ignoreEnemies && other.CompareTag(ENEMY_TAG)) return;

            if (!other.TryGetComponent(out HealthComponent healthComponent)) return;
            _characters.Add(healthComponent);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_ignoreFriendlies && other.CompareTag(FRIENDLY_TAG)) return;
            if (_ignoreEnemies && other.CompareTag(ENEMY_TAG)) return;

            if (!other.TryGetComponent(out HealthComponent healthComponent)) return;
            _characters.Remove(healthComponent);
        }

        private void DoDamageOverTime()
        {
            if (_dammageImmediately) DoDamage();
            else if (_characters.Count > 0) OnCanAttack?.Invoke();
        }

        public void DoDamage()
        {
            foreach (HealthComponent healthComponent in _characters)
            {
                // Invoke the OnAttack event and pass the gameobject that triggered the attack
                OnAttack?.Invoke(transform.parent.gameObject);
                healthComponent.Damage(_damage);
            }
        }
    }
}
