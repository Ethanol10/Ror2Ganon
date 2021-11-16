using EntityStates;
using System;
using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;
using GanondorfMod.SkillStates.BaseStates;

namespace GanondorfMod.SkillStates
{
    public class WizardFoot : BaseSkillState
    { 
        protected float duration = 0.75f;
        public static float initialSpeedCoefficient = 5f;
        public static float finalSpeedCoefficient = 2f;

        public static string wizardFootSoundString = "wizardsFoot1";
        protected string hitSoundString = "";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        private float rollSpeed;
        private OverlapAttack attack;
        private Vector3 aimRayDir;
        private Animator animator;
        private Vector3 previousPosition;
        private Camera playerCam;
        private Vector3 initialRot;
        private Vector3 kickRot;
        protected float stopwatch;
        private Vector3 playerRot;

        protected DamageType damageType = DamageType.Generic;
        private bool hasFired;
        private float hitPauseTimer;
        private BaseState.HitStopCachedState hitStopCachedState;
        protected NetworkSoundEventIndex impactSound;
        private Vector3 storedVelocity;
        protected float attackStartTime = 0.13f;
        protected float attackEndTime = 0.7f;
        protected float procCoefficient = 1f;
        protected float pushForce = 300f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseEarlyExitTime = 0.4f;
        protected float hitStopDuration = 0.012f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 4f;
        protected bool cancelled = false;
        private float earlyExitTime;
        protected bool inHitPause;


        public override void OnEnter()
        {
            base.OnEnter();
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
            this.hasFired = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            base.characterBody.outOfCombatStopwatch = 0f;

            //Get Animator to play animation.
            this.animator = base.GetModelAnimator();

            //Get ray to point ganon in the right direction
            Ray aimRay = base.GetAimRay();
            this.aimRayDir = aimRay.direction;
            this.kickRot = new Vector3(aimRay.direction.x * Mathf.Rad2Deg, aimRay.direction.y * Mathf.Rad2Deg, aimRay.direction.z * Mathf.Rad2Deg);
            this.initialRot = new Vector3(0, 0, 0);

            //Getting hitboxes
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform) {
                hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(),
                    (HitBoxGroup element) => element.groupName == "kick") ;
            }

            //This governs the info on the attack
            attack = new OverlapAttack();
            attack.damageType = this.damageType;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = Modules.StaticValues.wizardFootDamageCoefficient * this.damageStat;
            attack.procCoefficient = this.procCoefficient;
            attack.forceVector = this.bonusForce;
            attack.pushAwayForce = this.pushForce;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = this.impactSound;

            //Calculate speed of kick and aim
            this.RecalculateRollSpeed();
            //this.RecalculateAimingDir();

            if (base.characterMotor && base.characterDirection)
            {
                //base.characterMotor.velocity.y = 0f;
                base.characterMotor.velocity = this.aimRayDir * this.rollSpeed;
                base.GetModelTransform().Rotate(playerRot);
            }

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;

            base.PlayAnimation("FullBody, Override", "SpecialKickStart", "kick.playbackRate", this.duration);
            Util.PlaySound(WizardFoot.wizardFootSoundString, base.gameObject);
            this.animator.SetBool("isKicking", true);
            this.animator.SetBool("attacking", true);

            if (NetworkServer.active)
            {
                //Add buffs here
                //base.characterBody.AddTimedBuff(Modules.Buffs.armorBuff, 3f * this.duration);
                //base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f * this.duration);
            }
        }

        private void RecalculateRollSpeed()
        {
            this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(WizardFoot.initialSpeedCoefficient, WizardFoot.finalSpeedCoefficient, base.fixedAge / this.duration);
        }

        private void RecalculateAimingDir()
        {
            float t;
            //if (base.fixedAge / WizardFoot.duration < 0.9)
            //{
            //    t = 0.0f;
            //}
            //else {
            t = base.fixedAge / this.duration;
            //}
            this.playerRot = Vector3.Lerp(kickRot, initialRot, t);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateRollSpeed();
            //this.RecalculateAimingDir();

            if (base.characterDirection) base.characterDirection.forward = this.aimRayDir;
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(WizardFoot.dodgeFOV, 60f, base.fixedAge / this.duration);

            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.rollSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, this.aimRayDir), 0f);
                vector = this.aimRayDir * d;

                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
            //base.transform.Rotate(playerRot, Space.Self);

            //Handle Hitpause and momentarily stop ganon from flying for a few milliseconds
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
                if (this.animator) this.animator.SetFloat("kick.playbackRate", 0f);
            }

            //Trigger attack if within the active hitbox timing.
            if (this.stopwatch >= (duration * this.attackStartTime) && this.stopwatch <= (duration * this.attackEndTime))
            {
                FireAttack();
            }

            if (this.duration - base.fixedAge < this.duration-this.attackEndTime)
            { 
                this.animator.SetBool("isKicking", false);
            }
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void FireAttack(){
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlayAttackSpeedSound("wizardFootSE1", base.gameObject, 1.0f);

                if (base.isAuthority)
                {
                    //Effects and recoil
                    //this.PlaySwingEffect();
                    base.AddRecoil(-1f * this.attackRecoil, -2f * this.attackRecoil, -0.5f * this.attackRecoil, 0.5f * this.attackRecoil);
                }
            }

            if (base.isAuthority)
            {
                //Fire attack.
                if(this.attack.Fire())
                {
                    this.OnHitEnemyAuthority();
                }
            }
        }

        protected virtual void OnHitEnemyAuthority() {
            Util.PlaySound(this.hitSoundString, base.gameObject);

            if (!this.inHitPause && this.hitStopDuration > 0f)
            {
                storedVelocity = base.characterMotor.velocity;
                hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, animator, "kick.playbackRate");
                hitPauseTimer = hitStopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }

        public override void OnExit()
        {
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.OnExit();
            this.animator.SetBool("attacking", false);
            base.characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.aimRayDir);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.aimRayDir = reader.ReadVector3();
        }
    }
}