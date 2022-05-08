using EntityStates;
using GanondorfMod.Modules.Networking;
using GanondorfMod.Modules.Survivors;
using R2API.Networking;
using R2API.Networking.Interfaces;
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
        internal bool isFullyCharged;
        internal bool sentRequest;

        internal GanondorfController ganoncon;
        
        public override void OnEnter()
        {
            base.OnEnter();

            base.GetModelAnimator().SetFloat("Slash.playbackRate", attackSpeedStat);
            throwTime = baseThrowTime / base.attackSpeedStat;
            throwEnd = throwTime / 0.6f;
            throwStart = baseThrowStart / base.attackSpeedStat;
            base.PlayCrossfade("RightArm, Override", "preemptiveThrow", "Slash.playbackRate", throwTime, 0.1f);
            ganoncon = GetComponent<GanondorfController>();
            ganoncon.SwapToRightHand();
            swordSpawned = false;

            damage = Modules.StaticValues.swordBaseDamageCoefficient * base.damageStat;
            maxDamage = damage * Modules.StaticValues.swordMaximumDamageMultiplier;
            damageIncrementor = (maxDamage - damage) / (Modules.StaticValues.swordTimeToMaxCharge / attackSpeedStat);

            distance = Modules.StaticValues.swordThrowBaseDistance;
            maxDistance = Modules.StaticValues.swordThrowMaxDistance;
            distanceIncrementor = (maxDistance - distance) / (Modules.StaticValues.swordTimeToMaxCharge / attackSpeedStat);
            isFullyCharged = true;
            new ChargingSwordNetworkRequest(characterBody.masterObjectId, true).Send(NetworkDestination.Clients);
            sentRequest = false;

            AkSoundEngine.PostEvent(4208541365, base.gameObject);
        }

        public override void OnExit()
        {
            base.PlayCrossfade("RightArm, Override", "Empty", 0.1f);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority) 
            {
                if (base.inputBank.skill2.down)
                {
                    if (damage < maxDamage)
                    {
                        damage += damageIncrementor * Time.fixedDeltaTime;
                        isFullyCharged = true;
                    }
                    if (distance < maxDistance)
                    {
                        distance += distanceIncrementor * Time.fixedDeltaTime;
                        isFullyCharged = true;
                    }

                    if ((damage >= maxDamage || distance >= maxDistance) && isFullyCharged)
                    {
                        if (!sentRequest) 
                        {
                            sentRequest = true;
                            AkSoundEngine.PostEvent(2184839552, base.gameObject);
                            new ChargingSwordNetworkRequest(characterBody.masterObjectId, false).Send(NetworkDestination.Clients);
                            new FullyChargedSwordNetworkRequest(characterBody.masterObjectId, true).Send(NetworkDestination.Clients);
                        }
                    }
                }
                else if (!base.inputBank.skill2.down)
                {
                    AkSoundEngine.PostEvent(1506146985, base.gameObject);
                    new ChargingSwordNetworkRequest(characterBody.masterObjectId, false).Send(NetworkDestination.Clients);
                    new FullyChargedSwordNetworkRequest(characterBody.masterObjectId, false).Send(NetworkDestination.Clients);
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
