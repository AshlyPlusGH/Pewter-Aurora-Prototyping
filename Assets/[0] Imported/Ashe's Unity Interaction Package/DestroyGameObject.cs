using UnityEngine;

namespace Ash {
    public class DestroyGameObject : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private bool var1;

        void Awake(){ Setup(); }
        public void Setup(){  }

        public void Destroy(){ Destroy(gameObject); }
    }
}