using UnityEngine;
using UnityEngine.InputSystem;

namespace GodlyGambit
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private Vector2 _startPos = Vector2.zero;

        [Header("Collision Settings")]
        [SerializeField] private float _groundCheckRadius = 1f;

        [SerializeField] private float _validRadius = 3f;
        [SerializeField] private LayerMask _groundLayer = default;
        [SerializeField] private LayerMask _wallLayer = default;
        [SerializeField] private LayerMask _ignoreValidityLayer = default;

        [Header("Visual Settings")]
        [SerializeField] private bool _disableLightIfInvalid = true;

        [SerializeField] private bool _hideWhenUngrounded = true;
        [SerializeField] private Color[] _cursorLightColors = null;

        [Header("Inner Circle")]
        [SerializeField] private Sprite _invalidInnerSprite = null;

        [SerializeField] private Sprite[] _validInnerVariations = null;

        [Header("Outter Circle")]
        [SerializeField] private Sprite _invalidOutterSprite = null;

        [SerializeField] private Sprite[] _validOutterVariations = null;

        private Color _cursorLightColor = Color.white;
        private GodCursorHelper _cursorHelper = null;
        private Sprite _validInnerSprite = null;
        private Sprite _validOutterSprite = null;

        public bool IgnorePlayer { get; set; } = false;
        public bool CanPlaceAnywhere { get; set; } = false;
        private bool _isGamepad = false;
        private bool _isPositionValid = false;

        private Vector2 _inputVector = Vector2.zero;
        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _lastValidPos = Vector3.zero;

        public Vector3 Position => transform.position;

        public bool IsPositionValid
        {
            get
            {
                return _isPositionValid;
            }
            private set
            {
                // If the value is the same as the current value, do nothing
                if (_isPositionValid == value) return;
                _isPositionValid = value;
                UpdateMaterial();
            }
        }

        public float ValidRadius => _validRadius;

        private void Awake()
        {
            _isGamepad = GetComponentInParent<PlayerInput>()?.currentControlScheme == "Gamepad";

            _cursorHelper = GetComponentInChildren<GodCursorHelper>();
            if (!_cursorHelper) Debug.LogError("CursorController: No GodCursorHelper found!");

            SetupCursor();
        }

        private void Start()
        {
            int variantIndex = PlayerManager.Instance.MaterialVariantIndices[PlayerManager.Instance.GodIndex];
            _validInnerSprite = _validInnerVariations[variantIndex];
            _validOutterSprite = _validOutterVariations[variantIndex];
            _cursorLightColor = _cursorLightColors[variantIndex];
        }

        private void Update()
        {
            if (_isGamepad)
            {
                HandleGamepadMovement();
            }
            else
            {
                HandleMouseMovement();
            }

            CheckPosition();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsPositionValid ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _validRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _groundCheckRadius);
        }

        private void UpdateMaterial()
        {
            _cursorHelper.SetInnerCircleSprite(IsPositionValid ? _validInnerSprite : _invalidInnerSprite);
            _cursorHelper.SetOutterCircleSprite(IsPositionValid ? _validOutterSprite : _invalidOutterSprite);
            _cursorHelper.SetLightColor(_cursorLightColor);
            if (_disableLightIfInvalid) _cursorHelper.EnableLight(IsPositionValid);
        }

        private void SetupCursor()
        {
            // Make the ray start at the top of the ground check sphere
            Vector3 origin = new Vector3(_startPos.x, _groundCheckRadius, _startPos.y);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _groundCheckRadius * 2f, _groundLayer))
            {
                transform.position = hit.point;
                _groundNormal = hit.normal;
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                _lastValidPos = transform.position;
            }
        }

        private void CheckPosition()
        {
            if (CanPlaceAnywhere)
            {
                IsPositionValid = true;
                return;
            }

            // If ignore player is true, ignore the player layer
            LayerMask mask = _groundLayer | _ignoreValidityLayer | (IgnorePlayer ? LayerMask.GetMask("Fighter") : 0);
            IsPositionValid = !Physics.CheckSphere(transform.position, _validRadius, ~mask);
        }

        private void HandleGamepadMovement()
        {
            // Move the curor and check if the new position is valid
            Vector3 origin = transform.position + GetAdjustedDirection();
            IsPositionValid = MoveToGround(origin);
        }

        private void HandleMouseMovement()
        {
            // Make the cursor follow the mouse over the ground
            Ray ray = Camera.main.ScreenPointToRay(_inputVector);
            if (Physics.Raycast(ray, out RaycastHit hit, Camera.main.farClipPlane, _groundLayer))
            {
                transform.position = hit.point;

                // Make the cursor parallel to the normal
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                IsPositionValid = true;
                _cursorHelper.EnableCircles();
                return;
            }

            IsPositionValid = false;
            if (_hideWhenUngrounded) _cursorHelper.DisableCircles();
        }

        private bool MoveToGround(Vector3 origin)
        {
            return PerformSphereCast(origin);
        }

        private Vector3 GetAdjustedDirection()
        {
            if (!Camera.main) return Vector3.zero;

            // Calculate the camera-relative direction to move based on the input
            Vector3 moveDirection = GetCameraForward() * _inputVector.y + GetCameraRight() * _inputVector.x;

            // Project the input vector onto the plane that is perpendicular to the ground normal
            moveDirection -= Vector3.Dot(moveDirection, _groundNormal) * _groundNormal;

            // Calculate the movement speed based on the projected input vector
            return moveDirection.normalized * _speed * Time.deltaTime;
        }

        private Vector3 GetCameraForward()
        {
            return Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        }

        private Vector3 GetCameraRight()
        {
            return Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
        }

        private bool PerformSphereCast(Vector3 origin)
        {
            origin.y += _groundCheckRadius * 2f;

            if (Physics.SphereCast(origin, _groundCheckRadius, Vector3.down, out RaycastHit hit, Mathf.Infinity, _groundLayer))
            {
                // Move the hitpoint to the origin accordingly
                // Calculate offset from object's origin to hit point in the x and z directions
                Vector3 horizontalOffset = new Vector3(hit.point.x - origin.x, 0f, hit.point.z - origin.z);

                if (Physics.SphereCast(origin, _groundCheckRadius * .5f, GetAdjustedDirection(), out RaycastHit wallHit, _groundCheckRadius, _wallLayer))
                {
                    // Push the cursor away from the wall
                    // Calculate the distance from the wall
                    Vector3 pos = wallHit.point;
                    pos.y = hit.point.y;

                    float distance = Vector3.Distance(pos, origin);

                    // Calculate the opposite direction from the wall and the origin
                    Vector3 dir = (wallHit.point - transform.position).normalized;
                    pos = hit.point - dir * distance;
                    pos.y = hit.point.y;
                    transform.position = pos;

                    _lastValidPos = transform.position;
                    return true;
                }

                if (Physics.Raycast(hit.point - horizontalOffset, Vector3.up, out RaycastHit newHit, Mathf.Infinity, _groundLayer))
                {
                    Vector3 pos = newHit.point;
                    transform.position = pos;
                    _groundNormal = newHit.normal;
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, newHit.normal);

                    _lastValidPos = transform.position;
                    return true;
                }
                else
                {
                    transform.position = hit.point;
                    _groundNormal = hit.normal;
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    _lastValidPos = transform.position;
                    return true;
                }
            }

            transform.position = _lastValidPos;
            return false;
        }

        private void OnCursorMove(InputValue value) => _inputVector = value.Get<Vector2>();
    }
}
