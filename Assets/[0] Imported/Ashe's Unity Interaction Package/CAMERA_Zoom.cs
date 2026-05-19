using UnityEngine;
using UnityEngine.Events;

namespace Ash {
    public class CAMERA_Zoom : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]
        
        [SerializeField] private Camera target;

        [SerializeField] private int zoomMax = 10;
        [SerializeField] private int zoomMin = 1;

        [Space(10)]

        [SerializeField] private int zoomSpeed = 1;

        [Space(10)]

        [SerializeField] private ENUM_CAMERA_Type cameraType = ENUM_CAMERA_Type.Standard;

        [Space(10)]

        [SerializeField] private UnityEvent unityEventOnScroll;

        void Awake(){ Setup(); }
        void Setup()
        {
                if (target == null){ this.enabled = false; return; }
            if (target.orthographicSize < zoomMin){ target.orthographicSize = zoomMin; }
            if (target.orthographicSize > zoomMax){ target.orthographicSize = zoomMax; }
        }

        void Update(){ Scroll(); }
        void Scroll()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll == 0){ return; }

            switch (cameraType)
            {
                case ENUM_CAMERA_Type.Standard:
                    StandardScroll(scroll);
                    break;
            }

            unityEventOnScroll.Invoke();
        }
            void StandardScroll(float scroll)
            {
                if (scroll < 0f)
                {
                    // Scroll up
                    if (target.orthographicSize + zoomSpeed >= zoomMax){ target.orthographicSize = zoomMax; }
                    target.orthographicSize += zoomSpeed;
                }
                else if (scroll > 0f)
                {
                    // Scroll down
                    if (target.orthographicSize - zoomSpeed <= zoomMin){ target.orthographicSize = zoomMin; }
                    if (target.orthographicSize - zoomSpeed > 1){
                        target.orthographicSize -= zoomSpeed;
                    }
                }
            }
    }

    enum ENUM_CAMERA_Type
    {
        Standard
    }
}
