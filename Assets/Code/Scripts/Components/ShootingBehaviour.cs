using UnityEngine;

namespace GodlyGambit
{
    public class ShootingBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject _weaponPrefab = null;
        [SerializeField] private Transform _weaponSocket = null;
        [SerializeField] private ParticleSystem _fireParticle = null;

        private WeaponComponent _weapon = null;
        private MeshRenderer _weaponRenderer = null;

        private void Awake()
        {
            if (_weaponPrefab != null && _weaponSocket != null)
            {
                var gunObject = Instantiate(_weaponPrefab, _weaponSocket, true); // make a gun instance from the template on the location of the socket
                gunObject.transform.localPosition = Vector3.zero; //lock position and rotation of the gun to the parrent
                gunObject.transform.localRotation = Quaternion.identity;
                _weapon = gunObject.GetComponent<WeaponComponent>(); // make the gun acceseble in this script
                _weaponRenderer = gunObject.GetComponent<MeshRenderer>(); // make the gun acceseble in this script
            }
        }

        private void OnEnable()
        {
            if (_weapon)
            {
                _weapon.OnFire += OnFire;
            }
        }

        private void OnDisable()
        {
            if (_weapon)
            {
                _weapon.OnFire -= OnFire;
            }
        }

        private void OnFire()
        {
            if (_fireParticle)
            {
                _fireParticle.Play();
            }
        }

        public void SetWeaponMaterial(Material material)
        {
            if (!_weaponRenderer) return;
            _weaponRenderer.material = material;
        }

        public void FireWeapon()
        {
            if (!_weapon) return;
            _weapon.Fire();
        }

        public void Reload()
        {
            if (!_weapon) return;

            _weapon.Reload();
        }
    }
}
