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

        internal float rampingSpeed;
        internal float rampingSpeedIncrement;

        internal float baseEndHitTimer;
        internal float endHitTimer;
        internal bool isFired;
        internal float finalTimeHeld;

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
        private Ray aimRay;

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
            finalTimeHeld = 0f;

            rampingSpeed = Modules.StaticValues.recklessChargeInitialChargeSpeed;

            isFired = false;
            stateTimer = 0f;
            hasPlayedAnim = false;
            maxSpeed = base.moveSpeedStat * Modules.StaticValues.recklessChargeFinalChargeSpeedMultiplier;

            rampingSpeedIncrement = (maxSpeed - rampingSpeed) / Modules.StaticValues.recklessChargeTimeToFullSpeed ;
            this.GetModelAnimator().SetFloat("Slash.playbackRate", attackSpeedStat);
            ganoncon = base.GetComponent<GanondorfController>();
            ganoncon.SwapToSword();
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterBody.jumpPower = originalJumpPower;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stateTimer += Time.fixedDeltaTime;
            aimRay = base.GetAimRay();

            switch (state) 
            {
                case ChargeState.START:
                    base.characterMotor.velocity = Vector3.zero;
                    if (!hasPlayedAnim) 
                    {
                        hasPlayedAnim = true;
                        base.PlayAnimation("FullBody, Override", "StartingSwordCharge", "Slash.playbackRate", initialChargeTimer);
                    }
                    base.characterMotor.rootMotion = aimRay.direction.normalized * rampingSpeed * Time.fixedDeltaTime;
                    base.characterDirection.moveVector = aimRay.direction.normalized;
                    if (stateTimer > initialChargeTimer) 
                    {
                        state = ChargeState.CHARGING;
                        base.PlayAnimation("FullBody, Override", "MidSwordCharge", "Slash.playbackRate", midChargeTimer);
                        base.GetModelAnimator().SetBool("isSwordCharging", true);
                        stateTimer = 0f;
                    }
                    break;
                case ChargeState.CHARGING:
                    if (rampingSpeed < maxSpeed) 
                    {
                        Debug.Log(rampingSpeed);
                        rampingSpeed += rampingSpeedIncrement * Time.fixedDeltaTime;
                    }

                    if (base.isAuthority && base.inputBank.skill2.down)
                    {
                        //Keep running
                        base.characterMotor.moveDirection = aimRay.direction.normalized;
                        base.characterMotor.rootMotion += aimRay.direction.normalized * rampingSpeed * Time.fixedDeltaTime;
                        base.characterDirection.moveVector = aimRay.direction.normalized;
                    }
                    else if (base.isAuthority && !base.inputBank.skill2.down) 
                    {
                        state = ChargeState.END;
                        base.PlayAnimation("FullBody, Override", "ChargeSwordEnd", "Slash.playbackRate", endChargeTimer);
                        finalTimeHeld = stateTimer;
                        stateTimer = 0f;
                        base.characterMotor.rootMotion = Vector3.zero;
                        base.characterDirection.moveVector = aimRay.direction;
                    }
                    break;
                case ChargeState.END:
                    if (stateTimer > endHitTimer && !isFired) 
                    {
                        isFired = true;
                        BlastAttack blastAttack = new BlastAttack();
                        blastAttack.damageType = new DamageTypeCombo(DamageType.Stun1s, DamageTypeExtended.Generic, DamageSource.Utility);
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
                        blastAttack.position = base.transform.position;
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
            if (stateTimer > endHitTimer && state == ChargeState.END) 
            {
                return InterruptPriority.Skill;
            }
            return InterruptPriority.Death;
        }
    }
}
