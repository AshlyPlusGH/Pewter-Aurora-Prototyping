using NaughtyAttributes;
using UnityEngine;

namespace Ash {
    [RequireComponent(typeof(Rigidbody))]
    public class InteractRigidbody : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private Rigidbody rb;

        [SerializeField, Range(1,1000)] private float forceMult = 1;

        void Awake(){ Setup(); }
        public void Setup(){  }

        public void AddForce(Vector3 force)
        {
            rb.AddForce(force * forceMult);
        }

        public void AddForceCardinalForward()
        {
            rb.AddForce(Vector3.forward * forceMult);
        }

        public void AddForceForward()
        {
            rb.AddForce(transform.forward * forceMult);
        }
    }
}