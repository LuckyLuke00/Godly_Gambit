using UnityEngine;
using System.Collections;

namespace GodlyGambit
{
    public class SpawnMeteoriteAbility : Ability
    {
        [Header("Meteorite Settings")]
        [SerializeField] private float _Spawnheight = 10.0f;

        [SerializeField] private GameObject _meteoritePrefab = null;
        [SerializeField] private GameObject _meteoriteGhostPrefab = null;

        private GameObject _ghost;
        private MeshRenderer _renderer = new MeshRenderer();

        private bool _hasGhost = false;

        // Event for when ability is placed
        public delegate void PlaceMeteoriteEvent();
        public static event PlaceMeteoriteEvent OnPlaceMeteorite;

        protected override void Awake()
        {
            base.Awake();

            if (!_meteoritePrefab)
            {
                Debug.LogError("No meteorite prefab assigned to SpawnMeteoriteAbility");
            }

            if (!_meteoriteGhostPrefab)
            {
                Debug.LogError("No meteorite ghost prefab assigned to SpawnMeteoriteAbility");
            }

            CreateMeteor();
            DifficultyScalingMeteorite();
        }

        private void OnEnable()
        {
            OnUpdateMaterial += UpdateMaterial;
            PlayerManager.OnNewPlayerJoined += DifficultyScalingMeteorite;
            HealthComponent.OnLivesChanged += DifficultyScalingSoldiers;
        }

        private void OnDisable()
        {
            OnUpdateMaterial -= UpdateMaterial;
            PlayerManager.OnNewPlayerJoined -= DifficultyScalingMeteorite;
            HealthComponent.OnLivesChanged -= DifficultyScalingSoldiers;
        }

        public override void DeactivateGhosts()
        {
            _ghost.SetActive(false);
        }

        public override void Place()
        {
            if (!HasCooledDown) return;
            IsActive = false;

            CreateMeteor();
            DeactivateGhosts();

            Vector3 position = GetParent().position;
            position.y = _Spawnheight;

            var rotation = _meteoritePrefab.transform.rotation;

            Instantiate(_meteoritePrefab, position, rotation);
            OnPlaceMeteorite?.Invoke();

            StartCoolDownTimer();
        }

        public override void Toggle()
        {
            IsActive = !IsActive;

            if (IsActive)
            {
                _ghost.SetActive(true);
            }
            else
            {
                DeactivateGhosts();
            }
        }

        private void CreateMeteor()
        {
            if (_hasGhost)
            {
                DeactivateGhosts();
                return;
            }

            _ghost = Instantiate(_meteoriteGhostPrefab, GetParent().position, GetParent().rotation, GetParent()); ;
            _renderer = _ghost.GetComponentInChildren<MeshRenderer>();

            _ghost.SetActive(false);
            _hasGhost = true;
        }

        private void UpdateMaterial()
        {
            if (!HasCooledDown)
            {
                _renderer.material = GhostMaterial;
                return;
            }

            if (!CanPlace)
            {
                _renderer.material = InvalidMaterial;
                return;
            }

            _renderer.material = ValidMaterial;
        }

        private void DifficultyScalingMeteorite()
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
            base.DifficultyScaling();
        }
    }
}
