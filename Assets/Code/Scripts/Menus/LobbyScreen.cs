using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GodlyGambit
{
    public class LobbyScreen : MonoBehaviour
    {
        [SerializeField] private Audio _music = null;
        [SerializeField] private Audio[] _effects = null;
        [SerializeField] private GameObject[] _playerJoinUIElements = null;
        [SerializeField] private string _previousSceneName = null;

        private readonly List<PlayerJoinElement> _playerJoinElements = new List<PlayerJoinElement>();
        private PlayerManager _playerManager = null;

        private void Awake()
        {
            _playerManager = FindObjectOfType<PlayerManager>();

            if (_playerJoinUIElements.Length < 1)
            {
                Debug.LogError("No PlayerJoinElements found!");
                return;
            }

            foreach (GameObject playerJoinElement in _playerJoinUIElements)
            {
                _playerJoinElements.Add(playerJoinElement.GetComponent<PlayerJoinElement>());
            }
        }

        private void Start()
        {
            AudioManager.PlayMusic(_music, 5f);
            AudioManager.PlaySound(_effects, 2.5f);

            PlayerInputManager.instance.EnableJoining();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame ||
                Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                LoadPreviousScene();
                return;
            }

            if (_playerManager.PlayerCount < _playerManager.MinPlayers) return;

            bool loadNextScene = false;
            if (Keyboard.current != null)
            {
                loadNextScene |= Keyboard.current.enterKey.wasPressedThisFrame;
                loadNextScene |= Keyboard.current.spaceKey.wasPressedThisFrame;

                if (loadNextScene)
                {
                    LoadNextScene();
                    return;
                }
            }

            if (Gamepad.current != null)
            {
                loadNextScene |= Gamepad.current.startButton.wasPressedThisFrame;

                if (loadNextScene)
                {
                    LoadNextScene();
                    return;
                }
            }
        }

        private void OnEnable()
        {
            PlayerManager.OnPlayerJoin += AssignPlayerJoinElement;
            RejoinHelper.OnPlayerRejoin += FadeLight;
        }

        private void OnDisable()
        {
            PlayerManager.OnPlayerJoin -= AssignPlayerJoinElement;
            RejoinHelper.OnPlayerRejoin -= FadeLight;
        }

        private void AssignPlayerJoinElement(int playerIndex)
        {
            _playerJoinElements[playerIndex].ShowPlayerImage(playerIndex);
        }

        private void FadeLight(int playerIndex)
        {
            _playerJoinElements[playerIndex].FadeLight();
        }

        private void LoadNextScene()
        {
            AudioManager.StopMusic();
            AudioManager.StopAllSounds();

            _playerManager.CanGameStart = true;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        private void LoadPreviousScene()
        {
            // Disable joining
            PlayerInputManager.instance.DisableJoining();

            // Remove all players
            _playerManager.UnjoinAllPlayers();
            Destroy(_playerManager.gameObject);

            // Prevent a join element from displaying when returning to the previous scene
            foreach (PlayerJoinElement playerJoinElement in _playerJoinElements)
            {
                Destroy(playerJoinElement.gameObject);
            }

            SceneManager.LoadScene(_previousSceneName);
        }
    }
}
