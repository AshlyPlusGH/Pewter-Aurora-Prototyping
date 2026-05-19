using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Ash {
    public class DEBUG_UnityEventOnInspectorButton : MonoBehaviour
    {
        [SerializeField] private UnityEvent unityEvent;

        [Button]
        void Trigger(){ unityEvent.Invoke(); }
    }
}