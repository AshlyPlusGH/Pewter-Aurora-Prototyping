using UnityEngine;

namespace Ash
{
    public class FollowMouse : MonoBehaviour
    {
        [SerializeField] private float zPos = 0;
        [SerializeField] private TransformArea transformArea = TransformArea.World;

        void Update()
        {
            Vector3 targetPosition = Input.mousePosition;
            targetPosition.z = zPos;
            if (transformArea == TransformArea.World){ targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); }
            if (!IsValid(targetPosition)){ return; }
            gameObject.transform.position = targetPosition;
        }

        private bool IsValid(Vector3 v)
        {
            return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
        }
    }

    enum TransformArea
    {
        World,
        Screen
    }
}