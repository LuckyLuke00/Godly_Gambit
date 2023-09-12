using UnityEngine;

namespace GodlyGambit
{
    public class FirebowlColorSwitcher : MonoBehaviour
    {
        [SerializeField] private Light[] _lights = null;
        
        [Header("Optional")]
        [SerializeField] private ParticleSystem _particles = null;
        private Material _particleMaterial = null;

        private void Awake()
        {
            if (!_particles) return;

            _particleMaterial = _particles.GetComponent<Renderer>().material;
        }

        public void SwitchColors(UnityEngine.Color lightColor, UnityEngine.Color fireColor)
        {
            foreach (var light in _lights)
            {
                light.color = lightColor;
            }

            // Set the albedo map of the particle material to the color if it exists
            if (_particleMaterial) _particleMaterial.SetColor("_Color", fireColor);
        }

        public void SwitchColors(UnityEngine.Color lightColor)
        {
            foreach (var light in _lights)
            {
                light.color = lightColor;
            }

            // Set the albedo map of the particle material to the color if it exists
            if (_particleMaterial) _particleMaterial.SetColor("_Color", lightColor);
        }
    }
}
