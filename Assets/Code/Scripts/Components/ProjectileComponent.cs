using UnityEngine;

namespace GodlyGambit
{
    public class ProjectileComponent : MonoBehaviour
    {
        [SerializeField] private bool _rumbleOnDestroy = false;
        [SerializeField] private bool _useWallDetection = true;
        [SerializeField] private float _lifeTimeSec = 2.5f;
        [SerializeField] private float _speed = 300.0f;
        [SerializeField] private LayerMask _wallLayer;

        [SerializeField] private Audio _hitSound = null;
        [SerializeField] private Audio _crashSound = null;

        [SerializeField] private ParticleSystem _hitVXF = null;

        private void Awake()
        {
            if (_crashSound != null) AudioManager.PlaySound(_crashSound, true, .5f);
        }

        private void FixedUpdate()
        {
            WallDetection();
            transform.position += transform.forward * Time.deltaTime * _speed; // use forward vector of an object to transform it forward
        }

        private void Update()
        {
            _lifeTimeSec -= Time.deltaTime;
            if (_lifeTimeSec < 0.0f)
            {
                Destroy(gameObject);
            }
        }

        private void WallDetection()
        {
            if (_useWallDetection)
            {
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, Time.deltaTime * _speed, _wallLayer))
                {
                    if (_rumbleOnDestroy) PlayerManager.Instance.RumbleAll(.25f, 1f, .5f);

                    // Wall detected
                    PlayerManager.Instance.RumbleAll(.25f, 1f, .5f);
                    if (_hitSound != null) AudioManager.PlaySound(_hitSound, true);

                    if (_hitVXF)
                    {
                        ParticleSystem instance = Instantiate(_hitVXF, hit.point, Quaternion.identity);
                        instance.transform.parent = hit.transform;
                        Destroy(instance.gameObject, instance.main.duration);
                        Destroy(gameObject, instance.main.duration);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
