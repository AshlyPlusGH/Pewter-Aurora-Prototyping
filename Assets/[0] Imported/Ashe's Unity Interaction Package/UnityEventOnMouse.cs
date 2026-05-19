using UnityEngine;
using UnityEngine.Events;

namespace Ash{
    public class UnityEventOnMouse : MonoBehaviour
    {
        [SerializeField] private bool debug;
                private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private int mouseButton;
        [SerializeField] private ENUM_MOUSE_PressType inputType;

        [SerializeField] private UnityEvent unityEvent;

        void Update(){ CheckForPresses(); }
        void CheckForPresses()
        {
            switch (inputType)
            {
                case ENUM_MOUSE_PressType.Up:
                    if (Input.GetMouseButtonUp(mouseButton)){ Trigger(); }
                break;
                case ENUM_MOUSE_PressType.Down:
                    if (Input.GetMouseButtonDown(mouseButton)){ Trigger(); }
                break;
                case ENUM_MOUSE_PressType.Stay:
                    if (Input.GetMouseButton(mouseButton)){ Trigger(); }
                break;
            }
        }

        void Trigger(){ unityEvent.Invoke(); }
    }

    public enum ENUM_MOUSE_PressType
    {
        Up,
        Down,
        Stay
    }
}