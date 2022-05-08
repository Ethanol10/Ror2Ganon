using EntityStates;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Survivors;
using GanondorfMod.SkillStates.BaseStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates
{
    public class Punch : BaseMeleeAttack
    {
        protected bool punchActive;
        protected bool dashActive;
        protected bool kickActive;
        protected bool downAirActive;
        protected bool isAttacking;
        // LIGHT ATTACK IN THE AIR WHEN DOUBLE KICKING.
        // Second part of kicking will use the default melee hit
        protected string lightKickSoundString = "lightHitsVoice";
        protected string heavyKickSoundString = "heavyHitsVoice";
        protected string lightKickHitSoundString = "lightHitSFX";
        protected string heavyKickHitSoundString = "hardHitsSFX";
        //protected GameObject swingEffectPrefab;
        //protected GameObject hitEffectPrefab;
        private OverlapAttack lightKickAttack;
        private float lightKickAttackStartTime;
        private float lightKickDuration;
        private bool lightKickFired;
        private float lightKickEarlyExitTime;
        private float lightKickHitStopDuration;
        private float lightKickAttackRecoil;
        private float lightKickHitHopVelocity;
        private float lightKickAttackEndTime;
        private GanondorfController ganonController;

        // DASH ATTACK
        // should stop the player from dashing right after move finishes.
        // Though I think all primary's are not set to agile so maybe we don't need to worry about it.
        protected string dashSoundString = "lightHitSFX";
        protected string dashHitSoundString = "hardHitsSFX";
        protected string voiceString = "lightHitsVoice";
        //protected GameObject swingEffectPrefab;
        //protected GameObject hitEffectPrefab;
        private bool wasSprinting;
        protected float dashDuration = 0.25f;
        protected float initialSpeedCoefficient = 6f;
        protected float finalSpeedCoefficient = 1f;
        private float dashSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        public float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        //If character is grounded, just use default and call basemeleeattack.
        public override void OnEnter()
        {
            //Set up animators and intialise bools
            base.OnEnter();
            this.inHitPause = false;
            this.swingSoundString = "swingSFX";
            this.hasFired = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            this.animator.SetBool("attacking", true);
            this.animator.SetFloat("punch.playbackRate", attackSpeedStat);
            punchActive = false;
            kickActive = false;
            dashActive = false;
            isAttacking = true;
            downAirActive = false;
            lightKickFired = false;
            ganonController = base.GetComponent<GanondorfController>();
            ganonController.SwapToFist();

            wasSprinting = base.characterBody.isSprinting;
            base.characterBody.isSprinting = false;
            if (isGrounded && wasSprinting)
            {
                //prepare dash attack here
                setupDashAttack();
                dashActive = true;

                if (base.isAuthority && base.inputBank && base.characterDirection)
                {
                    this.forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
                }
                RecalculateDashSpeed();
                if (base.characterMotor && base.characterDirection)
                {
                    base.characterMotor.velocity.y = 0f;
                    base.characterMotor.velocity = this.forwardDirection * this.dashSpeed;
                }
                Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
                this.previousPosition = base.transform.position - b;
                if (NetworkClient.active)
                {
                    base.characterBody.AddTimedBuffAuthority(Modules.Buffs.armorBuff.buffIndex, dashDuration * 4.0f);
                    //duration is so short in multiplayer that I had to crank this up, it basically never gets applied at .25f
                }

                //Play Particle effect
                ganonController.ShoulderRLightning.Play();
            }
            else if (!isGrounded && CheckLookingDown()) 
            {
                setupDownAirAttack();
                downAirActive = true;

                ganonController.HandRLightning.Play();
            }
            else if (!isGrounded)
            {
                //prepare aerial attack here
                setupLightKickHitbox();
                kickActive = true;
            }
            else
            {
                //Prepare punch.
                setupPunchHitbox();
                punchActive = true;

                //Play particle effect
                ganonController.HandRLightning.Play();
            }
        }

        private void RecalculateDashSpeed()
        {
            this.dashSpeed = this.moveSpeedStat * Mathf.Lerp(this.initialSpeedCoefficient, this.finalSpeedCoefficient, stopwatch / this.dashDuration);
        }

        protected override void PlayAttackAnimation()
        {
            if (base.isGrounded && wasSprinting)
            {
                base.PlayAnimation("FullBody, Override", "DashAttack", "punch.playbackRate", this.duration);
            }
            else if (downAirActive)
            {
                base.PlayAnimation("FullBody, Override", "DownAir", "punch.playbackRate", this.duration);
            }
            else if (!base.isGrounded)
            {
                base.PlayAnimation("FullBody, Override", "AerialAttack", "punch.playbackRate", this.duration);
            }
            else
            {
                base.PlayAnimation("Gesture, Override", "Punch", "punch.playbackRate", this.duration);
            }
        }

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        private bool CheckLookingDown() 
        {
            if (Vector3.Dot(base.GetAimRay().direction, Vector3.down) > 0.866f) 
            {
                return true;
            }
            return false;
        }

        protected override void OnHitEnemyAuthority()
        {
            if (punchActive) {
                Util.PlaySound(this.hitSoundString, base.gameObject);
            }
            if (dashActive)
            {
                Util.PlaySound(this.hitSoundString, base.gameObject);
            }
            if (kickActive)
            {
                Util.PlaySound(this.heavyKickHitSoundString, base.gameObject);
            }
            if (lightKickFired) {
                base.SmallHop(base.characterMotor, this.lightKickHitHopVelocity);
            }
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

            //Increment Buff count
            GetComponent<TriforceBuffComponent>().AddToBuffCount(1);
        }

        //check based on what attack has been selected at the beginning!
        public override void FixedUpdate()
        {
            //Dash forward only if dash is active.
            if (dashActive && stopwatch <= dashDuration)
            {
                this.RecalculateDashSpeed();

                if (base.characterDirection) base.characterDirection.forward = this.forwardDirection;
                if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, stopwatch / dashDuration);

                Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
                if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * dashSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, this.forwardDirection), 0f);
                    vector = this.forwardDirection * d;
                    vector.y = 0f;

                    base.characterMotor.velocity = vector;
                }
                this.previousPosition = base.transform.position;
            }

            //Play attack animation Once
            if (isAttacking) {
                this.PlayAttackAnimation();
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
                if (this.animator) this.animator.SetFloat("punch.playbackRate", 0f);
            }

            //check if lightKick should fire.
            if (kickActive && this.stopwatch >= (this.lightKickAttackStartTime / this.attackSpeedStat)
                && this.stopwatch <= (this.lightKickAttackEndTime / this.attackSpeedStat) )
            {
                this.FireLightKickAttack();
            }

            //Check if Dash or punch should trigger... or dair or heavy kick...
            if (this.stopwatch >= (this.attackStartTime / this.attackSpeedStat) && this.stopwatch <= (this.attackEndTime / this.attackSpeedStat))
            {
                if (kickActive) {
                    //Play Particle effect
                    ganonController.FootLFire.Play();
                }
                this.FireAttack();    
            }

            //end the move early.
            if (this.stopwatch >= (this.earlyExitTime) && base.isAuthority)
            {
                if (base.inputBank.skill1.down)
                {
                    if (!this.hasFired) this.FireAttack();
                    this.SetNextState();
                    return;
                }
            }

            //End move if timer exceeds duration.
            if (this.stopwatch >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        protected override void SetNextState()
        {
            this.outer.SetNextState(new Punch());
        }


        public void FireLightKickAttack() {
            if (!lightKickFired)
            {
                lightKickFired = true;
                Util.PlayAttackSpeedSound(this.swingSoundString, base.gameObject, this.attackSpeedStat);
                Util.PlaySound(this.lightKickSoundString, base.gameObject);
                if (base.isAuthority)
                {
                    this.PlaySwingEffect();
                    base.AddRecoil(-1f * this.attackRecoil, -2f * this.attackRecoil, -0.5f * this.attackRecoil, 0.5f * this.attackRecoil);
                }
            }

            if (base.isAuthority)
            {
                if (lightKickAttack.Fire())
                {
                    this.OnHitEnemyAuthority();
                }
            }
        }

        public override void FireAttack()
        {
            if (!this.hasFired)
            {
                
                this.hasFired = true;
                Util.PlayAttackSpeedSound(this.swingSoundString, base.gameObject, this.attackSpeedStat);
                if (kickActive)
                {
                    Util.PlaySound(this.heavyKickSoundString, base.gameObject);
                }
                else {
                    Util.PlaySound(this.voiceString, base.gameObject);
                    Util.PlaySound(this.dashSoundString, base.gameObject);
                }

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

        public override void OnExit()
        {
            //disable all particle effects
            ganonController.FootLFire.Stop();
            ganonController.ShoulderRLightning.Stop();
            ganonController.HandRLightning.Stop();
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.OnExit();
        }

        public void setupDashAttack()
        {
            DamageType dmgType = DamageType.Stun1s;
            float dmgCoeff = Modules.StaticValues.dashDamageCoefficient;
            float procCoeff = 1f;
            float pushFrce = 500f;
            Vector3 bonusFrce = Vector3.zero;
            float baseDur = 1.25f;
            float atkStartTime = 0.2f;
            float atkEndTime = 0.55f;
            float bseEarlyExitTime = 0.9f;
            float hitStopDur = 0.2f;
            float atkRecoil = 0.75f;
            float hitHopVelo = 2.5f;
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = System.Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(),
                    (HitBoxGroup element) => element.groupName == "dash");
            }

            this.attack = new OverlapAttack();
            this.attack.damageType = dmgType;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = dmgCoeff * this.damageStat;
            this.attack.procCoefficient = procCoeff;
            this.attack.hitEffectPrefab = Modules.Assets.meleeHitImpactLightning;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = pushFrce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;

            this.attackStartTime = atkStartTime;
            this.attackEndTime = atkEndTime;
            this.hitStopDuration = hitStopDur;
            this.attackRecoil = atkRecoil;
            this.hitHopVelocity = hitHopVelo;
            this.duration = baseDur / this.attackSpeedStat;
            this.earlyExitTime = bseEarlyExitTime / this.attackSpeedStat;
        }

        public void setupLightKickHitbox() {
            DamageType dmgType = DamageType.Generic;
            float procCoeff = 1f;
            float pushFrce = 0f;
            Vector3 bonusFrce = Vector3.zero;
            float baseDur = 1.229f;
            float atkStartTime = 0.1f;
            float atkEndTime = 0.33f;
            float hitStopDur = 0.012f;
            float atkRecoil = 0.75f;
            float hitHopVelo = 1.5f;
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = System.Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), 
                    (HitBoxGroup element) => element.groupName == "lightkick");
            }

            lightKickAttack = new OverlapAttack();
            lightKickAttack.damageType = dmgType;
            lightKickAttack.attacker = base.gameObject;
            lightKickAttack.inflictor = base.gameObject;
            lightKickAttack.teamIndex = base.GetTeam();
            lightKickAttack.damage = Modules.StaticValues.lightKickDamageCoefficient * damageStat;
            lightKickAttack.procCoefficient = procCoeff;
            lightKickAttack.hitEffectPrefab = this.hitEffectPrefab;
            lightKickAttack.forceVector = this.bonusForce;
            lightKickAttack.pushAwayForce = pushFrce;
            lightKickAttack.hitBoxGroup = hitBoxGroup;
            lightKickAttack.isCrit = base.RollCrit();
            lightKickAttack.impactSound = this.impactSound;

            this.lightKickAttackStartTime = atkStartTime;
            this.lightKickAttackEndTime = atkEndTime;
            this.lightKickHitStopDuration = hitStopDur;
            this.lightKickAttackRecoil = atkRecoil;
            this.lightKickHitHopVelocity = hitHopVelo;
            this.lightKickDuration = baseDur / this.attackSpeedStat;

            HitBoxGroup group2;

            if (modelTransform)
            {
                group2 = System.Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(),
                    (HitBoxGroup element) => element.groupName == "melee");
            }
            this.damageType = DamageType.Generic;
            this.procCoefficient = 1f;
            this.pushForce = 600f;
            this.bonusForce = Vector3.zero;
            this.baseDuration = 1.229f;
            this.attackStartTime = 0.60f;
            this.attackEndTime = 0.75f;
            this.baseEarlyExitTime = 0.9f;
            this.hitStopDuration = 0.1f;
            this.attackRecoil = 0.5f;
            this.hitHopVelocity = 10f / this.attackSpeedStat;

            this.swingSoundString = "jumpSFX";
            this.hitSoundString = "lightHitSFX";
            //Replace with particle effects later.
            //this.swingEffectPrefab = Modules.Assets.swordSwingEffect;
            this.hitEffectPrefab = Modules.Assets.meleeHitImpact;
            this.impactSound = Modules.Assets.punchSFX.index;

            this.attack = new OverlapAttack();
            this.attack.damageType = dmgType;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = Modules.StaticValues.heavyKickDamageCoefficient * this.damageStat;
            this.attack.procCoefficient = procCoeff;
            this.attack.hitEffectPrefab = this.hitEffectPrefab;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = pushFrce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;

            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
        }

        public void setupPunchHitbox() {
            this.hitboxName = "melee";
            this.damageType = DamageType.Generic;
            this.damageCoefficient = Modules.StaticValues.punchDamageCoefficient;
            this.procCoefficient = 1f;
            this.pushForce = 500f;
            this.bonusForce = forwardDirection;
            this.baseDuration = 0.81f;
            this.attackStartTime = 0.25f;
            this.attackEndTime = 0.45f;
            this.baseEarlyExitTime = 0.5f;
            this.hitStopDuration = 0.1f;
            this.attackRecoil = 0.5f;
            this.hitHopVelocity = 6f;

            this.swingSoundString = "swingSFX";
            this.hitSoundString = "tauntPunchThunder2";
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
            this.attack.damage = this.damageCoefficient * this.damageStat;
            this.attack.procCoefficient = this.procCoefficient;
            this.attack.hitEffectPrefab = Modules.Assets.meleeHitImpactLightning;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = this.pushForce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;

            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
        }

        public void setupDownAirAttack()
        {
            this.hitboxName = "downair";
            this.damageType = DamageType.Stun1s;
            this.damageCoefficient = Modules.StaticValues.downAirDamageCoefficient;
            this.procCoefficient = 1f;
            this.pushForce = 1500f;
            this.bonusForce = Vector3.down;
            this.baseDuration = 1.2f;
            this.attackStartTime = 0.3f;
            this.attackEndTime = 0.5f;
            this.baseEarlyExitTime = 1.0f;
            this.hitStopDuration = 0.2f;
            this.attackRecoil = 0.5f;
            this.hitHopVelocity = 20f;

            this.swingSoundString = "swingSFX";
            this.hitSoundString = "tauntPunchThunder2";
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
            this.attack.damage = this.damageCoefficient * this.damageStat;
            this.attack.procCoefficient = this.procCoefficient;
            this.attack.hitEffectPrefab = Modules.Assets.meleeHitImpactLightning;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = this.pushForce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;

            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}