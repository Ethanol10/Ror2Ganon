using EntityStates;
using GanondorfMod.Modules.Survivors;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

        internal GanondorfController ganoncon;
        
        public override void OnEnter()
        {
            base.OnEnter();

            base.GetModelAnimator().SetFloat("Slash.playbackRate", attackSpeedStat);
            throwTime = baseThrowTime / base.attackSpeedStat;
            throwEnd = throwTime / 0.6f;
            throwStart = baseThrowStart / base.attackSpeedStat;
            base.PlayAnimation("Sheathe, Override", "preemptiveThrow", "Slash.playbackRate", throwTime);
            ganoncon = GetComponent<GanondorfController>();
            ganoncon.SwapToRightHand();
            swordSpawned = false;

            damage = Modules.StaticValues.swordBaseDamageCoefficient * base.damageStat;
            maxDamage = damage * Modules.StaticValues.swordMaximumDamageMultiplier;
            damageIncrementor = (maxDamage - damage) / (Modules.StaticValues.swordTimeToMaxCharge / attackSpeedStat);

            distance = Modules.StaticValues.swordThrowBaseDistance;
            maxDistance = Modules.StaticValues.swordThrowMaxDistance;
            distanceIncrementor = (maxDistance - distance) / (Modules.StaticValues.swordTimeToMaxCharge / attackSpeedStat);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.inputBank.skill2.down)
            {
                if (damage < maxDamage) 
                {
                    damage += damageIncrementor * Time.fixedDeltaTime;
                }
                if (distance < maxDistance) 
                {
                    distance += distanceIncrementor * Time.fixedDeltaTime;
                }
            }
            else if (!base.inputBank.skill2.down) 
            {
                if (!swordSpawned) 
                {
                    swordSpawned = true;
                    //Throw Sword
                    GameObject swordProjectile = UnityEngine.Object.Instantiate(Modules.Assets.swordObject, ganoncon.handRight);
                    ThrownSwordContainer swordAttributes = swordProjectile.AddComponent<ThrownSwordContainer>();
                    swordAttributes.distanceToThrow = distance;
                    swordAttributes.startingPosition = base.transform.position;
                    swordAttributes.isReal = true;
                    swordAttributes.charBody = base.characterBody;
                    ganoncon.TempDisableSword();
                    base.outer.SetNextStateToMain();
                    //Later spawn projectile on all machines using a network request.
                }
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
