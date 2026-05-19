using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ash{
    public class UnityEventOnKey : MonoBehaviour
    {
        [SerializeField] private bool debug;
                private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private List<KeyCode> watchedKeys = new();
        [SerializeField] private ENUM_INPUT_Type inputType;

        [SerializeField] private UnityEvent unityEvent;

        void Update(){ CheckWatchedKeys(); }
        void CheckWatchedKeys()
        {
            foreach (var item in watchedKeys)
            {
                switch (inputType)
                {
                    case ENUM_INPUT_Type.Up:
                        if (Input.GetKeyUp(item)){ Trigger(); }
                        break;
                    case ENUM_INPUT_Type.Down:
                        if (Input.GetKeyDown(item)){ Trigger(); }
                        break;
                    case ENUM_INPUT_Type.Stay:
                        if (Input.GetKey(item)){ Trigger(); }
                        break;
                }
            }
        }

        void Trigger(){ unityEvent.Invoke(); }

        enum ENUM_INPUT_Type
        {
            Up,
            Down,
            Stay
        }
    }
}