using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ash{
    public class UnityEventOnColliderClicked : MonoBehaviour
    {
        [SerializeField] protected bool debug;
            protected void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}
            protected static void STATIC_Log(string contents){ Debug.Log(contents); }

        [Space(10)]

        [SerializeField] private int mouseButton = 0;
        [SerializeField] private ENUM_MOUSE_PressType inputType = ENUM_MOUSE_PressType.Down;
        
        [SerializeField] private UnityEvent unityEvent;
        public event Action onColliderClicked;

        [SerializeField] private bool detect2DColliders = true;
        [SerializeField] private bool detect3DColliders = true;

        void Update()
        {
            if (CheckForPresses()) TryTriggerOnCollider();
        }

        bool CheckForPresses()
        {
            switch (inputType)
            {
                case ENUM_MOUSE_PressType.Up:
                    return Input.GetMouseButtonUp(mouseButton);
                case ENUM_MOUSE_PressType.Down:
                    return Input.GetMouseButtonDown(mouseButton);
                case ENUM_MOUSE_PressType.Stay:
                    return Input.GetMouseButton(mouseButton);
            }

            return false;
        }

        void TryTriggerOnCollider()
        {
            if (detect2DColliders)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(worldPos, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    Log("Clicked collider: " + name);
                    Trigger();
                }
            }
            if (detect3DColliders)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Log("Clicked collider: " + name);
                        Trigger();
                    }
                }
            }
        }

        void Trigger(){ Log("Triggering!"); onColliderClicked?.Invoke(); unityEvent.Invoke(); }
    }
}