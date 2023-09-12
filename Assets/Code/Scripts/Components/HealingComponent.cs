using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class HealingComponent : MonoBehaviour
    {
        #region Variables
        private const string FRIENDLY_TAG = "Player";
        private const string ENEMY_TAG = "Enemy";

        [SerializeField] private bool _destroyOnHit = true;
        [SerializeField] private bool _ignoreEnemies = true;
        [SerializeField] private bool _ignoreFriendlies = false;
        [SerializeField] private uint _healing = 10;

        #endregion Variables

        private void OnTriggerEnter(Collider other)
        {
            if (_ignoreFriendlies && other.CompareTag(FRIENDLY_TAG)) return;
            if (_ignoreEnemies && other.CompareTag(ENEMY_TAG)) return;

            if (!other.TryGetComponent(out HealthComponent healthComponent)) return;
            healthComponent.Heal(_healing);

            if (_destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
