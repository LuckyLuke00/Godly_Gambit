using UnityEngine;

namespace GodlyGambit
{
    public interface INavMeshTargetSetter
    {
        public void SetTarget(Vector3 target);

        public Vector3 GetTarget();
    }
}
