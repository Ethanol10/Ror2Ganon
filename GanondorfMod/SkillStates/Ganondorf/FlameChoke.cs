using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using GanondorfMod.SkillStates;
using GanondorfMod.Modules.Survivors;
using GanondorfMod.Modules;

namespace GanondorfMod.SkillStates
{
    public class FlameChoke : BaseSkillState
    {
        private float grabDuration = 0.4f;
        private float windupDuration = 0.3f;
        public static float initialSpeedCoefficient = 3.2f;
        public static float finalSpeedCoefficient = 0f;
        private Vector3 aimRayDir;
        private float grabSpeed;
        private float dropForce = 50f; // how fast ganon should fall.
        private List<GrabController> grabController;
        private float stopwatch;
        private Animator anim;
        private Vector3 previousPosition;
        private bool finishMove;
        private bool doGroundedFinisher;
        private bool doAerialFinisher;
        private bool stateFixed;
        private float aerialAttackStart = 0.4f;
        private float aerialAttackEnd = 0.5f;
        private float aerialLetGo = 0.5f;
        private float groundedAttackStart = 0.4f;
        private float groundedAttackEnd = 0.5f;
        private float groundedLetGo = 0.6f;
        private BlastAttack attack;
        private float grabRadius = 8f;
        private float invincibilityWindow = 1.5f;
        private bool playedGrabSound = false;
        private bool hasFired = false;
        private GanondorfController ganonController;
        private TriforceBuffComponent triforceBuffComponent;

        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;
        public static float grabEndExplosionRadius = 10f;
        public static float flameChokeDamageCoefficient = Modules.StaticValues.flameChokeDamageCoefficient;
        public static float flameChokeProcCoefficient = 1f;
        public static float slamForce = 5000f;

        private bool isSecondary;
        private bool isBoosted;

        public override void OnEnter()
        {
            base.OnEnter();
            this.anim = base.GetModelAnimator();
            this.aimRayDir = base.GetAimRay().direction;
            stopwatch = 0.0f;
            grabController = new List<GrabController>();
            this.finishMove = false;
            this.doGroundedFinisher = false;
            this.doAerialFinisher = false;
            this.stateFixed = false;
            this.isSecondary = false;
            this.isBoosted = false;
            ganonController = base.GetComponent<GanondorfController>();
            triforceBuffComponent = base.GetComponent<TriforceBuffComponent>();
            anim.SetFloat("flameChoke.playbackrate", 2.5f);
            anim.SetBool("enemyCaught", false);
            anim.SetBool("continueGrabbing", true);
            anim.SetBool("performAerialSlam", false);

            base.PlayAnimation("FullBody, Override", "GrabStart", "flameChoke.playbackRate", grabDuration);

            this.grabSpeed = this.moveSpeedStat * LerpSpeedCoefficient();

            if (base.characterMotor && base.characterDirection)
            {
                //base.characterMotor.velocity.y = 0f;
                base.characterMotor.velocity = this.aimRayDir * this.grabSpeed;
            }
            //Play Sound
            Util.PlaySound("grabStartSFX", base.gameObject);

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;

            //Move changes based on what slot it was selected in.
            float damage = 0;
            if (base.inputBank.skill2.down)
            {
                isBoosted = false;
                isSecondary = true;
                damage = Modules.StaticValues.flameChokeAltDamageCoefficient * this.damageStat + (this.damageStat * this.attackSpeedStat * Modules.StaticValues.flameChokeDamageReducer);
            }
            else if (base.inputBank.skill3.down)
            {
                float boost = 1f;
                
                if (triforceBuffComponent.GetBuffCount() >= Modules.StaticValues.utilityStackConsumption) {
                    boost = Modules.StaticValues.utilityBoostCoefficient;
                    isBoosted = true;
                    ganonController.BodyLightning.Play();
                }
                damage = Modules.StaticValues.flameChokeDamageCoefficient * this.damageStat * boost + (this.damageStat * this.attackSpeedStat * Modules.StaticValues.flameChokeDamageReducer * 1.5f);
            }

            //Create blast attack, 
            attack = new BlastAttack();
            attack.damageType = DamageType.Stun1s;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.baseDamage = damage;
            attack.procCoefficient = 1.0f;
            attack.baseForce = 1500f;
            attack.radius = grabEndExplosionRadius;
            attack.crit = base.RollCrit();

            //Play Particle effects
            //ganonController.HandLFire.Play();
            ganonController.HandLSpeedLines.Play();
        }

