using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        [CreateAssetMenu(fileName = "[Effect] New [Logic][Queue][Triggered]", menuName = "EFFECT/LOGIC/++TRIGGERQUEUE || Sequential effects each waiting for the next apply command!")]
        public class EFFECT_TriggerQueue : soDATA_Effect
        {
            [Space(10)]

            [Header("-Trigger Queue-")]
            public List<soDATA_Effect> queuedEffects;

            public override void Apply(GameObject target = null)
            {
                queuedEffects[0].Apply();
                queuedEffects.RemoveAt(0);

                base.Apply(target);
            }
        }
    }
}