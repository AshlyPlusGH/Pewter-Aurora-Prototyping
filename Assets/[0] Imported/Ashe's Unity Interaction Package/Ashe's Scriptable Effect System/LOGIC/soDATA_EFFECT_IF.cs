using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        public class soDATA_EFFECT_IF : soDATA_Effect
        {
            [Space(10)]

            [Header("-IF-")]
            [SerializeField] List<soDATA_Effect> conditionalEffects;
            [SerializeField] List<soDATA_Effect> falseConditionalEffects;

            public virtual void Chain(){ foreach (soDATA_Effect effect in conditionalEffects){ effect.Apply(); }}
            public virtual void NotChain(){ foreach (soDATA_Effect effect in falseConditionalEffects){ effect.Apply(); }}
        }
    }
}