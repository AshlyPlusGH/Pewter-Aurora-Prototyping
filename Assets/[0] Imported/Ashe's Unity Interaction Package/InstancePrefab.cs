using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    public class InstancePrefab : MonoBehaviour
    {
        [SerializeField, Tooltip("Used to differenciate unique Instances!")] private string id;
        static Dictionary<string, InstancePrefab> instanceTracker = new();

        void OnEnable(){ Setup(); }
        void OnDisable(){ RemoveInstance(); }
        void OnDestroy(){ instanceTracker?.Remove(id); }
        void Awake(){ Setup(); }
        void Setup()
        {
            if (instanceTracker.ContainsValue(this)){ return; }
            if (instanceTracker.ContainsKey(id)){ Destroy(gameObject); return; }
            
            instanceTracker.Add(id, this);
        }
        void RemoveInstance()
        {
            if (!instanceTracker.ContainsKey(id)){ return; }

            instanceTracker.Remove(id);
            Destroy(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetStatics(){ instanceTracker = new(); }
    }
}