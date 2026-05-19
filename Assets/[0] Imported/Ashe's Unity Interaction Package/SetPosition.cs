using UnityEngine;

namespace Ash {
    public class SetPosition : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetPos;
        [SerializeField] private Transform targetPosTransform;

        public void Set(){ target.transform.position = targetPos; target.transform.position = targetPosTransform.transform.position; }
    }
}