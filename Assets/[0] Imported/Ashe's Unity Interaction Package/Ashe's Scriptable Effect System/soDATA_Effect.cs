using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using NaughtyAttributes;
using System;

namespace Ash {
    namespace SOEffectSystem {
        //INHERITED CLASS TO ALLOW ALL CHILDREN TO BE APPLIED GENERICALLY ALLOWING CODE TO BE CREATED AND RUN FROM SOs DYNAMICALLY AND IN AN OBJECT ORIENTED SYSTEM//
        //REPRESENTS THE RESULTS OF AN ACTION//
        public class soDATA_Effect : ScriptableObject
        {
            [Header("-Effect-")]
            [Label("Chained Effects")] public List<soDATA_Effect> chained;
            [Label("Chained Game Events")] public List<soDATA_GameEvent> triggeredEvents;

            public virtual void Apply(GameObject target = null){
                foreach (soDATA_GameEvent gameEvent in triggeredEvents){ gameEvent.Raise(); }

                foreach(soDATA_Effect effect in chained){ if (effect != null){ effect.Apply(); }}
                }

                public virtual IEnumerator ApplyRoutine(GameObject target = null){ Apply(target); yield break;}
        }

        [Serializable]
        public class TimedEffect
        {
            [Range(0,60)] public float delay;
            public soDATA_Effect effect;
        }
    }
}