using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        [CreateAssetMenu(fileName = "[Effect] New [Logic][Queue]", menuName = "EFFECT/LOGIC/++QUEUE || Sequential effects each waiting for the previous to conclude before applying!")]
        public class EFFECT_Queue : soDATA_Effect
        {
            [Space(10)]

            [Header("-Queue-")]
            public List<soDATA_Effect> queuedEffects;

            public override void Apply(GameObject target = null)
            {
                CoroutineRunner.Run(RunQueue());

                base.Apply(target);
            }

            IEnumerator RunQueue()
            {
                foreach (var effect in queuedEffects)
                {
                    if (effect == null)
                        continue;

                    // Wait for the effect to finish
                    yield return effect.ApplyRoutine();
                }
            }
        }
    }
}