using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GodlyGambit
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string _startGameScene = null;

        [SerializeField] private string _controlsScene = null;

        [Header("Audio")]
        [SerializeField] private Audio _music = null;

        [SerializeField] private Audio[] _effects = null;

        [SerializeField] private Audio _buttonClick = null;

        private void Start()
        {
            AudioManager.PlayMusic(_music, 5f);
            AudioManager.PlaySound(_effects, 2.5f);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                LoadPreviousScene();
                return;
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                LoadPreviousScene();
                return;
            }
        }

        public void StartGame()
        {
            LoadLobby();
        }

        private void LoadLobby()
        {
            AudioManager.PlaySound(_buttonClick, 0f);
            SceneManager.LoadScene(_startGameScene);
        }

        public void LoadControlsScene()
        {
            AudioManager.PlaySound(_buttonClick, 0f);
            SceneManager.LoadScene(_controlsScene);
        }

        private void LoadPreviousScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

        public void QuitGame()
        {
            // If we're in the editor, stop playing
            // Using preprocessor directives
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        }
    }
}
