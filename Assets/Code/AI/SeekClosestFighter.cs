using System.Collections.Generic;
using UnityEngine;

namespace GodlyGambit
{
    public class SeekClosestFighter : MonoBehaviour
    {
        private INavMeshTargetSetter _targetSetter = null;

        private void Awake()
        {
            if (!TryGetComponent(out _targetSetter))
            {
                Debug.LogError("SeekClosestFighter requires a component that implements INavMeshTargetSetter");
            }
        }

        private void Update()
        {
            _targetSetter.SetTarget(GetClosestFighter());
        }

        private Vector3 GetClosestFighter()
        {
            List<GameObject> fighters = PlayerManager.Instance.AliveFighters;
            Vector3 currentPosition = transform.position;

            // Find the closest fighter
            GameObject closestFighter = null;
            float minDistance = Mathf.Infinity;

            foreach (GameObject fighter in fighters)
            {
                if (fighter == null) continue;

                float distance = (fighter.transform.position - currentPosition).sqrMagnitude;
                if (distance >= minDistance) continue;

                closestFighter = fighter;
                minDistance = distance;
            }

            // If the closest fighter is null and it has not already reached the target, return the current position
            if (closestFighter == null)
            {
                PlayerManager.Instance.PurgeLevel();
                return Vector3.zero;
            }

            // Return the closest fighter's position
            return closestFighter.transform.position;
        }
    }
}
