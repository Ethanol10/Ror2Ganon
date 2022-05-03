using EntityStates;
using GanondorfMod.Modules.Survivors;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GanondorfMod.SkillStates.Ganondorf
{
    internal class RecklessCharge : BaseSkillState
    {
        internal float baseInitialChargeTimer;
        internal float initialChargeTimer;
        internal float baseMidChargeTimer;
        internal float midChargeTimer;
        internal bool hasPlayedAnim;
        internal float baseEndChargeTimer;
        internal float endChargeTimer;
        internal float stateTimer;

        internal float baseEndHitTimer;
        internal float endHitTimer;
        internal bool isFired;

        internal GanondorfController ganoncon;
        public enum ChargeState
        {
            START = 1,
            CHARGING = 2,
            END = 3
        }

        internal float originalMaxSpeed;
        internal float maxSpeed;
        public float originalJumpPower;
        public ChargeState state;

        public override void OnEnter()
        {
            base.OnEnter();
            originalJumpPower = base.characterBody.jumpPower;
            base.characterBody.jumpPower = 0f;
            state = ChargeState.START;
            baseInitialChargeTimer = 0.5f;
            initialChargeTimer = baseInitialChargeTimer / base.attackSpeedStat;
            baseMidChargeTimer = 1.6f;
            midChargeTimer = baseMidChargeTimer / base.attackSpeedStat;
            baseEndChargeTimer = 1.89f;
            endChargeTimer = baseEndChargeTimer / base.attackSpeedStat;
            baseEndHitTimer = 0.35f;
            endHitTimer = baseEndHitTimer / base.attackSpeedStat;

            isFired = false;
            stateTimer = 0f;
            hasPlayedAnim = false;
            maxSpeed = base.moveSpeedStat * Modules.StaticValues.recklessChargeFinalChargeSpeedMultiplier;
            this.GetModelAnimator().SetFloat("Slash.playbackRate", attackSpeedStat);

            ganoncon.SwapToSword();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stateTimer += Time.fixedDeltaTime;

            switch (state) 
            {
                case ChargeState.START:
                    base.characterMotor.velocity = Vector3.zero;
                    if (!hasPlayedAnim) 
                    {
                        hasPlayedAnim = true;
                        base.PlayAnimation("FullBody, Override", "StartingSwordCharge", "Slash.playbackRate", initialChargeTimer);
                    }
                    if(stateTimer > initialChargeTimer) 
                    {
                        state = ChargeState.CHARGING;
                        base.PlayAnimation("FullBody, Override", "MidSwordCharge", "Slash.playbackRate", midChargeTimer);
                        base.GetModelAnimator().SetBool("isSwordCharging", true);
                        stateTimer = 0f;
                    }
                    break;
                case ChargeState.CHARGING:
                    if (base.isAuthority && base.inputBank.skill3.down)
                    {

                    }
                    else if (base.isAuthority && !base.inputBank.skill3.down) 
                    {
                        state = ChargeState.END;
                        base.PlayAnimation("FullBody, Override", "ChargeSwordEnd", "Slash.playbackRate", endChargeTimer);
                        stateTimer = 0f;
                    }
                    break;
                case ChargeState.END:
                    if (stateTimer > endHitTimer && !isFired) 
                    {
                        isFired = true;
                        BlastAttack blastAttack = new BlastAttack();
                        blastAttack.damageType = DamageType.Stun1s;
                        blastAttack.attacker = base.gameObject;
                        blastAttack.inflictor = base.gameObject;
                        blastAttack.teamIndex = base.teamComponent.teamIndex;
                        blastAttack.baseDamage = this.damageStat;
                        blastAttack.procCoefficient = 1.0f;
                        blastAttack.radius = 10f;
                        blastAttack.bonusForce = Vector3.zero;
                        blastAttack.baseForce = 1000f;
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        blastAttack.crit = base.RollCrit();
                        blastAttack.Fire();
                    }
                    if (stateTimer > endChargeTimer) 
                    {
                        base.outer.SetNextStateToMain();
                    }
                    break;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
