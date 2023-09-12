using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GodlyGambit
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour, INavMeshTargetSetter
    {
        [SerializeField] private float _deathAnimLength = 2f;
        [SerializeField] private float _fadeAnimLength = 2f;

        private bool _isDead = false;
        private bool _startFade = false;
        private float _fadeTimer = 0f;

        private readonly List<Material> _materials = new List<Material>();

        private Animator _animator = null;
        private DamageOverTimeComponent _damageComponent = null;
        private HealthComponent _healthComponent = null;
        private NavMeshAgent _agent = null;
        private Vector3 _target = Vector3.zero;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
            _damageComponent = GetComponentInChildren<DamageOverTimeComponent>();
            _healthComponent = GetComponent<HealthComponent>();

            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                _materials.AddRange(renderer.materials);
            }

            SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
            {
                _materials.AddRange(renderer.materials);
            }

            // If no damage component is found log an error
            if (!_damageComponent)
            {
                Debug.LogError("EnemyController: No DamageComponent found!");
            }
        }

        private void Update()
        {
            // I tried using coroutines to fade the materials but it is detramental to performance
            // In the update loop it's a lot faster
            if (_startFade)
            {
                _fadeTimer += Time.deltaTime;
                _materials.ForEach(FadeMaterial);
            }

            if (_isDead) return;

            _agent.SetDestination(_target);
            _animator?.SetBool("IsRunning", _agent.velocity.sqrMagnitude > .1f);
        }

        private void OnEnable()
        {
            _damageComponent.OnCanAttack += PlayAttackAnim;
            _healthComponent.OnDeath += Kill;
        }

        private void OnDisable()
        {
            _damageComponent.OnCanAttack += PlayAttackAnim;
            _healthComponent.OnDeath -= Kill;
        }

        private void PlayAttackAnim()
        {
            _animator?.SetTrigger("triStab");
        }

        public void Attack()
        {
            _damageComponent?.DoDamage();
        }

        public void Kill()
        {
            //_agent.SetDestination(transform.position);
            //_agent.isStopped = true;
            _isDead = true;

            // Disable the navmesh agent
            _agent.enabled = false;

            // Play the death animation
            _animator?.SetBool("IsDead", true);

            // Stop Attacking
            if (_damageComponent) Destroy(_damageComponent);

            // Disable colliders
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            _startFade = true;
            _materials.ForEach(SetMaterialFade);

            // Get the death animation length
            Destroy(gameObject, _deathAnimLength + _fadeAnimLength);
        }

        private void SetMaterialFade(Material material)
        {
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private void FadeMaterial(Material material)
        {
            // Do not start fading until the death animation is over
            if (_fadeTimer < _deathAnimLength) return;

            // Fade the material
            Color color = material.color;
            color.a = Mathf.Lerp(1f, 0f, (_fadeTimer - _deathAnimLength) / _fadeAnimLength);
            material.color = color;
        }

        public void SetTarget(Vector3 target) => _target = target;

        public Vector3 GetTarget() => _target;
    }
}
