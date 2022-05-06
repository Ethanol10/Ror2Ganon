using EntityStates;
using GanondorfMod.Modules.Survivors;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GanondorfMod.SkillStates.Ganondorf
{
    internal class SwordCharge : BaseSkillState
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
            throwTime = baseThrowTime / base.attackSpeedStat;
            throwEnd = throwTime / 0.6f;
            throwStart = baseThrowStart / base.attackSpeedStat;
            base.PlayAnimation("RightArm, Override", "preemptiveThrow", "Slash.playbackRate", throwTime);
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
            base.PlayAnimation("RightArm, Override", "Empty");
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority) 
            {
                if (base.inputBank.skill3.down)
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
                else if (!base.inputBank.skill3.down)
                {
                    EntityStateMachine[] stateMachines = characterBody.gameObject.GetComponents<EntityStateMachine>();
                    //"No statemachines?"
                    if (!stateMachines[0])
                    {
                        Debug.LogWarning("StateMachine search failed! Wrong object?");
                    }
                    else 
                    {
                        foreach (EntityStateMachine stateMachine in stateMachines)
                        {
                            if (stateMachine.customName == "Slide")
                            {
                                stateMachine.SetState(new SwordThrow {
                                    distance = distance,
                                    damage = damage
                                });
                            }
                        }
                    }

                    base.outer.SetNextStateToMain();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
