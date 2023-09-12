using UnityEngine;

namespace GodlyGambit
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool _isGodSpawnPoint = false;

        [Header("Debug: Settings")]
        [SerializeField] private bool _toggleGroundSnapping = false;

        [SerializeField] private float _checkWidth = 1f;
        [SerializeField] private float _checkHeight = 2f;

        private Vector3 _groundPos = Vector3.zero;
        public bool IsGodSpawnPoint => _isGodSpawnPoint;

        private bool IsPositionValid()
        {
            if (!Application.isPlaying && !_toggleGroundSnapping)
            {
                return false;
            }

            if (!FindGround()) return false;

            MoveToGround();

            if (IsClipping()) return false;

            return true;
        }

        private void MoveToGround()
        {
            Vector3 position = transform.position;
            position.y = _groundPos.y;
            transform.position = position;
        }

        private bool FindGround()
        {
            RaycastHit hit;
            Vector3 origin = transform.position;
            origin.y += _checkHeight * .5f;

            if (Physics.Raycast(origin, Vector3.down, out hit, 10.0f))
            {
                Debug.DrawRay(origin, Vector3.down * hit.distance, Color.yellow);
                // Store the closest hitpoint
                _groundPos = hit.point;
                return true;
            }
            return false;
        }

        private bool IsClipping()
        {
            // Check if the cube is clipping inside other objects, provide a small ofsett in the y direction so the ground isn't detected
            Vector3 position = transform.position;
            position.y += _checkHeight * .5f + .1f;

            return Physics.CheckBox(position, new Vector3(_checkWidth, _checkHeight, _checkWidth) * .5f, transform.rotation);
        }

        private void DrawDebugCollider()
        {
            // Apply an offset so the cube's base is at the root position
            Vector3 position = Vector3.zero;
            position.y += _checkHeight * .5f;

            Gizmos.DrawWireCube(position, new Vector3(_checkWidth, _checkHeight, _checkWidth));
        }

        private void DrawDebugArrow()
        {
            Gizmos.color = Color.white;

            Vector3 position = Vector3.zero;
            position.y += _checkHeight * .5f;
            position.z += _checkWidth * .5f;
            Gizmos.DrawRay(position, Vector3.forward);
        }

        private void RotateGizmo()
        {
            Matrix4x4 rotationMatrix = new Matrix4x4();
            rotationMatrix.SetTRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
        }

        private void OnDrawGizmos()
        {
            // Change the color of the gizmo based on if the position is valid
            Gizmos.color = IsPositionValid() ? Color.green : Color.red;

            RotateGizmo();
            DrawDebugCollider();
            DrawDebugArrow();
        }

        public bool IsOccupied()
        {
            return IsClipping();
        }

        public Transform Transform
        { get { return transform; } }
    }
}
