using UnityEngine;

namespace Ash {
    [ExecuteInEditMode]
    public class FramerateCapper : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private int framerate = 60;

        void Awake(){ Setup(); }
        public void Setup()
        {
            #if UNITY_EDITOR
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = framerate;
            #endif
        }
    }
}