using UnityEngine;
using UnityEngine.Events;

namespace Ash {
    public class UnityEventOnDestroy : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private UnityEvent unityEventOnDestroy;

        void OnDestroy(){ unityEventOnDestroy.Invoke(); }
    }
}