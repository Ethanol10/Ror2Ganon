using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates
{
    internal class ObliterateEnd : BaseSkillState
    {

        public float damage;
        private float animTimer;

        public override void OnEnter()
        {
            base.OnEnter();

            base.PlayCrossfade("FullBody, Override", "EndForwardSmash", 0.1f);
            animTimer = 1.55f;

        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayCrossfade("FullBody, Override", "BufferEmpty", 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > animTimer) 
            {
                base.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
        }
    }
}