        public override void FixedUpdate()
        {
            //We need to keep track of state during the move
            base.FixedUpdate();

            if (!this.doGroundedFinisher || !this.doAerialFinisher) {
                stopwatch += Time.fixedDeltaTime;
            }

            //Check if the windup is complete, it should only play for a set amount of time.
            //Give speed towards aimray direction for the duration of the grab.
            if ( (stopwatch > windupDuration && stopwatch < grabDuration + windupDuration)) {
                SpeedBoostOnGrabDuration();
                AttemptGrab(grabRadius);
            }

            //Once grab duration is over, check if a finisher is required
            if (stopwatch > windupDuration + grabDuration && !stateFixed)
            {
                //Prepare to finish move by running the function to handle attack based on being grounded.
                if (grabController.Count > 0)
                {
                    if (isGrounded)
                    {
                        //Anim isGrounded is already set by the game
                        
                        this.doGroundedFinisher = true;
                        anim.SetBool("performAerialSlam", false);
                    }
                    else
                    {
                        //Not grounded, do the aerial grab.
                        base.characterMotor.velocity = Vector3.zero;
                        anim.SetBool("performAerialSlam", true);
                        this.doAerialFinisher = true;
                    }
                    anim.SetBool("enemyCaught", true);
                    anim.SetBool("continueGrabbing", false);
                }
                else {
                    finishMove = true;
                    anim.SetBool("continueGrabbing", false);
                }
                // Fix the state so that no other path can be selected.
                this.stateFixed = true;
                stopwatch = 0.0f;
            }

            //Perform grounded finisher.
            if (this.doGroundedFinisher) {
                GroundedFinisher();
            }

            //Perform aerial slam.
            if (this.doAerialFinisher) {
                StartDrop();
            }

            //Finish the move after either function is finished.
            if (finishMove && base.isAuthority) {

                SpeedBoostOnGrabDuration();
                anim.SetBool("continueGrabbing", false);
                this.outer.SetNextStateToMain();
            }
        }

