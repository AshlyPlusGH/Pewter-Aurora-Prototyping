using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        public class EFFECT_Canister : MonoBehaviour
        {
            [SerializeField] soDATA_GameEvent eventTrigger;
            [SerializeField] soDATA_GameEvent eventToActivate;

            [SerializeField] List<soDATA_Effect> effects = new List<soDATA_Effect>();
            List<soDATA_Effect> inputEffects;

            void Awake(){ if (eventTrigger != null){ eventTrigger.RegisterListener(Activate); } inputEffects = new List<soDATA_Effect>(effects);}

            public void AddEffectsToOnCardPlayed(List<soDATA_Effect> newEffects){ foreach(soDATA_Effect effect in newEffects){ effects.Add(effect); }}

            public void Activate(){ if (eventToActivate != null){ eventToActivate.Raise(); } foreach (soDATA_Effect effect in effects){ effect.Apply(); }}

            public void Clear(){ effects.Clear(); foreach (soDATA_Effect effect in inputEffects){ effects.Add(effect); }}
        }
    }
}