using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private List<Image> _playerFrames = new List<Image>();
        [SerializeField] private List<Image> _playerImages = new List<Image>();
        [SerializeField] private List<Image> _placesTextBackground = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> _placesText = new List<TextMeshProUGUI>();
        [SerializeField] private List<GameObject> _emberPrefabs = new List<GameObject>();
        [SerializeField] private List<Transform> _emberSockets = new List<Transform>();

        [SerializeField] private ParticleSystem _winnerExplosion = new ParticleSystem();

        [SerializeField] private List<Texture2D> _playerSourceImages = new List<Texture2D>();

        [SerializeField] private float _disableButtonTime = 1;

        [SerializeField, Range(0f, 10f)] private float _initialWaitTime = 0.25f;
        [SerializeField, Range(0f, 10f)] private float _timeBetweenFrames = 0.75f;
        [SerializeField, Range(0f, 10f)] private float _timeBetweenFramesAndButtons = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("the scale effect will happen when the apha is grater or eaqaul to this value")]
        private float _minAlphaScaleEffect = 0.5f;

        [SerializeField] private float _scaleDuration = 0.5f;
        [SerializeField] private float _maxScaleMultiplier = 1.2f;

        [SerializeField, Range(0f, 10f), Tooltip("Time it takes for all elements to fade in")]
        private float _fadeInTime = 0.5f;

        [SerializeField] private List<GameObject> _buttonsToDisable = new List<GameObject>();

        [SerializeField] private AudioSource _clickSound;
        [SerializeField] private AudioSource _playerSoundPart01;
        [SerializeField] private AudioSource _playerSoundPart02;

        [SerializeField] private Audio _endScreenMusic = null;
        [SerializeField] private Audio _victoryEffect = null;

        private float _disableTimer = 0;
        private bool _enableButton = false;

        private bool _IsScaleEffectPlaying;

        // Start is called before the first frame update
        private void Start()
        {
            foreach (var image in _playerImages)
            {
                image.enabled = false;
            }

            foreach (var frame in _playerFrames)
            {
                frame.enabled = false;
            }

            foreach (var text in _placesText)
            {
                text.enabled = false;
            }

            foreach (var background in _placesTextBackground)
            {
                background.enabled = false;
            }

            foreach (var buttons in _buttonsToDisable)
            {
                buttons.SetActive(false);
            }
            _winnerExplosion?.Stop();

            StartCoroutine(AnimateFrames());
        }

        private IEnumerator AnimateFrames()
        {
            // Create a list of indices to keep track of the original order
            List<int> indices = new List<int>();
            for (int i = 0; i < ScoreManager.GetDictionarySize(); ++i)
            {
                indices.Add(i);
            }

            // Sort indices based on lives, then time
            indices = indices.OrderByDescending(index => ScoreManager.GetPlayerLives(index)).ThenBy(index => ScoreManager.GetPlayerTime(index)).ToList();

            // Iterate through the sorted indices and update the images accordingly
            yield return new WaitForSeconds(_initialWaitTime);
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                _playerSoundPart01?.Play();
                int index = indices[i];

                if (i == 0)
                {
                    _winnerExplosion?.Play();
                }

                _playerImages[i].enabled = true;
                _playerFrames[i].enabled = true;
                _placesText[i].enabled = true;
                _placesTextBackground[i].enabled = true;

                SetAlpha(_playerImages[i], 0);
                SetAlpha(_playerFrames[i], 0);
                SetAlpha(_placesText[i], 0);
                SetAlpha(_placesTextBackground[i], 0);

                Texture2D texture = _playerSourceImages[ScoreManager.GetColor(index)];
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                _playerImages[i].sprite = sprite;

                var playerEmbers = _emberPrefabs[ScoreManager.GetColor(index)];

                GameObject instantiatedEmbers = Instantiate(playerEmbers, _emberSockets[i].position, _emberSockets[i].rotation);
                instantiatedEmbers.transform.GetChild(0).localScale = _emberSockets[i].localScale;

                float elapsedTime = 0f;
                while (elapsedTime < _timeBetweenFrames)
                {
                    float normalizedTime = Mathf.Clamp01(elapsedTime / _fadeInTime);
                    SetAlpha(_playerImages[i], normalizedTime);
                    SetAlpha(_playerFrames[i], normalizedTime);
                    SetAlpha(_placesText[i], normalizedTime);
                    SetAlpha(_placesTextBackground[i], normalizedTime);
                    elapsedTime += Time.deltaTime;

                    if (normalizedTime >= _minAlphaScaleEffect && !_IsScaleEffectPlaying)
                    {
                        StartCoroutine(ScaleImage(_playerFrames[i]));
                        StartCoroutine(ScaleImage(_playerImages[i]));
                    }

                    yield return null;
                }
                _IsScaleEffectPlaying = false;

                if (i == 0)
                {
                    AudioManager.PlaySound(_victoryEffect, .0f);
                    AudioManager.PlayMusic(_endScreenMusic, 5f);
                }

                SetAlpha(_playerImages[i], 1);
                SetAlpha(_playerFrames[i], 1);
                SetAlpha(_placesText[i], 1);
                SetAlpha(_placesTextBackground[i], 1);

                yield return new WaitForSeconds(_timeBetweenFrames);
            }

            yield return new WaitForSeconds(_timeBetweenFramesAndButtons);

            foreach (var button in _buttonsToDisable)
            {
                button.SetActive(true);

                // Gradually change the alpha value of the button
                float elapsedTime = 0f;
                while (elapsedTime < _timeBetweenFramesAndButtons)
                {
                    float normalizedTime = Mathf.Clamp01(elapsedTime / _fadeInTime);
                    SetAlpha(button.GetComponent<Image>(), normalizedTime);
                    SetAlpha(button.GetComponentInChildren<TextMeshProUGUI>(), normalizedTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                SetAlpha(button.GetComponent<Image>(), 1);
                SetAlpha(button.GetComponentInChildren<TextMeshProUGUI>(), 1);

                yield return new WaitForSeconds(_timeBetweenFramesAndButtons);
            }
        }

        private void Update()
        {
            if (!_enableButton)
            {
                _disableTimer += Time.deltaTime;
                if (_disableTimer >= _disableButtonTime)
                { _enableButton = true; }
            }
        }

        public void PlayAgain()
        {
            AudioManager.StopMusic(1f);

            if (_enableButton)
            {
                ScoreManager.ResetScore();
                PlayerManager.Instance.Resetart();
                if (_clickSound)
                {
                    if (!_clickSound.isPlaying)
                    {
                        _clickSound.Play();
                        Invoke("LoadSceneWithDelay", _clickSound.clip.length);
                    }
                }
                else
                {
                    // Load the next scene
                    SceneManager.LoadScene("01_Arena");
                }
            }
        }

        private void LoadSceneWithDelay()
        {
            SceneManager.LoadScene("01_Arena");
        }

        public void QuitGame()
        {
            if (_enableButton)
            {
                // Otherwise quit the application

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
        }

        private void SetAlpha(Image image, float alphaValue)
        {
            Color imageColor = image.color;
            imageColor.a = alphaValue;
            image.color = imageColor;
        }

        private void SetAlpha(TextMeshProUGUI text, float alphaValue)
        {
            Color textColor = text.color;
            textColor.a = alphaValue;
            text.color = textColor;
        }

        private IEnumerator ScaleImage(Image image)
        {
            _IsScaleEffectPlaying = true;
            float elapsedTime = 0f;
            Vector3 initialScale = image.transform.localScale;
            Vector3 targetScale = initialScale * _maxScaleMultiplier;
            Vector3 finalScale = initialScale;

            while (elapsedTime < _scaleDuration)
            {
                float normalizedTime = Mathf.Clamp01(elapsedTime / _scaleDuration);
                if (normalizedTime <= 0.5f)
                {
                    // Scale up
                    image.transform.localScale = Vector3.Lerp(initialScale, targetScale, normalizedTime * 2f);
                }
                else
                {
                    // Scale down
                    image.transform.localScale = Vector3.Lerp(targetScale, finalScale, (normalizedTime - 0.5f) * 2f);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _playerSoundPart02?.Play();
            image.transform.localScale = finalScale;
        }
    }
}
