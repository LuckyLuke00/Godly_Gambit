using UnityEngine;
using UnityEngine.UI;

namespace GodlyGambit
{
    public class UISpriteSheetAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] _spriteSheet = null;
        [SerializeField] private float _frameRate = 10f;

        private Image _image = null;

        private void Awake()
        {
            if (!TryGetComponent(out _image))
            {
                Debug.LogError("No Image component found on " + gameObject.name);
            }
        }

        private void Update()
        {
            if (!_image || _spriteSheet.Length == 0) return;

            // Increment the frame index based on the frame rate
            int currentFrame = Mathf.FloorToInt(Time.time * _frameRate) % _spriteSheet.Length;

            // Update the sprite
            _image.sprite = _spriteSheet[currentFrame];
        }
    }
}
