using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace GanondorfMod.SkillStates
{
    internal class SpawnState : BaseState
    {
        public float spawnDuration = 2.5f;
        public bool hasplayedSound;

        public override void OnEnter() 
        {
            base.OnEnter();
            hasplayedSound = false;
            base.PlayAnimation("Body", "SpawnIn", "Slash.playbackRate", spawnDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > 0.2f && !hasplayedSound) 
            {
                hasplayedSound = true;
                Util.PlaySound("spawnVoice", gameObject);
                Util.PlaySound("Spawning", gameObject);
            }
            if (base.fixedAge > spawnDuration) 
            {
                base.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
