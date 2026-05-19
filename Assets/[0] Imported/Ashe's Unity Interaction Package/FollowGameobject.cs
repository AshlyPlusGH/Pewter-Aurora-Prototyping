using UnityEngine;

namespace Ash {
    public class FollowGameobject : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private GameObject target;

        private Vector3 offset;

        void Awake(){ Setup(); }
        void Setup()
        {
            if (target == null){ this.enabled = false; return; }

            offset = transform.position;
        }

        void Update()
        {
            UpdatePos();
        }
        void UpdatePos(){ transform.position = target.transform.position + offset; }

        public void SetFollowTarget(GameObject newTarget){ target = newTarget; UpdatePos(); }
    }
}