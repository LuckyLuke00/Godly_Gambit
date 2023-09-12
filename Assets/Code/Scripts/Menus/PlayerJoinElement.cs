using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class PlayerJoinElement : MonoBehaviour
    {
        [SerializeField] private GameObject _joinTextElement = null;
        [SerializeField] private Image _playerImage = null;
        [SerializeField] private Image _playerTextBackground = null;
        [SerializeField] private TextMeshProUGUI _playerName = null;

        [Header("Light Element")]
        [SerializeField] private float _fadeTime = .5f;

        [SerializeField] private Image _playerLightElement = null;
        [SerializeField] private Sprite[] PlayerLightVariants = null;

        [Header("Fighter Images: Same order as in PlayerManager")]
        [SerializeField] private Sprite[] _fighterImageVariants = null;

        private void Awake()
        {
            _playerImage.enabled = false;
            _playerLightElement.enabled = false;
            _playerName.enabled = false;
            _playerTextBackground.enabled = false;
        }

        public void ShowPlayerImage(int playerIndex)
        {
            // Set the player image
            _playerImage.sprite = _fighterImageVariants[PlayerManager.Instance.MaterialVariantIndices[playerIndex]];

            // Set the player name by replacing the number with the player index
            _playerName.text = _playerName.text.Replace("1", (playerIndex + 1).ToString());

            // Set the light element
            _playerLightElement.sprite = PlayerLightVariants[PlayerManager.Instance.MaterialVariantIndices[playerIndex]];

            // Disable the join text element
            _joinTextElement.SetActive(false);

            // Enable the player image
            _playerImage.enabled = true;
            _playerName.enabled = true;
            _playerTextBackground.enabled = true;

            FadeLight();
        }
        public void FadeLight()
        {
            if (!_playerLightElement || PlayerLightVariants.Length < 1) return;

            _playerLightElement.enabled = true;

            // Interrupt the coroutine if it's already running
            StopAllCoroutines();

            // Fade in with a coroutine, passing the current alpha value
            StartCoroutine(FadeInAndOutLight(_playerLightElement.color.a));
        }

        // Fade in the light element
        private System.Collections.IEnumerator FadeInAndOutLight(float currentAlpha)
        {
            // Fade in the image alpha over time from the current alpha value
            for (float i = currentAlpha; i <= 1f; i += Time.deltaTime / _fadeTime)
            {
                _playerLightElement.color = new Color(_playerLightElement.color.r, _playerLightElement.color.g, _playerLightElement.color.b, i);
                yield return null;
            }

            // Fade out the image alpha over time
            for (float i = 1f; i >= 0f; i -= Time.deltaTime / _fadeTime)
            {
                _playerLightElement.color = new Color(_playerLightElement.color.r, _playerLightElement.color.g, _playerLightElement.color.b, i);
                yield return null;
            }

            _playerLightElement.enabled = false;
        }

    }
}
