using UnityEngine;
using UnityEngine.InputSystem;

namespace GodlyGambit
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class InputManagerHelper : MonoBehaviour
    {
        private static PlayerInputManager _playerInputManager = null;
        private static InputManagerHelper Instance { get; set; } = null;
        //private static PlayerManager PlayerManager { get; set; } = null;

        public static PlayerInputManager InputManager
        {
            get => _playerInputManager;
            private set => _playerInputManager = value;
        }

        private void Awake()
        {
            CheckForPlayerInputManager();
            DontDestroyOnLoad(Instance);
        }

        private void OnEnable()
        {
            InputManager.onPlayerJoined += AttachPlayerToSelf;
        }

        private void OnDisable()
        {
            InputManager.onPlayerJoined -= AttachPlayerToSelf;
        }

        private void AttachPlayerToSelf(PlayerInput playerInput)
        {
            playerInput.transform.SetParent(transform);
        }

        private static void CheckForPlayerInputManager()
        {
            if (!Instance)
            {
                Instance = FindObjectOfType<InputManagerHelper>();
                InputManager = Instance.GetComponent<PlayerInputManager>();
                return;
            }

            var inputManagerHelpers = FindObjectsOfType<InputManagerHelper>();
            foreach (var inputManagerHelper in inputManagerHelpers)
            {
                if (Instance == inputManagerHelper) continue;
                Destroy(inputManagerHelper.gameObject);
            }
        }
    }
}
