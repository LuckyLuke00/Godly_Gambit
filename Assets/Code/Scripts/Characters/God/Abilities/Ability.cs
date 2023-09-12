using System.Collections;
using UnityEngine;

namespace GodlyGambit
{
    public abstract class Ability : MonoBehaviour
    {
        [Header("Ability Info")]
        [SerializeField] private string _name = "Ability";

        [Header("Ability Settings")]
        [SerializeField] private bool _canPlaceAnyWhere = false;

        [SerializeField] private bool _canPlaceOnPlayer = false;

        [SerializeField] private float _cooldownTime = 2.5f;
        [SerializeField, Range(0f, 10f)] private float _spawnRadius = 3f;

        [Space(3)]
        [SerializeField] protected bool _useDifficultyScaling = true;

        [SerializeField, Tooltip("Per player this ammount of time that will be subtracted of the cooldown Time")]
        private float _timeReductionPerPlayer = 0.25f;

        [Header("Visual Settings")]
        [SerializeField] private Material _validMaterial = null;

        [SerializeField] private Material _ghostMaterial = null;

        [SerializeField] private Material _invalidMaterial = null;

        private float _cooldownTimer = 0f;
        [HideInInspector] private float _OldCooldownTime;

        private Transform _parent = null;
        public Transform DynamicParent { get; private set; }

        // Getter for _canPlaceOnPlayer
        public bool CanPlaceOnPlayer
        {
            get { return _canPlaceOnPlayer; }
        }

        public bool CanPlaceAnyWhere
        {
            get { return _canPlaceAnyWhere; }
        }

        protected bool _hasCooledDown = true;

        public bool HasCooledDown
        {
            get { return _hasCooledDown; }
            set
            {
                // If the value is the same as the current value, do nothing
                if (_hasCooledDown == value) return;
                _hasCooledDown = value;

                OnUpdateMaterial?.Invoke();
            }
        }

        private bool _canPlace = false;

        public delegate void UpdateMaterialEvent();

        public event UpdateMaterialEvent OnUpdateMaterial;

        public bool CanPlace
        {
            get { return _canPlace; }
            set
            {
                // If the value is the same as the current value, do nothing
                if (_canPlace == value) return;
                _canPlace = value;

                OnUpdateMaterial?.Invoke();
            }
        }

        protected float SpawnRadius
        {
            get => _spawnRadius;
            set => _spawnRadius = value;
        }

        protected Material ValidMaterial => _validMaterial;
        protected Material GhostMaterial => _ghostMaterial;
        protected Material InvalidMaterial => _invalidMaterial;

        protected virtual void Awake()
        {
            DynamicParent = GameObject.Find("_Dynamic").transform;
            _OldCooldownTime = _cooldownTime;
            IsActive = false;
        }

        private void OnEnable()
        {
            PlayerManager.OnNewPlayerJoined += DifficultyScaling;
            HealthComponent.OnLivesChanged += DifficultyScalingSoldiers;
        }

        private void OnDisable()
        {
            PlayerManager.OnNewPlayerJoined -= DifficultyScaling;
            HealthComponent.OnLivesChanged -= DifficultyScalingSoldiers;
        }

        protected virtual void Update()
        {
            HasCooledDown = _cooldownTimer <= 0f;
            if (!HasCooledDown)
            {
                _cooldownTimer -= Time.deltaTime;
            }
            else
            {
                ResetCooldownTimer();
            }
        }

        public void SetParent(Transform parent)
        {
            if (_parent != null)
            {
                Debug.LogError("Parent already set");
                return;
            }

            // Create a new gameobject to hold the abilities, and attach it to the parent
            GameObject abilityHolder = new GameObject($"Ability: {_name}");
            abilityHolder.transform.SetParent(parent);

            _parent = abilityHolder.transform;
            _parent.position = parent.position;
        }

        public bool IsActive { get; set; }

        public abstract void Toggle();

        public abstract void DeactivateGhosts();

        public abstract void Place();

        public Transform GetParent() => _parent;

        protected void StartCoolDownTimer() => _cooldownTimer = _cooldownTime;

        private void ResetCooldownTimer() => _cooldownTimer = 0f;

        protected void DifficultyScaling()
        {
            StartCoroutine(DelayedDifficultyScaling());
        }

        private void DifficultyScalingSoldiers(GameObject sender, HealthComponent healthComponent)
        {
            StartCoroutine(DelayedDifficultyScaling());
        }

        private IEnumerator DelayedDifficultyScaling()
        {
            yield return null; // Wait for the current frame to finish

            if (_useDifficultyScaling)
            {
                var playerManager = FindObjectOfType<PlayerManager>();
               
                _cooldownTime = _OldCooldownTime - (_timeReductionPerPlayer * (playerManager.AliveFighters.Count - 1));
            }

            OnUpdateMaterial?.Invoke();
        }
    }
}
