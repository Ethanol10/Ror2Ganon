﻿using EntityStates;
using RoR2;
using RoR2.Audio;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates.BaseStates
{
    public class BaseMeleeAttack : BaseSkillState
    {
        public int swingIndex;

        protected string hitboxName = "melee";

        protected DamageType damageType = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Primary);
        protected float damageCoefficient;
        protected float procCoefficient;
        protected float pushForce;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseDuration;
        protected float attackStartTime;
        protected float attackEndTime;
        protected float baseEarlyExitTime;
        protected float hitStopDuration;
        protected float attackRecoil;
        protected float hitHopVelocity;
        //protected bool cancelled = false;

        protected string swingSoundString = "";
        protected string hitSoundString = "";
        protected string muzzleString = "SwingCenter";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound;

        protected float earlyExitTime;
        public float duration;
        protected bool hasFired;
        protected float hitPauseTimer;
        protected OverlapAttack attack;
        protected bool inHitPause;
        protected bool hasHopped;
        protected float stopwatch;
        protected Animator animator;
        protected BaseState.HitStopCachedState hitStopCachedState;
        protected Vector3 storedVelocity;
        protected bool isRunning;

        //public override void OnEnter()
        //{
        //    base.OnEnter();
        //    //this.duration = this.baseDuration / this.attackSpeedStat;
        //    //this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
        //    this.hasFired = false;
        //    this.animator = base.GetModelAnimator();
        //    base.StartAimMode(0.5f + this.duration, false);
        //    //base.characterBody.outOfCombatStopwatch = 0f;
        //    this.animator.SetBool("attacking", true);

        //    //HitBoxGroup hitBoxGroup = null;
        //    //Transform modelTransform = base.GetModelTransform();

        //    //if (modelTransform)
        //    //{
        //    //    hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == this.hitboxName);
        //    //}

        //    //this.attack = new OverlapAttack();
        //    //this.attack.damageType = this.damageType;
        //    //this.attack.attacker = base.gameObject;
        //    //this.attack.inflictor = base.gameObject;
        //    //this.attack.teamIndex = base.GetTeam();
        //    //this.attack.damage = this.damageCoefficient * this.damageStat;
        //    //this.attack.procCoefficient = this.procCoefficient;
        //    //this.attack.hitEffectPrefab = this.hitEffectPrefab;
        //    //this.attack.forceVector = this.bonusForce;
        //    //this.attack.pushAwayForce = this.pushForce;
        //    //this.attack.hitBoxGroup = hitBoxGroup;
        //    //this.attack.isCrit = base.RollCrit();
        //    //this.attack.impactSound = this.impactSound;
        //}

        public override void OnExit()
        {
            //if (!this.hasFired /*&& !this.cancelled*/) this.FireAttack();
            
            base.OnExit();
        }

        protected virtual void PlayAttackAnimation()
        {
            //base.PlayCrossfade("Gesture, Override", "Punch", "punch.playbackRate", this.duration, 0.05f);
        }

        protected virtual void PlaySwingEffect()
        {
            EffectManager.SimpleMuzzleFlash(this.swingEffectPrefab, base.gameObject, this.muzzleString, true);
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound(this.hitSoundString, base.gameObject);

            if (!this.hasHopped)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded && this.hitHopVelocity > 0f)
                {
                    base.SmallHop(base.characterMotor, this.hitHopVelocity);
                }

                this.hasHopped = true;
            }

            if (!this.inHitPause && this.hitStopDuration > 0f)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "punch.playbackRate");
                this.hitPauseTimer = this.hitStopDuration / this.attackSpeedStat;
                this.inHitPause = true;
            }
        }

        public virtual void FireAttack()
        {
            if (!this.hasFired)
            {
                this.PlayAttackAnimation();
                this.hasFired = true;
                Util.PlayAttackSpeedSound(this.swingSoundString, base.gameObject, this.attackSpeedStat);

                if (base.isAuthority)
                {
                    this.PlaySwingEffect();
                    base.AddRecoil(-1f * this.attackRecoil, -2f * this.attackRecoil, -0.5f * this.attackRecoil, 0.5f * this.attackRecoil);
                }
            }

            if (base.isAuthority)
            {
                if (this.attack.Fire())
                {
                    this.OnHitEnemyAuthority();
                }
            }
        }

        protected virtual void SetNextState()
        {
            int index = this.swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            this.outer.SetNextState(new BaseMeleeAttack
            {
                swingIndex = index
            });
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.hitPauseTimer -= Time.fixedDeltaTime;
            this.stopwatch += Time.fixedDeltaTime;

            if (this.hitPauseTimer <= 0f && this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
                base.characterMotor.velocity = this.storedVelocity;
            }

            if (!this.inHitPause)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("punch.playbackRate", 0f);
            }

            if (this.stopwatch >= (this.duration * this.attackStartTime) && this.stopwatch <= (this.duration * this.attackEndTime))
            {
                this.FireAttack();
            }

            //if (this.stopwatch >= (this.duration - this.earlyExitTime) && base.isAuthority)
            //{
            //    if (base.inputBank.skill1.down)
            //    {
            //        if (!this.hasFired) this.FireAttack();
            //        this.SetNextState();
            //        return;
            //    }
            //}

            if (this.stopwatch >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.swingIndex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.swingIndex = reader.ReadInt32();
        }
    }
}