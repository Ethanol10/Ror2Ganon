using EntityStates;
using System;
using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;
using GanondorfMod.Modules.Survivors;
using GanondorfMod.Modules;

namespace GanondorfMod.SkillStates
{
    public class WizardFoot : BaseSkillState
    { 
        protected float duration = 0.75f;
        public static float initialSpeedCoefficient = 3.0f;
        public static float finalSpeedCoefficient = 1.5f;

        public static string wizardFootSoundString = "wizardsFootVoice";
        protected string hitSoundString = "lightHitSFX";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        private float rollSpeed;
        private OverlapAttack attack;
        private Vector3 aimRayDir;
        private Animator animator;
        private Vector3 previousPosition;
        protected float stopwatch;
        private Vector3 playerRot;

        protected DamageTypeCombo damageType = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Secondary);
        private bool hasFired;
        private float hitPauseTimer;
        private BaseState.HitStopCachedState hitStopCachedState;
        protected NetworkSoundEventIndex impactSound;
        private Vector3 storedVelocity;
        protected float attackStartTime = 0.13f;
        protected float attackEndTime = 0.7f;
        protected float procCoefficient = 1f;
        protected float pushForce = 1000f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseEarlyExitTime = 0.75f;
        protected float hitStopDuration = 0.1f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 4f;
        protected bool cancelled = false;
        private float earlyExitTime;
        protected bool inHitPause;
        private Transform modelTrans;
        private Quaternion _lookRot;
        private GanondorfController ganonController;
        private TriforceBuffComponent triforceBuffCon;

        private bool isSecondary;
        private bool isBoosted;

        public override void OnEnter()
        {
            base.OnEnter();
            this.earlyExitTime = this.baseEarlyExitTime;
            this.hasFired = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            base.characterBody.outOfCombatStopwatch = 0f;
            isSecondary = false;
            isBoosted = false;
            ganonController = base.GetComponent<GanondorfController>();
            triforceBuffCon = base.GetComponent<TriforceBuffComponent>();

            //Get Animator to play animation.
            this.animator = base.GetModelAnimator();

            //Get ray to point ganon in the right direction
            Ray aimRay = base.GetAimRay();
            this.aimRayDir = aimRay.direction;

            //Get Point to rotate towards.
            this.playerRot = aimRay.direction;
            modelTrans = base.GetModelTransform();
            _lookRot = Quaternion.LookRotation(this.aimRayDir);

            //Getting hitboxes
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform) {
                hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(),
                    (HitBoxGroup element) => element.groupName == "kick") ;
            }

            //Enable particle effects
            //ganonController.FootRFire.Play();
            ganonController.SwapToFist();
            ganonController.KneeRSpeedLines.Play();

            //Change the move based on which button was pressed.
            float damage = 0;
            if (base.inputBank.skill2.down){
                isSecondary = true;
                damage = (Modules.StaticValues.wizardFootDamageCoefficient * this.damageStat) + (this.damageStat * (this.moveSpeedStat / 50.0f));
                initialSpeedCoefficient = 3.0f;
            }
            else if (base.inputBank.skill3.down) {
                isSecondary = false;
                float boost = 1.0f;
                if (triforceBuffCon.GetBuffCount() >= Modules.StaticValues.utilityStackConsumption) {
                    isBoosted = true;
                    boost = Modules.StaticValues.utilityBoostCoefficient;
                    ganonController.BodyLightning.Play();
                }
                damage = (Modules.StaticValues.wizardFootAltDamageCoefficient * this.damageStat * boost) + (this.damageStat * (this.moveSpeedStat / 25.0f));
                initialSpeedCoefficient = 4.5f;
                this.damageType  = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Utility);
            }

            //This governs the info on the attack
            attack = new OverlapAttack();
            attack.damageType = this.damageType;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = damage;
            attack.procCoefficient = this.procCoefficient;
            attack.forceVector = this.bonusForce;
            attack.pushAwayForce = this.pushForce;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = this.impactSound;

            //Calculate speed of kick, as well as rotation.
            this.RecalculateRollSpeed();
            //this.RecalculateModelRot();

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
                base.characterBody.AddTimedBuffAuthority(Modules.Buffs.armorBuff.buffIndex, this.duration);
                //base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f * this.duration);
            }

            
        }

        private void RecalculateRollSpeed()
        {
            this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(isSecondary ? WizardFoot.initialSpeedCoefficient : WizardFoot.initialSpeedCoefficient * 1.4f , WizardFoot.finalSpeedCoefficient, base.fixedAge / this.duration);
        }

        private void RecalculateModelRot() {
            modelTrans.rotation = Quaternion.Slerp(Quaternion.identity, _lookRot, base.fixedAge / (this.duration / 2));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateRollSpeed();
            //Update hitpause timer

            //Change forward to aimDirection and update FOV
            if (base.characterDirection) base.characterDirection.forward = this.aimRayDir;
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(WizardFoot.dodgeFOV, 60f, base.fixedAge / this.duration);

            //Update velocity vector
            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.rollSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, this.aimRayDir), 0f);
                vector = this.aimRayDir * d;

                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;

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
                this.hitPauseTimer -= Time.fixedDeltaTime;
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("kick.playbackRate", 0f);
            }

            //Trigger attack if within the active hitbox timing.
            if (this.stopwatch >= (this.attackStartTime) && this.stopwatch <= (this.attackEndTime))
            {
                FireAttack();
            }

            //Check if the kick should end , and set next state to main
            if (this.duration - base.fixedAge < this.duration-this.attackEndTime)
            { 
                this.animator.SetBool("isKicking", false);
            }
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.animator.SetBool("isKicking", false);
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void FireAttack(){
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlayAttackSpeedSound("wizardsFoot", base.gameObject, 1.0f);

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
            if (isSecondary || !isBoosted)
            {
                triforceBuffCon.AddToBuffCount(2);
            }
            if (isBoosted) {
                triforceBuffCon.RemoveAmountOfBuff(Modules.StaticValues.utilityStackConsumption);
            }

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
            //Disable particle effects
            //ganonController.FootRFire.Stop();
            ganonController.BodyLightning.Stop();
            ganonController.KneeRSpeedLines.Stop();

            //Clean up by turning off bodyFlags for falldamage and rotate to correct position.
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            modelTrans.rotation = Quaternion.identity;
            this.animator.SetBool("isKicking", false);

            this.animator.SetBool("attacking", false);
            base.characterMotor.disableAirControlUntilCollision = false;

            base.OnExit();
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}