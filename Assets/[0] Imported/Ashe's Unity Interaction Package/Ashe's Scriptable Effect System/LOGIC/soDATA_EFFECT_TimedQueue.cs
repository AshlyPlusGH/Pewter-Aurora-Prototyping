using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        [CreateAssetMenu(fileName = "[Effect] New [Logic][Queue][Timed]", menuName = "EFFECT/LOGIC/++TIMEDQUEUE || A linked queue of effects who wait both for their effects to resolve and for their delay to elapse before the next effect is applied!")]
        public class EFFECT_TimedQueue : soDATA_Effect
        {
            [Space(10)]

            [Header("-Timed Queue-")]
            public List<TimedEffect> queued;

            public override void Apply(GameObject target = null)
            {
                CoroutineRunner.Run(RunQueue());

                base.Apply(target);
            }

            IEnumerator RunQueue()
            {
                foreach (var timed in queued)
                {
                    yield return new WaitForSeconds(timed.delay);

                    yield return timed.effect.ApplyRoutine();
                }
            }
        }
    }
}