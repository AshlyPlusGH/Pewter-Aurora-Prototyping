using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Ash {
    public class UnityEventOnSceneLoaded : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]
        
        [SerializeField, Scene] private List<string> checkedScenes;
        [SerializeField] private UnityEvent unityEvent;

        void OnEnable(){ Setup(); }
        void Setup()
        {
            RegisterListeners();
            SceneCheck(SceneManager.GetActiveScene());
        }
        void RegisterListeners()
        {
            SceneManager.sceneLoaded += SceneCheck;
        }

        void OnDisable(){ UnregisterListeners(); }
        void UnregisterListeners()
        {
            SceneManager.sceneLoaded -= SceneCheck;
        }

        void SceneCheck(Scene newScene, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (!checkedScenes.Contains(newScene.name)){ return; }
            unityEvent.Invoke();
        }
    }
}