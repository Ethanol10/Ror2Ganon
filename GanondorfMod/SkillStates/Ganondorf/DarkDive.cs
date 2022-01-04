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
            FASTFALL = 5,
            END = 6
        }

        private float stopwatch;
        private Animator anim;
        private DarkDiveState state;
        private bool fastFalltrigger;
        private float waitVal = 0.375f;
        private float jumpDuration = 1.0f;
        private float forcebackDuration = 1.0f;
        private float grabDuration = 0.3f;
        private Vector3 jumpVector;
        public static float initialSpeedCoefficient = 4.0f;
        public static float finalSpeedCoefficient = 0.2f;
        private List<GrabController> grabController;
        private GanondorfController ganonController;
        private float blastInterval;
        private float damperVal = 0.7f; //This value should be lower than 1.0f, otherwise it'll do the opposite and make him fly up further.
        private int noOfBlasts;
        private int blastsDone;
        private bool isExploded;
        private bool isBoosted;
        private bool isSecondary;
        private float damage;
        private float miniDamage;
        private TriforceBuffComponent triforceComponent;
        private bool grabFailed;
        private float buttonHeldDownTimer;

        private Vector3 aimRayDir;
        private float grabSpeed;
        private float dropForce = 50f; // how fast ganon should fall.

        private float aerialAttackStart = 0.4f;
        private float aerialAttackEnd = 0.5f;
        private float aerialLetGo = 0.5f;
        private float groundedAttackStart = 0.4f;
        private float groundedAttackEnd = 0.5f;
        private float groundedLetGo = 0.6f;
        private BlastAttack attack;
        private BlastAttack miniBlast;
        private float grabRadius;
        private float invincibilityWindow = 1.5f;
        private bool playedGrabSound = false;
        private bool hasFired = false;


        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;
        public static float grabEndExplosionRadius = 10f;
        public static float slamForce = 5000f;

        public override void OnEnter()
        {
            base.OnEnter();
            DarkDive.initialSpeedCoefficient = 3.2f;
            grabRadius = 8f;
            buttonHeldDownTimer = 0f;
            state = DarkDiveState.START;
            this.anim = base.GetModelAnimator();
            stopwatch = 0.0f;
            blastsDone = 0;
            grabController = new List<GrabController>();
            hasFired = false;
            isExploded = false;
            ganonController = base.GetComponent<GanondorfController>();
            triforceComponent = base.GetComponent<TriforceBuffComponent>();
            jumpVector = Vector3.up;
            anim.SetBool("enemyCaught", false);
            grabFailed = false;
            fastFalltrigger = false;

            //Calculate blast properties regarding occurence
            blastInterval = Modules.StaticValues.darkDiveBlastInterval;//attackSpeedStat;
            noOfBlasts = Modules.StaticValues.darkDiveBlastCountBase + (1 * (int)(attackSpeedStat / 2));

            base.PlayAnimation("FullBody, Override", "UpBStart", "flameChoke.playbackRate", jumpDuration);

            this.grabSpeed = this.moveSpeedStat * LerpSpeedCoefficient();

            //Play Sound
            Util.PlaySound("darkDive1", base.gameObject);
            Util.PlaySound("grabStartSFX", base.gameObject);

            if (base.inputBank.skill2.down)
            {
                isBoosted = false;
                isSecondary = true;
                damage = Modules.StaticValues.darkDiveAltDamageCoefficient * this.damageStat;
                miniDamage = Modules.StaticValues.darkDiveDamageCoefficient * this.damageStat * Modules.StaticValues.darkDiveDamageReducer;
                initialSpeedCoefficient = 3.0f;
            }
            else if (base.inputBank.skill3.down)
            {
                float boost = 1f;

                if (triforceComponent.GetBuffCount() >= Modules.StaticValues.utilityStackConsumption)
                {
                    boost = Modules.StaticValues.utilityBoostCoefficient;
                    isBoosted = true;
                    ganonController.BodyLightning.Play();
                }
                damage = Modules.StaticValues.darkDiveDamageCoefficient * this.damageStat * boost;
                miniDamage = Modules.StaticValues.darkDiveDamageCoefficient * this.damageStat * (Modules.StaticValues.darkDiveDamageReducer * 2);
                initialSpeedCoefficient = 4.0f;
            }

            //Create blast attack, 
            miniBlast = new BlastAttack();
            miniBlast.damageType = DamageType.Stun1s;
            miniBlast.attacker = base.gameObject;
            miniBlast.inflictor = base.gameObject;
            miniBlast.teamIndex = base.GetTeam();
            miniBlast.baseDamage = miniDamage;
            miniBlast.procCoefficient = 1.0f;
            miniBlast.baseForce = 500f;
            miniBlast.radius = grabEndExplosionRadius;
            miniBlast.crit = base.RollCrit();

            //Create blast attack, 
            attack = new BlastAttack();
            attack.damageType = DamageType.Stun1s;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.baseDamage = damage;
            attack.procCoefficient = 1.0f;
            attack.bonusForce = Vector3.up;
            attack.baseForce = 500f;
            attack.radius = grabEndExplosionRadius;
            attack.crit = base.RollCrit();
            
            ganonController.HandRSpeedLines.Play();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            if ((base.inputBank.skill2.down && isSecondary) || (!isSecondary && base.inputBank.skill3.down))
            {
                this.buttonHeldDownTimer += Time.fixedDeltaTime;
            }
            else {
                this.buttonHeldDownTimer = 0f;
            }

            if (buttonHeldDownTimer >= 0.5f) {
                fastFalltrigger = true;
            }

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
                if (!grabFailed) {
                    AttemptGrab(grabRadius);
                }

                if (this.stopwatch >= 0.1f) {
                    grabRadius = 10f;
                }
                if (this.stopwatch >= this.grabDuration) {
                    if (grabController.Count == 0) {
                        grabFailed = true;
                    }
                    if (grabController.Count > 0 && !grabFailed)
                    {
                        this.state = DarkDiveState.EXPLODE;
                        this.stopwatch = 0f;
                        base.PlayAnimation("FullBody, Override", "UpBGrabbed", "flameChoke.playbackRate", 0.3f);
                    }
                }
                if (this.stopwatch >= this.jumpDuration && grabFailed) {
                    if (fastFalltrigger)
                    {
                        this.state = DarkDiveState.FASTFALL;
                    }
                    else {
                        this.state = DarkDiveState.END;
                    }
                }
            }

            //Trigger blast attack "noOfTimes" spaced in "blastIntervalCooldown"
            if (state == DarkDiveState.EXPLODE)
            {
                base.characterMotor.velocity = Vector3.zero;
                isExploded = true;
                if (blastsDone < noOfBlasts) {
                    if (stopwatch > blastInterval) {
                        stopwatch = 0f;
                        miniBlast.position = base.transform.position;
                        Util.PlaySound("thunderPunch", base.gameObject);
                        int hitCount = miniBlast.Fire().hitCount;
                        if (hitCount > 0)
                        {
                            OnMiniBlastHitEnemyAuthority(hitCount);
                        }
                        blastsDone++;
                    }
                }

                if (blastsDone >= noOfBlasts) {
                    jumpVector = (Vector3.up + (base.GetAimRay().direction * -1f)).normalized;
                    anim.SetBool("enemyCaught", false);
                    this.state = DarkDiveState.FORCEBACK;
                    base.PlayAnimation("FullBody, Override", "UpBFinishGrab", "flameChoke.playbackRate", forcebackDuration);
                    stopwatch = 0f;
                    if (grabController.Count > 0)
                    {
                        foreach (GrabController gCon in grabController)
                        {
                            if (gCon)
                            {
                                gCon.Release();
                            }
                        }
                    }
                }
            }

            //Trigger one final blast and force ganon backwards.
            if (state == DarkDiveState.FORCEBACK) {
                //Blast attack
                //move backwards and upwards for a few seconds.
                base.characterMotor.disableAirControlUntilCollision = true;
                DarkDive.initialSpeedCoefficient = 2.5f;
                MoveUpwards();
                if (!hasFired) {
                    hasFired = true;
                    attack.position = base.transform.position;
                    Util.PlaySound("darkDive2", base.gameObject);
                    int hitCount = attack.Fire().hitCount;
                    if (hitCount > 0)
                    {
                        OnHitEnemyAuthority(hitCount);
                    }
                }

                if (stopwatch >= forcebackDuration && fastFalltrigger)
                {
                    state = DarkDiveState.FASTFALL;
                }
                else if (stopwatch >= forcebackDuration)
                {
                    state = DarkDiveState.END;
                }
            }

            if (state == DarkDiveState.FASTFALL) {
                base.characterMotor.velocity.y = -dropForce;
                if (isGrounded) {
                    base.characterMotor.velocity = Vector3.zero;
                    state = DarkDiveState.END;
                }
            }

            //End move, send to OnExit
            if (state == DarkDiveState.END)
            {
                base.characterMotor.disableAirControlUntilCollision = false;

                //end the move.
                if (!isExploded) {
                    attack.position = base.transform.position;
                    int hitCount = attack.Fire().hitCount;
                    if (hitCount > 0)
                    {
                        OnHitEnemyAuthority(hitCount);
                    }
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
            if (isSecondary || !isBoosted)
            {
                triforceComponent.AddToBuffCount(hitCount);
            }
            if (isBoosted)
            {
                triforceComponent.RemoveAmountOfBuff(Modules.StaticValues.utilityStackConsumption);
            }
        }

        protected void OnMiniBlastHitEnemyAuthority(int hitCount) {
            triforceComponent.AddToBuffCount(hitCount);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.cameraTargetParams) {
                base.cameraTargetParams.fovOverride = -1f;
            }
            //We can try cancel the move out before it ends and instead not revert to buffer empty.
            //base.PlayAnimation("FullBody, Override", "BufferEmpty");
            ganonController.HandRSpeedLines.Stop();
            ganonController.BodyLightning.Stop();

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
            foreach (HurtBox singularTarget in target)
            {
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
                            for (int i = 0; i < grabController.Count; i++)
                            {
                                if (grabController[i].gameObject.GetInstanceID() == singularTarget.healthComponent.body.gameObject.GetInstanceID())
                                {
                                    found = true;
                                    break; //Break out of loop.
                                }
                            }
                            if (!found)
                            {
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (state == DarkDiveState.FORCEBACK || state == DarkDiveState.END || state == DarkDiveState.JUMP || state == DarkDiveState.FASTFALL) {
                return InterruptPriority.Any;
            }

            return InterruptPriority.Frozen;
        }
    }
}
