using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        [CreateAssetMenu(fileName = "StringToBePrinted", menuName = "EFFECT/DEBUGGING/++TEST || Logs this effect's name when applied!")]
        public class EFFECT_Test : soDATA_Effect
        {
            public override void Apply(GameObject target = null)
            {
                Debug.Log("Testing Effect: " + name);
                
                base.Apply(target);
            }
        }
    }
}