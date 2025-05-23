﻿using EntityStates;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Survivors;
using GanondorfMod.SkillStates.BaseStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates
{
    public class WarlockPunchScepter : BaseMeleeAttack
    {
        private bool isAttacking;
        private string voiceLine = "";
        private float dmgMultiplier = 1f;
        private bool hitEnemy = false;
        private GanondorfController ganonController;
        private bool halfBoosted;
        private bool fullBoosted;

        //If character is grounded, just use default and call basemeleeattack.
        public override void OnEnter()
        {
            base.OnEnter();
            this.swingSoundString = "tauntSpin";
            this.hasFired = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            ganonController = base.GetComponent<GanondorfController>();
            //base.characterBody.outOfCombatStopwatch = 0f;
            this.animator.SetBool("attacking", true);
            isAttacking = true;

            //Boosts the damage ONLY if at 50 or more stack.
            halfBoosted = false;
            fullBoosted = false;

            int buffCount = base.characterBody.GetBuffCount(Modules.Buffs.triforceBuff);

            if (buffCount < Modules.StaticValues.maxPowerStack / 2)
            {
                dmgMultiplier = 1.0f;
            }
            else if (buffCount >= Modules.StaticValues.maxPowerStack / 2 && buffCount < Modules.StaticValues.maxPowerStack)
            {
                dmgMultiplier = Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducerScepter / 3;
                halfBoosted = true;
            }
            else if (buffCount >= Modules.StaticValues.maxPowerStack)
            {
                dmgMultiplier = Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducerScepter;
                fullBoosted = true;
            }

            setupWarlockPunchHitbox();

            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(Modules.Buffs.armorBuff, this.duration);
            }

            //Enable Particle Effects
            ganonController.HandLFire.Play();
            ganonController.PunchCharge.Play();
            ganonController.HandLSpeedLines.Play();
            ganonController.SwapToFist();
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("Sheathe, Override", "warlockpunch", "warlockPunch.playbackRate", this.duration, 0.2f);
        }

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            //Apply Hithop velocity.
            if (!this.hasHopped)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded && this.hitHopVelocity > 0f)
                {
                    base.SmallHop(base.characterMotor, this.hitHopVelocity);
                }

                this.hasHopped = true;
            }

            //Create hitstop cache.
            if (!this.inHitPause && this.hitStopDuration > 0f)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "warlockPunch.playbackRate");
                this.hitPauseTimer = this.hitStopDuration / this.attackSpeedStat;
                this.inHitPause = true;
            }

            if (!halfBoosted && !fullBoosted)
            {
                GetComponent<TriforceBuffComponent>().AddToBuffCount(Modules.StaticValues.maxPowerStack / 10);
            }
            else if (halfBoosted)
            {
                GetComponent<TriforceBuffComponent>().RemoveAmountOfBuff(Modules.StaticValues.maxPowerStack / 2);
            }
            else if (fullBoosted)
            {
                GetComponent<TriforceBuffComponent>().RemoveAmountOfBuff(Modules.StaticValues.maxPowerStack);
            }
            //base.characterBody.SetBuffCount(Modules.Buffs.absorbtionBuff.buffIndex, 0);
            //base.characterBody.OnClientBuffsChanged();
            //}
            Wave wave = new Wave
            {
                amplitude = 2f,
                frequency = 180f,
                cycleOffset = 0f
            };

            RoR2.ShakeEmitter.CreateSimpleShakeEmitter(GetAimRay().origin, wave, 0.5f, 20f, true);
        }

        //check based on what attack has been selected at the beginning!
        public override void FixedUpdate()
        {
            //Play attack animation Once
            if (isAttacking) {
                this.PlayAttackAnimation();
                Util.PlaySound(voiceLine, base.gameObject);
                isAttacking = false;
            }
            

            //Return player back to usual speed once hitpause is over
            if (this.hitPauseTimer <= 0f && this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
                base.characterMotor.velocity = this.storedVelocity;
            }
            //Increment stopwatch if we are in hitpause
            if (!this.inHitPause)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                //Increment Timers for hitstun
                this.hitPauseTimer -= Time.fixedDeltaTime;
                //Set velocity to zero, and punch playback rate to 0 to simulate a strong hit
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("warlockPunch.playbackRate", 0f);
            }

            //Check if Dash or punch should trigger
            if (this.stopwatch >= (this.attackStartTime) && this.stopwatch <= (this.attackEndTime))
            {
                this.FireAttack();    
            }

            //End move if timer exceeds duration.
            if (this.stopwatch >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        //Fire attack, Pretty standard stuff basically inheriting from base melee attack
        public override void FireAttack()
        {
            if (!this.hasFired)
            {
                
                this.hasFired = true;
                Util.PlayAttackSpeedSound(this.swingSoundString, base.gameObject, this.attackSpeedStat);

                if (base.isAuthority)
                {
                    this.PlaySwingEffect();
                    base.AddRecoil(-1f * this.attackRecoil, -2f * this.attackRecoil, -0.5f * this.attackRecoil, 0.5f * this.attackRecoil);
                }
                EffectManager.SpawnEffect(Modules.Assets.warlockPunchEffect, new EffectData
                {
                    origin = base.GetAimRay().origin + 5 * new Vector3(base.GetAimRay().direction.x, 0, base.GetAimRay().direction.z),
                    scale = 1f,
                    rotation = Quaternion.LookRotation(new Vector3(base.GetAimRay().direction.x, 0, base.GetAimRay().direction.z))
                }, false);
            }

            if (base.isAuthority)
            {
                if (this.attack.Fire())
                {
                    this.OnHitEnemyAuthority();
                }
            }
        }

        //Use base exit.
        public override void OnExit()
        {
            //stop playing particles
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            ganonController.HandLFire.Stop();
            ganonController.PunchCharge.Stop();
            ganonController.HandLSpeedLines.Stop();
            base.OnExit();
        }

        //Setup the hitbox for warlock punch.
        public void setupWarlockPunchHitbox() {
            
            int randomNum = UnityEngine.Random.Range(1, 101);
            if (randomNum < 5)
            {
                this.damageCoefficient = Modules.StaticValues.warlockPunchDamageCoefficient * Modules.StaticValues.warlockMemeDamage;
                this.voiceLine = "ganonScreamingWarlockPunch";
                this.pushForce = 100000f;
                this.swingSoundString = "memeExplosion";
            }
            else {
                this.swingSoundString = "warlockPunchSFX";
                this.hitSoundString = "hardHitsSFX";
                this.voiceLine = "warlockPunchVoice";
                this.damageCoefficient = Modules.StaticValues.warlockPunchDamageCoefficient;
                this.pushForce = 10000f;
            }
            this.hitboxName = "warlock";
            this.damageType = new DamageTypeCombo(DamageType.Generic | DamageType.AOE, DamageTypeExtended.Generic, DamageSource.Special);
            this.procCoefficient = 1.5f;
            this.bonusForce = Vector3.zero;
            this.baseDuration = 2.7f;
            this.attackStartTime = 1.4f;
            this.attackEndTime = 2.04f;
            this.baseEarlyExitTime = 2.7f;
            this.hitStopDuration = 0.5f;
            this.attackRecoil = 0.5f;
            this.hitHopVelocity = 10f;

            
            
            //Replace with particle effects later.
            //this.swingEffectPrefab = Modules.Assets.swordSwingEffect;
            //this.hitEffectPrefab = Modules.Assets.swordHitImpactEffect;
            this.impactSound = Modules.Assets.punchSFX.index;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = System.Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(),
                    (HitBoxGroup element) => element.groupName == this.hitboxName);
            }
            this.attack = new OverlapAttack();
            this.attack.damageType = this.damageType;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = this.damageCoefficient * this.damageStat * this.dmgMultiplier;
            this.attack.procCoefficient = this.procCoefficient;
            this.attack.hitEffectPrefab = this.hitEffectPrefab;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = this.pushForce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;

            this.duration = this.baseDuration; /*/ this.attackSpeedStat;*/ //doesn't scale at all.
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}