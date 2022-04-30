using EntityStates;
using GanondorfMod.Modules.Survivors;
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
        internal bool hasPlayedAnim;

        internal float stateTimer;

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
            stateTimer = 0f;
            hasPlayedAnim = false;
            maxSpeed = base.moveSpeedStat * Modules.StaticValues.recklessChargeFinalChargeSpeedMultiplier; 


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
                    }
                    break;
                case ChargeState.CHARGING:
                    break;
                case ChargeState.END:
                    base.outer.SetNextStateToMain();
                    break;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
