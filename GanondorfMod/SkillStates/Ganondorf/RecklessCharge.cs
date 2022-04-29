using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace GanondorfMod.SkillStates.Ganondorf
{
    internal class RecklessCharge : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.jumpPower = 0f;

        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
