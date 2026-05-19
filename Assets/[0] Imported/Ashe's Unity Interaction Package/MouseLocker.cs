using UnityEngine;

namespace Ash {
    public class MouseLocker : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private ENUM_MouseState onTriggerMouseState = ENUM_MouseState.Swap;

        public void Trigger()
        {
            switch (onTriggerMouseState)
            {
                case ENUM_MouseState.Locked:
                    LockMouse();
                    break;
                case ENUM_MouseState.Unlocked:
                    UnlockMouse();
                    break;
                case ENUM_MouseState.Swap:
                    if (Cursor.lockState == CursorLockMode.Locked){ UnlockMouse(); }
                    else { LockMouse(); }
                    break;
            }
        }

        public void LockMouse(){ STATIC_LockMouse(); }
        public static void STATIC_LockMouse(){ Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        public void UnlockMouse(){ STATIC_UnlockMouse(); }
        public static void STATIC_UnlockMouse(){ Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }

    enum ENUM_MouseState
    {
        Locked,
        Unlocked,
        Swap
    }
}