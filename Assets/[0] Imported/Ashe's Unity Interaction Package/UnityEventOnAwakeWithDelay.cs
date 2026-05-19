using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Ash{
    public class UnityEventOnAwakeWithDelay : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private float delay;
        [SerializeField] private UnityEvent unityEvent;

        [SerializeField] private bool runOnce;
        private static GenericDictionary<string,bool> hasRanData = new();
        private static bool isStaticSetup = false;

        [SerializeField, ReadOnly] private string guid;

        private void Awake(){ Setup(); }
        void Setup()
        {
            if (!runOnce){ StartCoroutine(COROUTINE_TriggerEvent()); return; }

            STATIC_Setup();

                if ((guid == "") || (guid == null) || (guid == String.Empty)) { Debug.LogError("ERROR: GUID was None! At: " + this); return; }
            if (!hasRanData.ContainsKey(guid))
            {
                hasRanData.Add(new KeyValuePair<string, bool>(guid, false));

                StartCoroutine(COROUTINE_TriggerEvent());
            }
        }

        static void STATIC_Setup() 
        {
                if (isStaticSetup) { return; }
            isStaticSetup = true;

            hasRanData = new();
        }

        private IEnumerator COROUTINE_TriggerEvent()
        {
            if (runOnce){
                if (!hasRanData.ContainsKey(guid)){ Debug.Log("ERROR at: " + gameObject); Debug.Log("GUID was: " + guid); yield break; }
                if (runOnce && hasRanData[guid]){ Log("Script has already Run!"); yield break; } //If run once and shouldnt run multiple quit!
            }

            if (delay > 0){ yield return new WaitForSecondsRealtime(delay); }

            Log("Triggering!"); unityEvent.Invoke();

            UpdateData(true);

            yield break;
        }

        void UpdateData(bool ifScriptHasRun)
        {
            hasRanData[guid] = ifScriptHasRun;
        }

        [Button] public void GenerateNewGUID()
        {
            guid = System.Guid.NewGuid().ToString();
        }

        [Button] public void GenerateAllNewGUIDsInLoadedScenes()
        {
            foreach (var item in FindObjectsByType<UnityEventOnAwakeWithDelay>())
            {
                item.GenerateNewGUID();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetStatics(){ hasRanData = new(); }
    }
}