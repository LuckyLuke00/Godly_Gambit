using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GodlyGambit
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] private Audio _music = null;
        [SerializeField] private Audio[] _effects = null;

        private void Start()
        {
            AudioManager.PlayMusic(_music, 5f);
            AudioManager.PlaySound(_effects, 2.5f);
        }

        private void Update()
        {
            bool loadNextScene = false;
            if (Keyboard.current != null)
            {
                loadNextScene |= Keyboard.current.anyKey.wasPressedThisFrame;
                if (loadNextScene) LoadNextScene();
            }

            if (Mouse.current != null)
            {
                loadNextScene |= Mouse.current.leftButton.wasPressedThisFrame;
                loadNextScene |= Mouse.current.rightButton.wasPressedThisFrame;
                loadNextScene |= Mouse.current.middleButton.wasPressedThisFrame;

                if (loadNextScene) LoadNextScene();
            }

            if (Gamepad.current != null)
            {
                loadNextScene |= Gamepad.current.buttonSouth.wasPressedThisFrame;
                loadNextScene |= Gamepad.current.buttonNorth.wasPressedThisFrame;
                loadNextScene |= Gamepad.current.buttonEast.wasPressedThisFrame;
                loadNextScene |= Gamepad.current.buttonWest.wasPressedThisFrame;

                loadNextScene |= Gamepad.current.startButton.wasPressedThisFrame;
                loadNextScene |= Gamepad.current.selectButton.wasPressedThisFrame;

                if (loadNextScene) LoadNextScene();
            }
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
