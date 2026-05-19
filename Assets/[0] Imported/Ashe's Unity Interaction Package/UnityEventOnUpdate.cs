using UnityEngine;
using UnityEngine.Events;

namespace Ash {
    public class UnityEventOnUpdate : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private UnityEvent unityEvent;

        void Update(){ Trigger(); }
        void Trigger()
        {
            unityEvent.Invoke();
        }
    }
}