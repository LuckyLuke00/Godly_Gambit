using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class SpawnItemOnDeath : MonoBehaviour
    {
        [SerializeField] private SpawnableItem[] _spawnableItems = null;
        private HealthComponent _healthComponent = null;

        // Subscibe to the OnDeath event of the health component
        private void Awake()
        {
            // Try get the health component on this gameobject
            if (!TryGetComponent(out _healthComponent))
            {
                Debug.LogError("No HealthComponent found on this gameobject");
            }
        }

        private void OnEnable()
        {
            _healthComponent.OnDeath += SpawnItem;
        }

        // Unsubscibe to the OnDeath event of the health component
        private void OnDisable()
        {
            _healthComponent.OnDeath -= SpawnItem;
        }

        private void SpawnItem()
        {
            // We need this list so that the first item in the list isn't more likely to be spawned than the last item
            List<SpawnableItem> spawnableItems = new List<SpawnableItem>();

            // Loop through all spawnable items
            foreach (SpawnableItem spawnableItem in _spawnableItems)
            {
                // Check if the item should be spawned
                if (Random.value > spawnableItem.PercentChance) continue;

                spawnableItems.Add(spawnableItem);
            }

            // Check if there are any items to spawn
            if (spawnableItems.Count == 0) return;

            // Get a random item from the list
            SpawnableItem itemToSpawn = spawnableItems[Random.Range(0, spawnableItems.Count)];

            // Spawn the item
            Instantiate(itemToSpawn.Item, transform.position, Quaternion.identity);
        }
    }

    [System.Serializable]
    public class SpawnableItem
    {
        [SerializeField] private GameObject _item = null;
        [SerializeField, Range(0f, 1f)] private float _percentChance = .0f;

        public GameObject Item
        { get { return _item; } }

        public float PercentChance
        { get { return _percentChance; } }
    }
}
