using EntityStates;
using GanondorfMod.Modules.Survivors;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates.Ganondorf
{
    internal class SwordThrow : BaseSkillState
    {

        internal float baseThrowTime = 2.5f;
        internal float throwTime;

        internal float baseThrowStart = 0.15f;
        internal float throwStart;

        internal float throwEnd;
        internal bool swordSpawned;

        internal float damage;
        internal float maxDamage;
        internal float damageIncrementor;

        internal float distance;
        internal float maxDistance;
        internal float distanceIncrementor;

        internal float lastAnimStopwatch;

        internal GanondorfController ganoncon;
        
        public override void OnEnter()
        {
            base.OnEnter();

            base.GetModelAnimator().SetFloat("Slash.playbackRate", attackSpeedStat);
            swordSpawned = false;
            throwTime = baseThrowTime / this.attackSpeedStat;
            ganoncon = GetComponent<GanondorfController>();
            swordSpawned = false;
            lastAnimStopwatch = 0f;
            if (isGrounded) 
            {
                base.SmallHop(base.characterMotor, 15f);
            }
        }

        public override void OnExit()
        {
            base.PlayCrossfade("Sheathe, Override", "Empty", 0.2f);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority) 
            {
                lastAnimStopwatch += Time.fixedDeltaTime;
                if (!swordSpawned)
                {
                    base.PlayCrossfade("Sheathe, Override", "Throw", "Slash.playbackRate", throwTime, 0.1f);
                    swordSpawned = true;
                    //Throw Sword
                    GameObject swordProjectile = UnityEngine.Object.Instantiate(Modules.Assets.swordObject, ganoncon.handRight);
                    ThrownSwordContainer swordAttributes = swordProjectile.AddComponent<ThrownSwordContainer>();
                    swordAttributes.distanceToThrow = distance;
                    swordAttributes.startingPosition = base.transform.position;
                    swordAttributes.isReal = true;
                    swordAttributes.charBody = base.characterBody;
                    swordAttributes.damageToDeal = damage;
                    ganoncon.TempDisableSword();
                    //Later spawn a "projectile" on all machines using a network request.
                }

                if (swordSpawned && lastAnimStopwatch > throwTime)
                {
                    base.outer.SetNextStateToMain();
                }
            }

            if (NetworkServer.active) 
            {
                lastAnimStopwatch += Time.fixedDeltaTime;
                if (!swordSpawned)
                {
                    base.PlayCrossfade("Sheathe, Override", "Throw", "Slash.playbackRate", throwTime, 0.1f);
                    swordSpawned = true;
                    //Throw Sword
                    GameObject swordProjectile = UnityEngine.Object.Instantiate(Modules.Assets.swordObject, ganoncon.handRight);
                    ThrownSwordContainer swordAttributes = swordProjectile.AddComponent<ThrownSwordContainer>();
                    swordAttributes.distanceToThrow = distance;
                    swordAttributes.startingPosition = base.transform.position;
                    swordAttributes.isReal = false;
                    swordAttributes.charBody = base.characterBody;
                    swordAttributes.damageToDeal = 0f;
                    ganoncon.TempDisableSword();
                    //Later spawn a "projectile" on all machines using a network request.
                }

                if (swordSpawned && lastAnimStopwatch > throwTime)
                {
                    base.outer.SetNextStateToMain();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
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
