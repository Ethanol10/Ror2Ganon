using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
            GameObject portalEffect = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/NullifierSpawnEffect");
            //Vector3 pos = self.gameObject.transform.position;
            Quaternion rot = base.gameObject.transform.rotation;
            rot = Quaternion.Euler(0, 90, 0);
            //pos.x += 1.5f;
            //pos.z -= 1.5f;
            EffectData effectData = new EffectData
            {
                origin = base.transform.position,
                rotation = rot,
            };
            EffectManager.SpawnEffect(portalEffect, effectData, true);
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
