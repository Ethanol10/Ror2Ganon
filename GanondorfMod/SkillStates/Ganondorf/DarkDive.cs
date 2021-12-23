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
    public class DarkDive : BaseSkillState
    {
        private enum DarkDiveState : ushort
        {
            START = 1,
            JUMP = 2,
            EXPLODE = 3,
            FORCEBACK = 4,
            END = 5
        }

        private float stopwatch;
        private Animator anim;
        private DarkDiveState state;
        private float waitVal = 0.375f;
        private float jumpDuration = 1.58f;
        private float grabDuration = 0.5f;
        private Vector3 jumpVector = Vector3.up;
        public static float initialSpeedCoefficient = 3.2f;
        public static float finalSpeedCoefficient = 0f;
        private List<GrabController> grabController;
        private GanondorfController ganonController;
        private float blastInterval = 0.1f;
        private float damperVal = 0.7f; //This value should be lower than 1.0f, otherwise it'll do the opposite and make him fly up further.
        private int noOfBlasts = 5;
        private int blastsDone;
        private bool isExploded;


        private Vector3 aimRayDir;
        private float grabSpeed;
        private float dropForce = 50f; // how fast ganon should fall.


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


        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;
        public static float grabEndExplosionRadius = 10f;
        public static float flameChokeDamageCoefficient = Modules.StaticValues.flameChokeDamageCoefficient;
        public static float flameChokeProcCoefficient = 1f;
        public static float slamForce = 5000f;

        public override void OnEnter()
        {
            base.OnEnter();
            state = DarkDiveState.START;
            this.anim = base.GetModelAnimator();
            stopwatch = 0.0f;
            blastsDone = 0;
            grabController = new List<GrabController>();
            this.finishMove = false;
            this.doGroundedFinisher = false;
            this.doAerialFinisher = false;
            this.stateFixed = false;
            isExploded = false;
            ganonController = base.GetComponent<GanondorfController>();
            anim.SetBool("enemyCaught", false);

            base.PlayAnimation("FullBody, Override", "UpBStart", "flameChoke.playbackRate", grabDuration);

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

            //Create blast attack, 
            attack = new BlastAttack();
            attack.damageType = DamageType.Stun1s;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.baseDamage = Modules.StaticValues.flameChokeDamageCoefficient * this.damageStat;
            attack.procCoefficient = 1.0f;
            attack.baseForce = 1500f;
            attack.radius = grabEndExplosionRadius;
            attack.crit = base.RollCrit();
            
            ganonController.HandRSpeedLines.Play();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            //Stay on the ground for duration of jump squat in animation.
            //Jump squat ends frame 9 of a 24fps animation.
            if (state == DarkDiveState.START)
            {
                if (this.stopwatch >= this.waitVal) {
                    stopwatch = 0f;
                    this.state = DarkDiveState.JUMP;
                }
            }

            //Handle Jump and grab everything during jump. End after duration.
            if (state == DarkDiveState.JUMP)
            {
                MoveUpwards();
                AttemptGrab();
                if (this.stopwatch >= this.jumpDuration) {
                    if (grabController.Count > 0)
                    {
                        this.state = DarkDiveState.EXPLODE;
                        this.stopwatch = 0f;
                    }
                    else {
                        this.state = DarkDiveState.END;
                    }
                }
            }

            //Trigger blast attack "noOfTimes" spaced in "blastIntervalCooldown"
            if (state == DarkDiveState.EXPLODE)
            {
                isExploded = true;
                if (blastsDone < noOfBlasts) {
                    if (stopwatch > blastInterval) {
                        stopwatch = 0f;
                        int hitCount = attack.Fire().hitCount;
                        if (hitCount > 0) {
                            OnHitEnemyAuthority(hitCount);
                        }
                        blastsDone++;
                    }
                }

                if (blastsDone >= noOfBlasts) {
                    this.state = DarkDiveState.FORCEBACK;
                }
            }

            //Trigger one final blast and force ganon backwards.
            if (state == DarkDiveState.FORCEBACK) {
                //Blast attack
                //move backwards and upwards for a few seconds.
            }

            //End move, send to OnExit
            if (state == DarkDiveState.END)
            {
                //end the move.
                if (!isExploded) {

                }
                this.outer.SetNextStateToMain();
            }
        }

        private void MoveUpwards() {
            base.characterMotor.rootMotion += this.jumpVector * (LerpSpeedCoefficient() * damperVal);
            base.characterMotor.velocity.y = 0f;
        }

        //Lerp code
        private float LerpTime()
        {
            float timeFraction = this.stopwatch / this.jumpDuration;
            return timeFraction >= 1 ? 1 : 1 - (float)Math.Pow(2, -10 * timeFraction);
        }

        private float LerpSpeedCoefficient()
        {
            return Mathf.Lerp(DarkDive.initialSpeedCoefficient, DarkDive.finalSpeedCoefficient, LerpTime());
        }

        protected void OnHitEnemyAuthority(int hitCount)
        {

        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.cameraTargetParams) {
                base.cameraTargetParams.fovOverride = -1f;
            }
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            ganonController.HandRSpeedLines.Stop();

            if (grabController.Count > 0) {
                foreach (GrabController gCon in grabController)
                {
                    if (gCon)
                    {
                        gCon.Release();
                    }
                }
            }
        }

        //Attempt grab, don't stop for any enemy.
        //Uses bullseye search, and puts every enemy into the grab controller.
        //grab radius of 10f is a little too big, try not to go over.
        public void AttemptGrab()
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
                            GrabController grabbedEnemy = singularTarget.healthComponent.body.gameObject.AddComponent<GrabController>();
                            grabbedEnemy.pivotTransform = this.FindModelChild("HandR");
                            grabController.Add(grabbedEnemy);
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
            this.AttemptGrab();

            if (isGrounded) {

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
