using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ash {
    // <summary>
    // Centralized scene management system with its own internal coroutine queue.
    // Handles loading, unloading, reloading, and tracking scenes safely.
    // </summary>
    public class SCENE : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        private static bool debugStatic = false;

        private static readonly List<string> builtScenes = new List<string>();
        private static readonly List<string> loadedScenes = new List<string>();

        private static SCENE instance;

        private static readonly Queue<IEnumerator> routineQueue = new Queue<IEnumerator>();
        private static bool isRunningQueue = false;
        
        #region Setup
            void Awake(){ Setup(); }
            public void Setup()
            {
                if ((instance == null) || (instance == this))
                {
                    if (instance != this){ instance = this; }
                        foreach (Component component in GetComponents<Component>()) // If tainted GO, Purify it!
                        {
                            if (component.GetType() == typeof(Transform)){ continue; }
                            if (component != this){ Destroy(component); }
                        }
                        transform.SetParent(null); //DDoL Fixer
                    DontDestroyOnLoad(gameObject);
                    SetupClass();
                }
                else { Destroy(gameObject); }
            }

            private static void SetupClass()
            {
                UpdateBuiltScenes();

                Scene activeScene = SceneManager.GetActiveScene();

                if (!loadedScenes.Contains(activeScene.name)){ loadedScenes.Add(activeScene.name); }
            }
        #endregion

        #region Utility
            private static void InstancedCheck(){
                if (instance != null){ return; }

                GameObject newSceneLoader = Instantiate(new GameObject("[Scene] RuntimeSceneLoader [SCENE]"));
                SCENE newInstance = newSceneLoader.AddComponent<SCENE>();

                newInstance.Setup();
            }

            /// <summary> Populates the list of scenes included in the build. </summary>
            private static void UpdateBuiltScenes()
            {
                builtScenes.Clear();

                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string name = SceneManager.GetSceneByBuildIndex(i).name;
                    builtScenes.Add(name);
                }
            }
        #endregion

        #region Queue System
            /// <summary> Adds a coroutine to the internal queue and starts processing if idle. </summary>
            private static void QueueRoutine(IEnumerator routine)
            {
                routineQueue.Enqueue(routine);

                if (!isRunningQueue)
                    instance.StartCoroutine(ProcessQueue());
            }

            /// <summary> Processes queued coroutines sequentially. </summary>
            private static IEnumerator ProcessQueue()
            {
                isRunningQueue = true;

                while (routineQueue.Count > 0)
                {
                    yield return instance.StartCoroutine(routineQueue.Dequeue());
                }

                isRunningQueue = false;
            }
        #endregion

        #region Public Calls
            /// <summary> Loads a scene additively and sets it active. </summary>
            public static void LoadScene(string sceneName) { InstancedCheck(); QueueRoutine(COROUTINE_LoadScene(sceneName)); }
            
            /// <summary> Unloads a specific scene by name. </summary>
            public static void UnloadScene(string sceneName){ InstancedCheck(); QueueRoutine(COROUTINE_UnloadScene(sceneName)); }

            /// <summary> Unloads the last loaded scene. </summary>
            public static void UnloadLastScene(){ InstancedCheck(); QueueRoutine(COROUTINE_UnloadLastScene()); }

            /// <summary> Unloads the first loaded scene. </summary>
            public static void UnloadFirstScene(){ InstancedCheck(); QueueRoutine(COROUTINE_UnloadFirstScene()); }

            /// <summary> Reloads a scene by unloading and loading it again. </summary>
            public static void ReloadScene(string sceneName)
            {
                InstancedCheck();

                QueueRoutine(COROUTINE_UnloadScene(sceneName));
                QueueRoutine(COROUTINE_LoadScene(sceneName));
            }

            /// <summary> Sets the Active Scene. </summary>
            public static void SetActiveScene(string sceneName)
            {
                InstancedCheck();

                QueueRoutine(COROUTINE_SetActiveScene(sceneName));
            }

            /// <summary> The Action that Quits the Game. </summary>
            private static void QuittingGame(){ InstancedCheck(); Application.Quit(); }
                /// <summary> Quits the Game. </summary>
                public static void QuitGame(){ QuittingGame(); }
                /// <summary> Quits the Game. </summary>
                public static void Quit(){ QuittingGame(); }
                /// <summary> Quits the Game. </summary>
                public static void QuitApp(){ QuittingGame(); }
                /// <summary> Quits the Game. </summary>
                public static void QuitApplication(){ QuittingGame(); }
        #endregion

        #region Coroutines
            /// <summary> Loads a scene additively and sets it active. </summary>
            private static IEnumerator COROUTINE_LoadScene(string sceneName)
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

                yield return null; // wait one frame

                Scene scene = SceneManager.GetSceneByName(sceneName);

                if (!scene.isLoaded)
                {
                    Debug.LogWarning($"Scene not loaded: {sceneName}");
                    yield break;
                }

                loadedScenes.Add(sceneName);
                if (debugStatic) Debug.Log($"{sceneName} Scene Loaded!");

                SceneManager.SetActiveScene(scene);
            }

            /// <summary> Unloads a scene by name. </summary>
            private static IEnumerator COROUTINE_UnloadScene(string sceneName, Action onSceneUnloaded = null)
            {
                if (debugStatic){ Debug.Log("Starting Scene Unload"); }

                Scene scene = SceneManager.GetSceneByName(sceneName);

                if (!scene.isLoaded)
                {
                    Debug.LogWarning($"Scene not loaded: {sceneName}");
                    yield break;
                }

                yield return SceneManager.UnloadSceneAsync(sceneName);

                if (scene.isLoaded){ Debug.LogWarning($"Scene was not Unloaded: {sceneName}"); yield break; }
                else if (debugStatic) Debug.Log($"{sceneName} Scene Unloaded!");

                loadedScenes.Remove(sceneName);
                onSceneUnloaded?.Invoke();

                yield return null;
            }

            /// <summary> Coroutine that unloads the most recently loaded scene. </summary>
            private static IEnumerator COROUTINE_UnloadLastScene(Action onSceneUnloaded = null)
            {
                string sceneName = loadedScenes[loadedScenes.Count - 1];
                yield return COROUTINE_UnloadScene(sceneName, onSceneUnloaded);
            }

            /// <summary> Coroutine that unloads the earliest loaded scene. </summary>
            private static IEnumerator COROUTINE_UnloadFirstScene(Action onSceneUnloaded = null)
            {
                string sceneName = loadedScenes[0];
                yield return COROUTINE_UnloadScene(sceneName, onSceneUnloaded);
            }

            /// <summary> Coroutine that sets the current Active Scene. </summary>
            private static IEnumerator COROUTINE_SetActiveScene(string sceneName)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

                yield return null;
            }
        #endregion
    }
}