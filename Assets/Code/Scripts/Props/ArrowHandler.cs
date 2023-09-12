using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GodlyGambit
{
    public class ArrowHandler : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float _height = 1.0f;
        [SerializeField] private float _lifeTime = 10.0f;
        [SerializeField] private float _speed = 50.0f;
        [SerializeField] private bool _useWallDetection = true;
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private ParticleSystem _hitVXF = null;
        [SerializeField] private List<GameObject> _objectsToDisable = new List<GameObject> ();

        private MeshRenderer _arrowMesh = null;
        private bool _IshitVXFSpawned = false;

        #endregion Variables

        private void Awake()
        {
            // Create
            Invoke(KILL_METHODNAME, _lifeTime);
            _arrowMesh = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            if(!WallDetection())
            transform.position += transform.forward * (Time.deltaTime * _speed);
            
        }

        private void FixedUpdate()
        {
            HandleTrajectory();
        }

        private const string KILL_METHODNAME = "Kill";

        private void Kill()
        {
            Destroy(gameObject);
        }

        private void HandleTrajectory()
        {
            Ray collisionRay = new Ray(transform.position, -transform.up);
            if (Physics.Raycast(collisionRay, out RaycastHit hit))
            {
                // If the collider is a trigger, ignore it
                if (hit.collider.isTrigger) return;

                transform.position = hit.point + Vector3.up * _height;
                return;
            }

            if (Physics.Raycast(collisionRay, out hit))
            {
                if (hit.collider.isTrigger) return;

                transform.position = hit.point + Vector3.up * _height;
            }
        }

        private bool WallDetection()
        {
            if (_useWallDetection)
            {
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, Time.deltaTime * _speed, _wallLayer))
                {                    
                    if (_hitVXF)
                    {
                        float lifetime = _hitVXF.main.startLifetime.constant + 0.1f;

                        foreach (GameObject obj in _objectsToDisable)
                        {
                            if (obj != null) obj.SetActive(false);
                        }
                        _arrowMesh.enabled = false;

                        Vector3 offset = transform.forward * 0.9f;

                        if (!_IshitVXFSpawned)
                        {
                            _IshitVXFSpawned = true;
                            GameObject hitVXFInstance = Instantiate(_hitVXF.gameObject,
                                transform.position + offset, Quaternion.identity);

                            hitVXFInstance.transform.parent = transform;
                            hitVXFInstance.transform.forward = transform.forward;


                            Destroy(gameObject, lifetime);
                        }
                    }
                    else
                    {
                        Destroy(gameObject);
                        
                    }

                    return true;
                }
            }

            return false;
        }

    }
}
