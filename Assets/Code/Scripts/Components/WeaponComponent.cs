using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class WeaponComponent : MonoBehaviour
    {
        [SerializeField] private string _parentName = "_Dynamic";
        [SerializeField] private GameObject _projectileTemplate = null;
        [SerializeField] private bool _hasInfinitBullets = true;
        [SerializeField] private int _clipSize = 50;
        [SerializeField] private float _fireRate = 25.0f; //bullets per second
        [SerializeField] private List<Transform> _fireSockets = new List<Transform>();

        [Header("Sound settings")]
        [SerializeField] private AudioSource _fireSound;
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        private bool _triggerPulled = false;
        private int _currentAmmo = 50;
        private float _fireTimer = 0.0f;
        private GameObject _parent = null;

        #region Events
        
        public delegate void FireDelegate();
        public event FireDelegate OnFire;
        
        #endregion Events

        private void Awake()
        {
            _currentAmmo = _clipSize;
            _parent = GameObject.Find(_parentName);
        }


        void Update() // checks if the trigger has been pulled
        {
            if (_fireTimer > 0.0f)
                _fireTimer -= Time.deltaTime;

            if (_fireTimer <= 0.0f && _triggerPulled)
                FireProjectile();

            _triggerPulled = false;
        }

        private void FireProjectile()//spawns a object at the location of the fire sockets 
                                     //the object preferableneeds to have a damage component of some sort
        {
            if (_currentAmmo <= 0 && !_hasInfinitBullets) return;// checks if the clip is empty or if _hasInfinitBullets is false
                                                                 // and cancels the action if you have 0 ammo and you do not have infinit ammo 

            if (_projectileTemplate == null)
                return;

            --_currentAmmo;

            for (int i = 0; i < _fireSockets.Count; i++)
            {
                var projectile = Instantiate(_projectileTemplate, _fireSockets[i].position, _fireSockets[i].rotation);
                
                // Attach the projectile to the _Dynamic level object, to keep the hierarchy clean
                // This is not necessary, but it's a good practice
                projectile.transform.parent = _parent?.transform;

                OnFire?.Invoke();
            }

            _fireTimer += 1.0f / _fireRate;

            if (_fireSound)
            {
                float randomPitch = Random.Range(_minPitch, _maxPitch);
                _fireSound.pitch = randomPitch;
                
                _fireSound.Play();//playerfeedback sound that you shot
            }
        }

        public void Fire()// this fuction is called in the shooting behavior of the chatacter using this weapon
        {
            _triggerPulled = true;
        }

        public void Reload()// reloads the weapon but is not really needed if _triggerPulled is true
        {
            _currentAmmo = _clipSize;
        }
    }
}