        //Roll code
        private void SpeedBoostOnGrabDuration() {
            this.grabSpeed = this.moveSpeedStat * LerpSpeedCoefficient();
            if (base.characterDirection)
            {
                base.characterDirection.forward = this.aimRayDir;
            }
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride
                   = Mathf.Lerp(WizardFoot.dodgeFOV, 60f, stopwatch - windupDuration / this.grabDuration);
            }

            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.grabSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, this.aimRayDir), 0f);
                vector = this.aimRayDir * d;

                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
        }

        //Lerp code, modified to keep max speed until the last 1/10th of the move.
        private float LerpSpeedCoefficient()
        {
            float lerpVal;
            if (stopwatch - windupDuration / this.grabDuration < 0.9)
            {
                lerpVal = 0.0f;
            }
            else {
                lerpVal = stopwatch - windupDuration / this.grabDuration;
            }
            return Mathf.Lerp(isSecondary ? FlameChoke.initialSpeedCoefficient * 0.7f : FlameChoke.initialSpeedCoefficient , FlameChoke.finalSpeedCoefficient, lerpVal);
        }

        protected virtual void OnHitEnemyAuthority(int hitCount)
        {
            Util.PlaySound("flameChokeSFXend", base.gameObject);
            if (isSecondary || !isBoosted) {
                triforceBuffComponent.AddToBuffCount(hitCount);
            }
            if (isBoosted) {
                triforceBuffComponent.RemoveAmountOfBuff(Modules.StaticValues.utilityStackConsumption);
            }
        }

        public override void OnExit()
        {
            //Stop all particle effects
            ganonController.BodyLightning.Stop();
            //ganonController.HandLFire.Stop();
            ganonController.HandLSpeedLines.Stop();
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (base.cameraTargetParams) {
                base.cameraTargetParams.fovOverride = -1f;
            }

            ganonController.SetMaxVal(grabController.Count);

            //Release all controllers.
            if (grabController.Count > 0) {
                foreach (GrabController gCon in grabController)
                {
                    if (gCon)
                    {
                        gCon.Release();
                    }
                }
            }

            base.OnExit();
        }

        //Attempt grab, don't stop for any enemy.
        //Uses bullseye search, and puts every enemy into the grab controller.
        //grab radius of 10f is a little too big, try not to go over.
        public void AttemptGrab(float grabRadius)
        {
            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = false,
                searchOrigin = base.transform.position,
                searchDirection = UnityEngine.Random.onUnitSphere,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = grabRadius,
                maxAngleFilter = 360f
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            List<HurtBox> target = search.GetResults().ToList<HurtBox>();
            foreach (HurtBox singularTarget in target) {
                if (singularTarget)
                {
                    if (singularTarget.healthComponent && singularTarget.healthComponent.body)
                    {
                        if (BodyMeetsGrabConditions(singularTarget.healthComponent.body))
                        {
                            if (!playedGrabSound)
                            {
                                Util.PlaySound("GrabSounds", base.gameObject);
                                playedGrabSound = true;
                                if (NetworkServer.active)
                                {
                                    base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, this.invincibilityWindow);
                                }
                            }
                            bool found = false;
                            for ( int i = 0; i < grabController.Count; i++) {
                                if (grabController[i].gameObject.GetInstanceID() == singularTarget.healthComponent.body.gameObject.GetInstanceID())
                                {
                                    found = true;
                                    break; //Break out of loop.
                                }
                            }
                            if (!found) {
                                GrabController grabbedEnemy = singularTarget.healthComponent.body.gameObject.AddComponent<GrabController>();
                                grabbedEnemy.pivotTransform = this.FindModelChild("HandL");
                                grabController.Add(grabbedEnemy);
                            }
                        }
                    }
                }
            }
        }

        private bool BodyMeetsGrabConditions(CharacterBody targetBody)
        {
            bool meetsConditions = true;
            return meetsConditions;
        }

        public void StartDrop() {
            base.characterMotor.disableAirControlUntilCollision = true;
            base.characterMotor.velocity.y = -dropForce;
            this.AttemptGrab(grabRadius);

            if (isGrounded) {
                ganonController.HandLSpeedLines.Stop();
                base.characterMotor.velocity = Vector3.zero;
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > aerialAttackStart && stopwatch < aerialAttackEnd) {
                    if (!hasFired && base.isAuthority)
                    {
                        hasFired = true;
                        attack.position = this.FindModelChild("HandL").position;
                        int hitCount = attack.Fire().hitCount;
                        if (hitCount > 0) {
                            OnHitEnemyAuthority(hitCount);
                        }
                    }
                    //play sounds after firing
                    Util.PlaySound("flameChokeSFXEnd", base.gameObject);
                }
                if (stopwatch > aerialLetGo) {
                    if (!hasFired && base.isAuthority) {
                        attack.position = this.FindModelChild("HandL").position;
                        int hitCount = attack.Fire().hitCount;
                        if (hitCount > 0)
                        {
                            OnHitEnemyAuthority(hitCount);
                        }
                    }
                    finishMove = true;
                }
            }
        }

        public void GroundedFinisher() {
            if (stopwatch > groundedAttackStart && stopwatch < groundedAttackEnd) {
                base.characterMotor.velocity = Vector3.zero;
                if (!hasFired && base.isAuthority)
                {
                    hasFired = true;
                    attack.position = this.FindModelChild("HandL").position;
                    int hitCount = attack.Fire().hitCount;
                    if (hitCount > 0)
                    {
                        OnHitEnemyAuthority(hitCount);
                    }
                }

                //Play sounds after firing
                Util.PlaySound("flameChokeSFXEnd", base.gameObject);
            }
            if (stopwatch >= groundedLetGo) {
                if (!hasFired && base.isAuthority) {
                    attack.position = this.FindModelChild("HandL").position;
                    int hitCount = attack.Fire().hitCount;
                    if (hitCount > 0)
                    {
                        OnHitEnemyAuthority(hitCount);
                    }
                }
                finishMove = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
