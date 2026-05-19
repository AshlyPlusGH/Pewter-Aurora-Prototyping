using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Ash {
    [ExecuteInEditMode]
    public class LookAtTransform : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private Transform target;
        [SerializeField, Tag] private string targetTagFindOnAwake;

        void Awake(){ Setup(); }
        public void Setup(){ if (target != null){ return; } target = GameObject.FindWithTag(targetTagFindOnAwake).transform; }

        void Update()
        {
            transform.LookAt(target);
        }
    }
}