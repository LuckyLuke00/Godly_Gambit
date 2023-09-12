using UnityEngine;
using UnityEngine.InputSystem;

namespace GodlyGambit
{
    public class RejoinHelper : MonoBehaviour
    {
        private PlayerInput _playerInput = null;

        public delegate void PlayerRejoin(int playerIndex);

        public static event PlayerRejoin OnPlayerRejoin;

        private void Awake()
        {
            if (!TryGetComponent(out _playerInput))
            {
                Debug.LogError("PlayerInput component not found on " + gameObject.name);
            }
        }

        // Subscribe to the ReJoin action
        private void OnRejoin()
        {
            // Invoke the event
            OnPlayerRejoin?.Invoke(_playerInput.playerIndex);
        }
    }
}
