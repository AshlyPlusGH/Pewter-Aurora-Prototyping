using UnityEngine;

namespace Ash {
    namespace SOEffectSystem {
        [CreateAssetMenu(
            fileName = "[Effect] New [Logic][IfGamerule]",
            menuName = "EFFECT/LOGIC/++IFGAMERULE || Applies an IF statement to the chain. This one uses enums to concern GameRules stored in the RULES.cs class!"
        )]
        public class EFFECT_IF_Gamerule : soDATA_EFFECT_IF
        {
            [Space(10)]

            [Header("-IF Gamerule-")]
            [SerializeField] GameRuleTarget targetRule;

            public override void Apply(GameObject target = null)
            {
                switch (targetRule)
                {
                    case GameRuleTarget.None:
                        break;
                    /*
                    case GameRuleTarget.RULE_tutorialEnabled:
                        if (RULES.RULE_tutorialEnabled){ Chain(); }
                        else{ NotChain(); }
                        break;
                    */
                }

                base.Apply(target);
            }
        }

        enum GameRuleTarget
        {
            None
            //RULE_tutorialEnabled
        }
    }
}