using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class PlayerSpawnPointManager : MonoBehaviour
    {
        private Transform _godSpawnPoint = null;
        private readonly List<SpawnPoint> _playerSpawnPoints = new List<SpawnPoint>();
        private readonly List<int> _usedSpawnPointIndices = new List<int>();

        public void FindSpawnPoints()
        {
            _playerSpawnPoints.Clear();
            _usedSpawnPointIndices.Clear();

            // Find all gameobjects with SpawnPoint script
            SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();

            // Get their transforms
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.IsGodSpawnPoint)
                {
                    _godSpawnPoint = spawnPoint.Transform;
                    continue;
                }

                _playerSpawnPoints.Add(spawnPoint);
            }

            if (!_godSpawnPoint)
            {
                Debug.LogError("<color=#ff6b6b>PlayerSpawnPointManager:</color> No god spawn point found!");
            }

            if (_playerSpawnPoints.Count == 0)
            {
                Debug.LogError("<color=#ff6b6b>PlayerSpawnPointManager:</color> No player spawn points found!");
            }
        }

        public Transform GetRandomPlayerSpawnPoint()
        {
            return _playerSpawnPoints[GetUnusedPlayerSpawnPointIndex()].Transform;
        }

        public Transform GetGodSpawnPoint() => _godSpawnPoint;

        private int GetUnusedPlayerSpawnPointIndex()
        {
            int randomIndex = Random.Range(0, _playerSpawnPoints.Count);
            if (_usedSpawnPointIndices.Count >= _playerSpawnPoints.Count)
            {
                // Clear
                _usedSpawnPointIndices.Clear();
                return randomIndex;
            }

            while (_usedSpawnPointIndices.Contains(randomIndex) || _playerSpawnPoints[randomIndex].IsOccupied())
            {
                randomIndex = Random.Range(0, _playerSpawnPoints.Count);
            }
            _usedSpawnPointIndices.Add(randomIndex);
            return randomIndex;
        }

        public void ResetUsedSpawnPoints()
        {
            _usedSpawnPointIndices.Clear();
        }
    }
}
