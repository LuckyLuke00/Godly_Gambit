using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class ItemDespawnComponent : MonoBehaviour
    {
        #region Variables
        [SerializeField, Tooltip("Time before the item despawns in seconds")] private float _despawnTime = 5.0f;
        [SerializeField, Tooltip("Time before the item starts to flash in seconds")] private float _flashTime = 3.0f;
        [SerializeField, Tooltip("Time between flashes in seconds")] private float _flashInterval = 0.2f;
        [SerializeField, Tooltip("Shortens time between flashes in seconds")] private float _flashIntervalAcelerator = 0.0f;
        [SerializeField, Tooltip("Minimum time between flashes in seconds")] private float _minFlashInterval = 0.1f;
        [SerializeField] private List<Renderer> _flashItems = new List<Renderer>();

        private MeshRenderer _meshRenderer;
        private bool _isFlashing;
        private float _flashTimer;
        #endregion Variables

        private void Awake()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (_meshRenderer == null )
            {
                Debug.LogError("No MeshRenderer component found.");
            }

            Invoke("StartFlashing", _flashTime);
            Invoke("Kill", _despawnTime);
        }
       
        void Update()
        {
            if (_isFlashing) 
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0.0f)
                {
                    foreach (var item in _flashItems) 
                    {
                        
                        item.enabled = !item.enabled;
                    }
                    //_meshRenderer.enabled = !_meshRenderer.enabled;                    
                    _flashTimer = _flashInterval;

                    if ((_flashInterval - _flashIntervalAcelerator) > _minFlashInterval)
                    _flashInterval -= _flashIntervalAcelerator;
                }
            }
        }

        private void StartFlashing()
        {
            _isFlashing = true;
            _flashTimer = _flashInterval;
        }

        private void Kill()
        {
            Destroy(gameObject);
        }
    }
}
