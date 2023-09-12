using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GodlyGambit
{
    [RequireComponent(typeof(CharacterController))]
    public class FighterController : MonoBehaviour
    {
        [SerializeField] private float _speed = 6f;
        [SerializeField, Range(0f, 1f)] private float _backwardSpeedMultiplier = 0.8f;

        [SerializeField] private GameObject _fighterCircle = null;
        [SerializeField] private GameObject _deathEffectPrefab = null;

        [Header("Dash Options")]
        [SerializeField, Tooltip("Duration of the dash"), Range(0f, float.MaxValue)] private float _dashTime = 0.25f;

        [SerializeField] private GameObject _dashEffect = null;
        [SerializeField] private float _dashEffectDuration = 2f;

        [SerializeField, Tooltip("Speed of the dash"), Range(0f, float.MaxValue)] private float _dashSpeed = 20f;
        [SerializeField, Range(0f, float.MaxValue)] private float _dashCooldownTime = 5.0f;
        [SerializeField] private bool _IgnoreEnemiesWhileDashing = true;

        [SerializeField] private AudioSource _dashSound;

        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        [SerializeField, Tooltip("If you stand still use the aim direction to dash")]
        private bool _dashUseLookDirectionIfStandingStill = false;

        [Header("Mouse Options")]
        [SerializeField] private bool _smoothMouseRotation = true;

        [SerializeField] private float _mouseRotationSpeed = 45f;

        [Header("Gamepad Options")]
        [SerializeField] private bool _enableStickFire = false;

        [SerializeField] private bool _faceMoveDirection = true;
        [SerializeField] private float _gamepadRotationSpeed = 15f;

        [Header("Effects")]
        [SerializeField] private GameObject _runEffect = null;

        [SerializeField] private AudioSource _DeathSound;

        private bool _dashHasCooledDown = false;
        private bool _isDashing = false;
        private bool _isFiring = false;
        private bool _isGamepad = false;
        private bool _isMoving = false;

        private Camera _camera = null;
        private CharacterController _characterController = null;

        private float _dashCooldownTimer = 0f;
        private float _dashStartTime;

        private MeshRenderer _circleRenderer = null;
        private ParticleSystem _runEffectParticles = null;
        private ShootingBehaviour _shootingBehaviour = null;
        private HealthComponent _healthComponent = null;

        private Vector2 _inputVector = Vector2.zero;
        private Vector2 _lookVector = Vector2.zero;
        private Vector3 _lastValidmoveDirection = Vector3.zero;
        private Vector3 _moveDirection = Vector3.zero;

        private FighterDashCircle _fighterDashCircle = null;

        private GameObject _currectDeathEffect = null;

        public bool IsDashing => _isDashing;
        public float DashCooldown => _dashCooldownTime;
        public float DashCooldownTimer => _dashCooldownTimer;

        #region Animation

        private Animator _figtherAnimator = null;

        #endregion Animation

        public delegate void FighterDied(GameObject fighter);

        public static event FighterDied OnFighterDied;

        private void Awake()
        {
            // Get the CharacterController component
            _characterController = GetComponent<CharacterController>();

            // Get the main camera
            _camera = Camera.main;

            // If the camera is null, log an error
            if (!_camera)
            {
                Debug.LogError("FighterController: No camera found! Make sure the scene contains a camera tagged as MainCamera.");
            }

            _shootingBehaviour = GetComponent<ShootingBehaviour>();
            _isGamepad = GetComponentInParent<PlayerInput>().currentControlScheme == "Gamepad";

            _figtherAnimator = GetComponentInChildren<Animator>();

            if (!_figtherAnimator)
            {
                Debug.LogWarning("No animator found on the fighter");
            }

            if (!_runEffect)
            {
                Debug.LogWarning("No run effect assigned to the fighter controller");
            }
            else
            {
                _runEffect = Instantiate(_runEffect, transform);
                _runEffect.SetActive(true);
                _runEffectParticles = _runEffect.GetComponent<ParticleSystem>();
                _runEffectParticles?.Stop();
            }

            _fighterDashCircle = GetComponentInChildren<FighterDashCircle>();
            if (!_fighterDashCircle)
            {
                Debug.LogWarning("No FighterDashCircle found on the fighter");
            }

            // Try to get the health component
            _healthComponent = GetComponent<HealthComponent>();
            if (!_healthComponent)
            {
                Debug.LogWarning("No health component found on the fighter");
            }

            _circleRenderer = _fighterCircle.GetComponent<MeshRenderer>();
            if (!_circleRenderer)
            {
                Debug.LogWarning("No circle renderer found on the fighter");
            }

            if (!_deathEffectPrefab)
            {
                Debug.LogWarning("No death effect found on the fighter");
            }

            _lastValidmoveDirection = transform.forward;
        }

        private void Update()
        {
            if (!_healthComponent.IsDead)
            {
                HandleMovement();
                HandleRotation();

                if (_isFiring)
                {
                    HandleFire();
                }

                if (_isDashing)
                {
                    HandelDash();
                }

                HandleDashCooldown();
                HandleRunEffect();
            }

            HandleAnimations();
        }

        private void OnEnable()
        {
            _healthComponent.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _healthComponent.OnDeath -= HandleDeath;
        }

        private void HandleMovement()
        {
            if (!_camera) return;

            // Calculate the camera-relative direction to move based on the input
            _moveDirection = (GetCameraForward() * _inputVector.y + GetCameraRight() * _inputVector.x);

            float dotProduct = Vector3.Dot(_moveDirection.normalized, _lookVector.normalized);

            float angle = Vector3.Angle(_moveDirection, _lookVector);

            // Determine the speed multiplier based on the angle and movement direction
            float speedMultiplier = (dotProduct < 0 && angle > 90f) ? _backwardSpeedMultiplier : 1f;

            float modifiedSpeed = _speed * speedMultiplier;

            _moveDirection *= modifiedSpeed;

            // Move the character (SimpleMove is framerate independant)
            _characterController.SimpleMove(_moveDirection);
            _isMoving = _characterController.velocity.magnitude > 0.1f;

            if (_moveDirection != Vector3.zero)
            {
                _lastValidmoveDirection = _moveDirection.normalized;
            }
            else if (_moveDirection == Vector3.zero && _dashUseLookDirectionIfStandingStill)
            {
                _lastValidmoveDirection = transform.forward.normalized;
            }
        }

        private void HandleRotation()
        {
            if (_lookVector == Vector2.zero)
            {
                if (_isGamepad && _faceMoveDirection)
                {
                    FaceMoveDirection();
                }

                return;
            }

            FaceLookDirection();
        }

        private void HandleAnimations()
        {
            //Make sure this function only runs when the animator is used/called
            if (!_figtherAnimator) return;

            //Set the IsRunning boolean equal to the _isMoving input
            _figtherAnimator.SetBool("IsRunning", _isMoving);

            //Check if character isn't moving to make sure this doesn't run when the character IS moving
            //Set the boolean for the firing animation
            _figtherAnimator.SetBool("IsIdleFiring", !_isMoving && _isFiring);

            Vector3 movement = transform.InverseTransformDirection(_inputVector.x, 0, _inputVector.y);

            //Set the local variables for velocity in the 2D movement plane to control the blend tree
            _figtherAnimator.SetFloat("VelocityX", movement.x);
            _figtherAnimator.SetFloat("VelocityZ", movement.z);
        }

        private void HandleFire()
        {
            if (!_shootingBehaviour) return;
            _shootingBehaviour.FireWeapon();
        }

        private void HandleRunEffect()
        {
            if (!_runEffect) return;

            // If the player is not moving, disable the run effect and return
            if (_inputVector == Vector2.zero)
            {
                _runEffectParticles.Stop();
                return;
            }

            // If the run effect is not playing, play it
            if (!_runEffectParticles.isPlaying)
            {
                _runEffectParticles.Play();
            }
        }

        private void FaceLookDirection()
        {
            if (_lookVector == Vector2.zero) return;

            if (_isGamepad)
            {
                HandleGamepadRotation();
                return;
            }

            HandleMouseRotation();
        }

        private void HandleGamepadRotation()
        {
            LookAt(new Vector3(_lookVector.x, .0f, _lookVector.y), _gamepadRotationSpeed);
        }

        private void HandleMouseRotation()
        {
            // Handle mouse input
            if (!_camera) return;

            Ray ray = _camera.ScreenPointToRay(_lookVector);
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 direction = Vector3.ProjectOnPlane(ray.GetPoint(distance) - transform.position, Vector3.up);

                if (_smoothMouseRotation)
                {
                    LookAt(direction, _mouseRotationSpeed);
                    return;
                }

                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private void FaceMoveDirection()
        {
            Vector3 lookDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.z);

            // If the player not moving, don't rotate
            if (lookDirection == Vector3.zero) return;

            // Get the direction the player is moving, but only on the XZ plane
            LookAt(lookDirection, _gamepadRotationSpeed);
        }

        private void LookAt(Vector3 target, float rotationSpeed)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target), rotationSpeed * Time.deltaTime);
        }

        private Vector3 GetCameraForward()
        {
            return Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
        }

        private Vector3 GetCameraRight()
        {
            return Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
        }

        public void Kill()
        {
            OnFighterDied?.Invoke(gameObject);
            Destroy(gameObject);
        }

        private void OnMove(InputValue value) => _inputVector = value.Get<Vector2>();

        private void OnLook(InputValue value) => _lookVector = value.Get<Vector2>();

        private void OnFire(InputValue value) => _isFiring = value.isPressed;

        private void OnStickFire(InputValue value)
        {
            if (!_enableStickFire) return;

            _isFiring = value.Get<Vector2>().sqrMagnitude > 0f;
        }

        private void OnDash()
        {
            if (!_isDashing && _dashHasCooledDown)
            {
                _isDashing = true;
                _dashStartTime = Time.time;
                _dashCooldownTimer = _dashCooldownTime;
                _figtherAnimator.SetTrigger("triDash");
                Destroy(Instantiate(_dashEffect, transform.position, transform.rotation, transform), _dashEffectDuration);
            }
        }

        private void HandelDash()
        {
            bool isEnemyInList = false;
            GameObject[] enemies = { };
            if (_IgnoreEnemiesWhileDashing)
            {
                enemies = GameObject.FindGameObjectsWithTag("Enemy");
                isEnemyInList = enemies.Length > 0;
            }

            if (Time.time < _dashStartTime + _dashTime)
            {
                _characterController.Move(_lastValidmoveDirection * _dashSpeed * Time.deltaTime);

                if (isEnemyInList)
                {
                    foreach (GameObject enemy in enemies)
                    {
                        Physics.IgnoreCollision(GetComponent<Collider>(), enemy.GetComponent<Collider>(), true);
                    }
                }
            }
            else
            {
                _isDashing = false;
                _dashStartTime = 0;
                if (isEnemyInList)
                {
                    foreach (GameObject enemy in enemies)
                    {
                        Physics.IgnoreCollision(GetComponent<Collider>(), enemy.GetComponent<Collider>(), false);
                    }
                }
            }

            if (_dashSound && !_dashSound.isPlaying)
            {
                float randomPitch = UnityEngine.Random.Range(_minPitch, _maxPitch);
                _dashSound.pitch = randomPitch;

                _dashSound.Play();
            }
        }

        private void HandleDashCooldown()
        {
            _dashHasCooledDown = _dashCooldownTimer <= 0f;
            if (!_dashHasCooledDown)
            {
                _dashCooldownTimer -= Time.deltaTime;
                _fighterDashCircle?.SetDashCirclePercent(_dashCooldownTimer / _dashCooldownTime);
            }
            else
            {
                _dashCooldownTimer = 0f;
                _fighterDashCircle?.SetDashCirclePercent(1f);
            }
        }

        public void SetCircleColor(Material material)
        {
            if (!_circleRenderer) return;

            _circleRenderer.material = material;
        }

        private void HandleDeath()
        {
            _fighterCircle.SetActive(false);
            _figtherAnimator.SetBool("IsDead", true);
            PlayerManager.Instance.AddDeadFighter(gameObject);
            _characterController.enabled = false;

            if (_healthComponent.CurrentLives < 0)
            {
                GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                return;
            }

            _healthComponent.SimpleKill();

            _currectDeathEffect = Instantiate(_deathEffectPrefab, transform.position, transform.rotation);
            var deathEffectController = _currectDeathEffect.GetComponent<DeathEffectController>();
            deathEffectController.SetDeadObject(gameObject);

            // Subscribe to the death effect's OnDeathEffectFinished event
            deathEffectController.OnDeathEffectComplete += DeathEffectComplete;

            _DeathSound?.Play();
        }

        private void DeathEffectComplete()
        {
            _currectDeathEffect.GetComponent<DeathEffectController>().OnDeathEffectComplete -= DeathEffectComplete;
            Destroy(_currectDeathEffect);

            _healthComponent.SimpleKillEvents();
        }

        public void ResetFighter()
        {
            _fighterCircle.SetActive(true);
            _characterController.enabled = true;
            _figtherAnimator.SetBool("IsDead", false);
            _healthComponent.ResetHealth();
        }
    }
}
