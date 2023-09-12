using UnityEngine;

namespace GodlyGambit
{
    [System.Serializable]
    public class Audio
    {
        [SerializeField] private AudioClip _clip = null;

        [SerializeField] private bool _loop = false;

        [SerializeField, Range(0f, 1f)] private float _pitchVariation = 0f;
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 0f;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0f, 500f)] private float _maxDistance = 100f;
        [SerializeField, Range(-1f, 3f)] private float _pitch = 1f;

        public AudioClip Clip { get => _clip; }
        public bool Loop { get => _loop; }
        public float MaxDistance { get => _maxDistance; }
        public float Pitch { get => _pitch; }
        public float PitchVariation { get => _pitchVariation; }
        public float SpatialBlend { get => _spatialBlend; }
        public float Volume { get => _volume; }
    }
}
