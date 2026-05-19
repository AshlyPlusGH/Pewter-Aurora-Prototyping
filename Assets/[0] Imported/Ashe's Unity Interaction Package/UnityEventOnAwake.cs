using UnityEngine;
using UnityEngine.Events;

namespace Ash {
    public class UnityEventOnAwake : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private UnityEvent unityEvent;

        private void Awake()
        {
            unityEvent.Invoke();
        }
    }
}