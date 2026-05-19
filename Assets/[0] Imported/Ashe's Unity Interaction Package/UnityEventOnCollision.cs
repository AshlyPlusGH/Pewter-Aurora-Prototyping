using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Ash {
    public class UnityEventOnCollision : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]
        
        [Header("Watched Colliders: Extra Colliders not present on this GameObject.")]
            [SerializeField] private List<Collider> watchedColliders = new();
            [SerializeField] private List<Collider2D> watchedColliders2D = new();

        [Space(10)]
        
        [Header("Unity Events")]
                [SerializeField] private bool isTriggeredOnce_OnEnter = false;
                private bool locked_OnEnter = false;
            [SerializeField] private UnityEvent unityEventOnEnter;

                [SerializeField] private bool isTriggeredOnce_OnStay = false;
                private bool locked_OnStay = false;
            [SerializeField] private UnityEvent unityEventOnStay;

                [SerializeField] private bool isTriggeredOnce_OnExit = false;
                private bool locked_OnExit = false;
            [SerializeField] private UnityEvent unityEventOnExit;

        [Space(0)]

        [Header("Accept Triggers on 2D/3D/Both")]
            [SerializeField] private ENUM_TriggerType triggerType = ENUM_TriggerType.triggerType_Both;

        private bool isSetupRequired = true;

        #region Setup
            void Awake(){ Setup(); }
            public void Setup()
            {
                if (!isSetupRequired){ return; }

                // 3D
                foreach (var target in watchedColliders)
                {
                    if (target.gameObject.GetComponent<UnityEventOnCollision>() != null){ continue; }

                    UnityEventOnCollision copyTrigger = target.gameObject.AddComponent<UnityEventOnCollision>();
                    copyTrigger.SetupThisCopy(this);
                }
                // 2D
                foreach (var target in watchedColliders2D)
                {
                    if (target.gameObject.GetComponent<UnityEventOnCollision>() != null){ continue; }

                    UnityEventOnCollision copyTrigger = target.gameObject.AddComponent<UnityEventOnCollision>();
                    copyTrigger.SetupThisCopy(this);
                }

                isSetupRequired = false;
            }
                public void SetupThisCopy(UnityEventOnCollision source)
                {
                    unityEventOnEnter = source.unityEventOnEnter;
                    unityEventOnStay = source.unityEventOnStay;
                    unityEventOnExit = source.unityEventOnExit;

                    triggerType = source.triggerType;

                    isSetupRequired = false;
                }
        #endregion

        #region 2D
            // Enter
            void OnTriggerEnter2D(Collider2D other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_3D){ return; }
                if (locked_OnEnter){ return; }
                
                unityEventOnEnter.Invoke();
            
                if (isTriggeredOnce_OnEnter){ locked_OnEnter = true; }
            }
            void OnCollisionEnter2D(Collision2D other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_3D){ return; }
                if (locked_OnEnter){ return; }
                
                unityEventOnEnter.Invoke();
            
                if (isTriggeredOnce_OnEnter){ locked_OnEnter = true; }
            }

            // Exit
            void OnTriggerStay2D(Collider2D other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_3D){ return; }
                if (locked_OnStay){ return; }
                
                unityEventOnStay.Invoke();
            
                if (isTriggeredOnce_OnStay){ locked_OnStay = true; }
            }
            void OnCollisionStay2D(Collision2D other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_3D){ return; }
                if (locked_OnStay){ return; }
                
                unityEventOnStay.Invoke();
            
                if (isTriggeredOnce_OnStay){ locked_OnStay = true; }
            }

            // Exit
            void OnTriggerExit2D(Collider2D other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_3D){ return; }
                if (locked_OnExit){ return; }
                
                unityEventOnExit.Invoke();
            
                if (isTriggeredOnce_OnExit){ locked_OnExit = true; }
            }
            void OnCollisionExit2D(Collision2D other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_3D){ return; }
                if (locked_OnExit){ return; }
                
                unityEventOnExit.Invoke();
            
                if (isTriggeredOnce_OnExit){ locked_OnExit = true; }
            }
        #endregion

        #region 3D
            // Enter
            void OnTriggerEnter(Collider other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_2D){ return; }
                if (locked_OnEnter){ return; }
                
                unityEventOnEnter.Invoke();
            
                if (isTriggeredOnce_OnEnter){ locked_OnEnter = true; }
            }
            void OnCollisionEnter(Collision other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_2D){ return; }
                if (locked_OnEnter){ return; }
                
                unityEventOnEnter.Invoke();
            
                if (isTriggeredOnce_OnEnter){ locked_OnEnter = true; }
            }

            // Exit
            void OnTriggerStay(Collider other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_2D){ return; }
                if (locked_OnStay){ return; }
                
                unityEventOnStay.Invoke();
            
                if (isTriggeredOnce_OnStay){ locked_OnStay = true; }
            }
            void OnCollisionStay(Collision other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_2D){ return; }
                if (locked_OnStay){ return; }
                
                unityEventOnStay.Invoke();
            
                if (isTriggeredOnce_OnStay){ locked_OnStay = true; }
            }

            // Exit
            void OnTriggerExit(Collider other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_2D){ return; }
                if (locked_OnExit){ return; }
                
                unityEventOnExit.Invoke();
            
                if (isTriggeredOnce_OnExit){ locked_OnExit = true; }
            }
            void OnCollisionExit(Collision other)
            {
                if (triggerType == ENUM_TriggerType.triggerType_2D){ return; }
                if (locked_OnExit){ return; }
                
                unityEventOnExit.Invoke();
            
                if (isTriggeredOnce_OnExit){ locked_OnExit = true; }
            }
        #endregion
    }

    enum ENUM_TriggerType
    {
        triggerType_Both,
        triggerType_2D,
        triggerType_3D
    }
}