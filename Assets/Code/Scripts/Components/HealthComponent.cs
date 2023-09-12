using System;
using UnityEngine;

namespace GodlyGambit
{
    public class HealthComponent : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool _killImmediately = true;
        [SerializeField] private bool _destroyOnDeath = true;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _maxLives = 3;
        [SerializeField] private int _currentLives = 0;

        [Header("Effects")]
        [SerializeField] private bool _playDamageEffect = true;

        [SerializeField] private ParticleSystem _damageEffect = null;

        [Header("Sound settings")]
        [SerializeField] private AudioSource _hitSound;

        [SerializeField] private AudioSource _healSound;

        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        private const string FRIENDLY_TAG = "Fighter";
        private const string ENEMY_TAG = "Enemy";

        private bool _effectPlayed = false;

        #endregion Variables

        #region Events

        public delegate void FighterLostLiveEvent(GameObject fighter, HealthComponent healthComponent);

        public static event FighterLostLiveEvent OnFighterLostLive;

        public delegate void DiedEvent();

        public event DiedEvent OnDeath;

        // Event that gets called when the health of the fighter changes, also sends the fighter that changed
        public delegate void HealthChangedEvent(GameObject sender, HealthComponent healthComponent);

        public static event HealthChangedEvent OnHealthChanged;

        public delegate void FighterDied();

        public static event FighterDied OnFighterDied;

        public delegate void LivesChangedEvent(GameObject sender, HealthComponent healthComponent);

        public static event LivesChangedEvent OnLivesChanged;

        #endregion Events

        #region Properties

        public int CurrentLives
        {
            get { return _currentLives; }
            set
            {
                _currentLives = value;
                OnLivesChanged?.Invoke(gameObject, this);
            }
        }

        public int VisualLives
        {
            get { return (_currentLives + 1); }
        }

        public bool IsDead { get; set; } = false;

        public float HealthPercentage
        {
            get
            {
                return ((float)CurrentHealth) / _maxHealth;
            }
        }

        public int CurrentHealth { get; set; } = 0;

        #endregion Properties

        private void Awake() //on start set all max values to the current one.
        {
            CurrentHealth = _maxHealth;
            _currentLives = _maxLives;
        }

        private void Start()
        {
            if (_playDamageEffect && _damageEffect == null)
            {
                _playDamageEffect = false;
                Debug.LogWarning("No damage effect set on " + gameObject.name);
            }
        }

        public void CopyHealthValues(HealthComponent healthComponent)
        {
            _maxHealth = healthComponent._maxHealth;
            _maxLives = healthComponent._maxLives;
            _currentLives = healthComponent._currentLives;
            _playDamageEffect = healthComponent._playDamageEffect;
            _damageEffect = healthComponent._damageEffect;
        }

        public void Damage(uint amount) //uint to only have positive values so this function only can damage the parrent
        {
            if (IsDead) return;

            CurrentHealth -= Convert.ToInt32(amount);
            OnHealthChanged?.Invoke(gameObject, this);

            if (_hitSound)
            {
                float randomPitch = UnityEngine.Random.Range(_minPitch, _maxPitch);
                _hitSound.pitch = randomPitch;

                _hitSound.Play();
            }

            if (_playDamageEffect && !_effectPlayed)
            {
                var pos = gameObject.transform.position;
                pos.y += 1;
                Destroy(Instantiate(_damageEffect).gameObject, _damageEffect.main.startLifetime.constant);
            }
            _effectPlayed = false;

            // Is here so we can still check if the fighter is dead after the damage is applied
            // In the event that we don't want to kill the fighter immediately (mainly for animations)
            IsDead = CurrentHealth <= 0;
            if (IsDead)
            {
                OnDeath?.Invoke();
                return;
            }

            if (_killImmediately) Kill();
        }

        public void Damage(uint amount, Vector3 damagePoint, Vector3 hitDirection)
        {
            if (_playDamageEffect)
            {
                Destroy(Instantiate(_damageEffect, damagePoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)).gameObject, _damageEffect.main.startLifetime.constant);
                _effectPlayed = true;
            }

            Damage(amount);
        }

        public void Heal(uint amount) //uint to only have positive values so this function only can heal the parrent
        {
            CurrentHealth += Convert.ToInt32(amount);

            _healSound?.Play();

            if (CurrentHealth >= _maxHealth) // check if the heal didn't overheal the parrent
            {
                CurrentHealth = _maxHealth;
            }

            OnHealthChanged?.Invoke(gameObject, this);
        }

        public void Kill()
        {
            if (tag == ENEMY_TAG && _destroyOnDeath) //bypass life system to have better performance enemies do not need lives.
            {
                Destroy(gameObject);
            }

            if (tag == FRIENDLY_TAG)
            {
                IsDead = true;

                if (CurrentLives > 0)
                {
                    CurrentHealth = _maxHealth;
                    --CurrentLives;

                    OnHealthChanged?.Invoke(gameObject, this);
                    OnFighterLostLive?.Invoke(gameObject, this);
                }
                else
                {
                    CurrentHealth = 0;
                    --CurrentLives;

                    OnFighterDied?.Invoke();
                    if (_destroyOnDeath) Destroy(gameObject);
                }
            }
        }

        // Without invoking events
        public void SimpleKill()
        {
            if (tag == FRIENDLY_TAG)
            {
                IsDead = true;

                if (CurrentLives > 0)
                {
                    CurrentHealth = _maxHealth;
                    --CurrentLives;
                }
                else
                {
                    CurrentHealth = 0;
                    --CurrentLives;
                }
            }
        }

        // Just invoking events
        public void SimpleKillEvents()
        {
            if (tag == FRIENDLY_TAG)
            {
                if (CurrentLives > -1)
                {
                    OnHealthChanged?.Invoke(gameObject, this);
                    OnFighterLostLive?.Invoke(gameObject, this);
                }
                else
                {
                    OnFighterDied?.Invoke();
                    OnDeath?.Invoke();
                    if (_destroyOnDeath) Destroy(gameObject);
                }
            }
        }

        public void ResetHealth(bool resetLives = true)// reset the health and lives if the bool is true
        {
            IsDead = false;
            CurrentHealth = _maxHealth;
            if (resetLives)
            {
                CurrentLives = _maxLives;
            }

            OnHealthChanged?.Invoke(gameObject, this);
        }
    }
}
