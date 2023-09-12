using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class SpawnSoldiersAbility : Ability
    {
        [Header("Soldier Settings")]
        [SerializeField, Range(0, 50)] private int _maxSoldiers = 10;
        [SerializeField] private GameObject _soldierSpawnEffect = null;
        [SerializeField] private float _soldierSpawnEffectDuration = 1f;

        [Space(3)]
        [SerializeField] private int _extraSoldiersPerPlayer = 2;

        [SerializeField] private float _extraSoldiersSpawnRadius = 0.1f;

        [Space(5)]
        [SerializeField] private GameObject _soldierPrefab = null;

        [SerializeField] private GameObject _soldierGhostPrefab = null;

        private readonly List<GameObject> _ghosts = new List<GameObject>();
        private readonly List<MeshRenderer> _renderers = new List<MeshRenderer>();

        private int _lastMaxSoldiers = 0;
        private float _lastSpawnRadius = 0f;

        [HideInInspector] private int _OldAmmountOfSoldiers;
        [HideInInspector] private float _OldSpawnRadius;

        [SerializeField] private AudioSource _spawnSound = null;

        // Event for when ability is placed
        public delegate void PlaceSoldiersEvent();

        public static event PlaceSoldiersEvent OnPlaceSoldiers;

        protected override void Awake()
        {
            base.Awake();

            if (!_soldierPrefab)
            {
                Debug.LogError("No soldier prefab assigned to SpawnSoldiersAbility");
            }

            if (!_soldierGhostPrefab)
            {
                Debug.LogError("No soldier ghost prefab assigned to SpawnSoldiersAbility");
            }

            CreateSoldiers();
            DifficultyScalingSoldiers();

            _lastMaxSoldiers = _maxSoldiers;
            _lastSpawnRadius = SpawnRadius;
            _OldAmmountOfSoldiers = _maxSoldiers;
            _OldSpawnRadius = SpawnRadius;
        }

        protected override void Update()
        {
            base.Update();

            if (IsSettingChanged()) UpdateSettings();
        }

        private void OnEnable()
        {
            OnUpdateMaterial += UpdateMaterial;
            PlayerManager.OnNewPlayerJoined += DifficultyScalingSoldiers;
            HealthComponent.OnLivesChanged += DifficultyScalingSoldiers;
        }

        private void OnDisable()
        {
            OnUpdateMaterial -= UpdateMaterial;
            PlayerManager.OnNewPlayerJoined -= DifficultyScalingSoldiers;
            HealthComponent.OnLivesChanged -= DifficultyScalingSoldiers;
        }

        public override void DeactivateGhosts()
        {
            _ghosts.ForEach(ghost => ghost.SetActive(false));
        }

        public override void Place()
        {
            if (!HasCooledDown) return;

            IsActive = false;

            CreateSoldiers();
            DeactivateGhosts();

            for (int i = 0; i < _maxSoldiers; ++i)
            {
                Instantiate(_soldierPrefab, _ghosts[i].transform.position, _ghosts[i].transform.rotation, DynamicParent);
            }

            _spawnSound?.Play();

            // Instantiate the Spawn effect
            Destroy(Instantiate(_soldierSpawnEffect, GetParent().position, GetParent().rotation, GetParent()), _soldierSpawnEffectDuration);

            OnPlaceSoldiers?.Invoke();

            StartCoolDownTimer();
        }

        public override void Toggle()
        {
            IsActive = !IsActive;

            if (IsActive)
            {
                PlaceInCircle();
            }
            else
            {
                DeactivateGhosts();
            }
        }

        private void CreateSoldiers()
        {
            int ghostAmount = _maxSoldiers - _ghosts.Count;

            if (ghostAmount <= 0)
            {
                DeactivateGhosts();
                return;
            }

            for (int i = 0; i < ghostAmount; ++i)
            {
                GameObject ghost = Instantiate(_soldierGhostPrefab, GetParent().position, GetParent().rotation, GetParent());

                _ghosts.Add(ghost);
                _renderers.Add(ghost.GetComponentInChildren<MeshRenderer>());

                ghost.SetActive(false);
            }
        }

        private void PlaceInCircle()
        {
            CreateSoldiers();

            float angle = 360f / _maxSoldiers;
            for (int i = 0; i < _maxSoldiers; ++i)
            {
                // Calculate the local position of the enemy, taking into account the parent's rotation
                Vector3 pos = GetParent().position + Quaternion.Euler(0f, angle * i, 0f) * Vector3.forward * SpawnRadius;
                Vector3 localPos = Quaternion.Euler(0f, angle * i, 0f) * Vector3.forward * SpawnRadius;
                Vector3 dir = (pos - GetParent().position).normalized;

                _ghosts[i].transform.localPosition = localPos;
                _ghosts[i].transform.localRotation = Quaternion.LookRotation(dir);

                _ghosts[i].SetActive(true);
            }
        }

        private bool IsSettingChanged()
        {
            return _lastMaxSoldiers != _maxSoldiers || _lastSpawnRadius != SpawnRadius;
        }

        private void UpdateSettings()
        {
            PlaceInCircle();

            _lastMaxSoldiers = _maxSoldiers;
            _lastSpawnRadius = SpawnRadius;

            DeactivateGhosts();
            UpdateMaterial();
            IsActive = false;
        }

        private void UpdateMaterial()
        {
            if (!HasCooledDown)
            {
                _renderers.ForEach(r => r.material = GhostMaterial);
                return;
            }

            if (!CanPlace)
            {
                _renderers.ForEach(r => r.material = InvalidMaterial);
                return;
            }

            _renderers.ForEach(r => r.material = ValidMaterial);
        }

        private void DifficultyScalingSoldiers()
        {
            StartCoroutine(DelayedDifficultyScaling());
        }

        private void DifficultyScalingSoldiers(GameObject sender, HealthComponent healthComponent)
        {
            StartCoroutine(DelayedDifficultyScaling());
        }

        private IEnumerator DelayedDifficultyScaling()
        {
            yield return null;

            if (_useDifficultyScaling)
            {
                var playerManager = FindObjectOfType<PlayerManager>();
                _maxSoldiers = _OldAmmountOfSoldiers + (_extraSoldiersPerPlayer * (playerManager.AliveFighters.Count - 1));
                SpawnRadius = _OldSpawnRadius + (_extraSoldiersSpawnRadius * (playerManager.AliveFighters.Count - 1));
                base.DifficultyScaling();
            }
        }
    }
}
