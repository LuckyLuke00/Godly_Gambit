using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GodlyGambit
{
    public class ControlsScreen : MonoBehaviour
    {
        [SerializeField] private string _backScene = null;
        [SerializeField] private Audio _buttonClick = null;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Back();
                return;
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                Back();
                return;
            }
        }

        public void Back()
        {
            if (_backScene == null)
            {
                Debug.LogError("Back scene not set!");
                return;
            }

            AudioManager.PlaySound(_buttonClick, 0f);
            SceneManager.LoadScene(_backScene);
        }
    }
}
