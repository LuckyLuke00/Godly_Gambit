using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class ScoreManager : MonoBehaviour
    {
        private static Dictionary<int, float> _playerTimes = new Dictionary<int, float>();
        private static Dictionary<int, int> _playerLives = new Dictionary<int, int>();
        private static Dictionary<int, int> _playerColors = new Dictionary<int, int>();

        public static void AddPlayerTime(int instanceId, float time)
        {
            if (!_playerTimes.ContainsKey(instanceId))
            {
                _playerTimes.Add(instanceId, time);
            }
        }

        public static void AddPlayerLives(int instanceId, int lives)
        {
            if (_playerTimes.ContainsKey(instanceId))
            {
                _playerLives.Add(instanceId, lives);
            }
        }

        public static void AddPlayerColor(int instanceId, int color)
        {
            if (_playerTimes.ContainsKey(instanceId))
            {
                _playerColors.Add(instanceId, color);
            }
        }

        public static float GetPlayerTime(int instanceId)
        {
            if (_playerTimes.ContainsKey(instanceId))
            {
                return _playerTimes[instanceId];
            }
            return 0f;
        }

        public static int GetPlayerLives(int instanceId)
        {
            if (_playerTimes.ContainsKey(instanceId))
            {
                return _playerLives[instanceId];
            }
            return 0;
        }

        public static int GetDictionarySize()
        {
            return _playerTimes.Count;
        }

        public static int GetColor(int instanceId)
        {
            if (_playerTimes.ContainsKey(instanceId))
            {
                return _playerColors[instanceId];
            }
            return 0;
        }

        public static void ResetScore()
        {
            _playerTimes.Clear();
            _playerLives.Clear();
            _playerColors.Clear();
        }
    }
}
