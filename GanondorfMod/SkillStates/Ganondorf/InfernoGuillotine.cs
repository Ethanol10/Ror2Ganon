using EntityStates;
using GanondorfMod.SkillStates.BaseStates;
using GanondorfMod.Modules.Survivors;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using GanondorfMod.Modules;
using System.Collections.Generic;
using System.Linq;

namespace GanondorfMod.SkillStates
{
    public class InfernoGuillotine : BaseMeleeAttack
    {
        enum KickState : ushort
        {
            CHARGE = 0,
            DROP = 1,
            EXPLODE = 2
        }

        private bool checkedWeight;
        private float maxWeight;
        private bool isAttacking;
        private string voiceLine = "";
        private float dmgMultiplier = 1f;
        private bool hitEnemy = false;
        private GanondorfController ganonController;
        private BlastAttack blastAttack;
        private float windupTimer = 1.2f;
        private float stopwatch = 0f;
        private float dropForce = 100f;
        private KickState state;
        private float initialSmallhop = 20.0f;
        private float pullRadius = 30f;
        private bool isExplosion;
        private float explosionRadius = 20f;
        private bool alreadyPulled;
        private float pullMultiplier = 1.5f;
        private Transform slamIndicatorInstance;
        private Transform slamCenterIndicatorInstance;
        private Ray downRay;

        public override void OnEnter()
        {
            base.OnEnter();
            maxWeight = Modules.StaticValues.infernoGuillotinePullForce;
            this.swingSoundString = "tauntSpin";
            this.hasFired = false;
            this.isExplosion = true;
            this.alreadyPulled = false;
            this.checkedWeight = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            ganonController = base.GetComponent<GanondorfController>();
            //base.characterBody.outOfCombatStopwatch = 0f;
            this.animator.SetBool("attacking", true);
            isAttacking = true;
            dmgMultiplier = base.characterBody.GetBuffCount(Modules.Buffs.triforceBuff) / Modules.StaticValues.warlockPunchDamageReducer;
            if (dmgMultiplier < 1.0f) {
                dmgMultiplier = 1.0f;
            }
            setupInfernoGuillotineAttack();

            //Reset
            state = KickState.CHARGE;
            if (!isGrounded) {
                base.characterMotor.velocity = Vector3.zero;
                base.SmallHop(base.characterMotor, this.initialSmallhop);
            }
            
            //Enable Particle Effects
            ganonController.FootRInfernoFire.Play();
            ganonController.PullShockwave.Play();

            Util.PlaySound(this.voiceLine, base.gameObject);

            CreateIndicator();

            //Play attack anim
            PlayAttackAnimation();
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayAnimation("FullBody, Override", "InfernoKickStart", "warlockPunch.playbackRate", this.duration);
        }

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            //Create hitstop cache.
            if (!this.inHitPause && this.hitStopDuration > 0f)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "warlockPunch.playbackRate");
                this.hitPauseTimer = this.hitStopDuration / this.attackSpeedStat;
                this.inHitPause = true;
            }

            //Wipe buff if hit.
            if(this.isExplosion){
                GetComponent<TriforceBuffComponent>().WipeBuffCount();
            }
        }

        //check based on what attack has been selected at the beginning!
        public override void FixedUpdate()
        {
            //We are going to split the move up into 3 sections, Charge, Drop, Explode.

            //Charge, this has some SUCC code.
            if (state == KickState.CHARGE) {
                //increment the timer.
                stopwatch += Time.fixedDeltaTime;
                SearchToPull();
                UpdateSlamIndicator();

                if (stopwatch > windupTimer) {
                    state = KickState.DROP;
                    ganonController.PullShockwave.Stop();
                    ganonController.InfernoKickFalling.Play();
                }
            }

            //drop
            if (state == KickState.DROP) {
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
                    //Animation automatically transitions into the next state, don't panic.
                    if (this.attack.Fire())
                    {
                        OnHitEnemyAuthority();
                        //TODO get hitstun in this damn thing
                    }
                    base.characterMotor.velocity.y = -dropForce;
                }
                else
                {
                    //Increment Timers for hitstun
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    //Set velocity to zero, and punch playback rate to 0 to simulate a strong hit
                    if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                    if (this.animator) this.animator.SetFloat("warlockPunch.playbackRate", 0f);
                }

                if (isGrounded) {
                    ganonController.InfernoKickFalling.Stop();
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    this.inHitPause = false;
                    base.characterMotor.velocity = Vector3.zero;
                    state = KickState.EXPLODE;
                }
            }
            //explode
            if (state == KickState.EXPLODE) {
                //Animation automatically transitions due to a condition that checks if we're grounded. Don't panic.
                Util.PlaySound(this.hitSoundString, base.gameObject);
                Util.PlaySound(this.swingSoundString, base.gameObject);
                if (!hasFired) {
                    hasFired = true;
                    blastAttack.position = this.transform.position;
                    if (blastAttack.Fire().hitCount > 0) {
                        this.isExplosion = true;
                        OnHitEnemyAuthority();
                    }
                }

                for (int i = 0; i <= 6; i++)
                {
                    Vector3 effectPosition = base.characterBody.footPosition + (UnityEngine.Random.insideUnitSphere * 8f);
                    effectPosition.y = base.characterBody.footPosition.y;
                    EffectManager.SpawnEffect(EntityStates.LemurianBruiserMonster.SpawnState.spawnEffectPrefab, new EffectData
                    {
                        origin = effectPosition,
                        scale = 4f
                    }, true);
                }

                this.outer.SetNextStateToMain();
            }
        }

        public void GetMaxWeight()
        {
            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = false,
                searchOrigin = base.transform.position,
                searchDirection = UnityEngine.Random.onUnitSphere,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = pullRadius,
                maxAngleFilter = 360f
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            maxWeight = Modules.StaticValues.infernoGuillotinePullForce;

            this.checkedWeight = true;

            List<HurtBox> target = search.GetResults().ToList<HurtBox>();
            foreach (HurtBox singularTarget in target)
            {
                if (singularTarget)
                {
                    if (singularTarget.healthComponent && singularTarget.healthComponent.body)
                    {
                        if (singularTarget.healthComponent.body.characterMotor)
                        {
                            if (singularTarget.healthComponent.body.characterMotor.mass > maxWeight){
                                maxWeight = singularTarget.healthComponent.body.characterMotor.mass;
                            }
                        }
                        else if (singularTarget.healthComponent.body.rigidbody) {
                            if (singularTarget.healthComponent.body.rigidbody.mass > maxWeight) {
                                maxWeight = singularTarget.healthComponent.body.rigidbody.mass;
                            }
                        }
                    }
                }
            }
        }

        public void SearchToPull() {
            if (!checkedWeight) {
                GetMaxWeight();
            }

            BlastAttack pullAttack = new BlastAttack();
            pullAttack.damageType = DamageType.Generic;
            pullAttack.position = base.transform.position;
            pullAttack.attacker = base.gameObject;
            pullAttack.inflictor = base.gameObject;
            pullAttack.teamIndex = base.teamComponent.teamIndex;
            pullAttack.baseDamage = 0f;
            pullAttack.procCoefficient = 0f;
            pullAttack.radius = explosionRadius * 2f;
            pullAttack.bonusForce = Vector3.zero;
            pullAttack.baseForce = -maxWeight;
            pullAttack.crit = false;

            pullAttack.Fire();
        }

        private void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = base.transform.position
                };

                this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamIndicatorInstance.localScale = Vector3.one * explosionRadius;

                this.slamCenterIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamCenterIndicatorInstance.localScale = (Vector3.one * explosionRadius) / 3f;
            }
        }

        private void UpdateSlamIndicator()
        {
            if (this.slamIndicatorInstance)
            {
                float maxDistance = 250f;

                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = base.transform.position
                };

                RaycastHit raycastHit;
                if (Physics.Raycast(this.downRay, out raycastHit, maxDistance, LayerIndex.world.mask))
                {
                    this.slamIndicatorInstance.transform.position = raycastHit.point;
                    this.slamIndicatorInstance.transform.up = raycastHit.normal;

                    this.slamCenterIndicatorInstance.transform.position = raycastHit.point;
                    this.slamCenterIndicatorInstance.transform.up = raycastHit.normal;
                }
            }
        }

        //Use base exit.
        public override void OnExit()
        {
            //stop playing particles
            ganonController.FootRInfernoFire.Stop();
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            if (this.slamCenterIndicatorInstance) EntityState.Destroy(this.slamCenterIndicatorInstance.gameObject);
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.OnExit();
        }

        //Setup the hitbox for warlock punch.
        public void setupInfernoGuillotineAttack() {
            //int randomNum = UnityEngine.Random.Range(1, 1001);
            //if (randomNum < 5)
            //{
            //    this.damageCoefficient = Modules.StaticValues.warlockPunchDamageCoefficient * Modules.StaticValues.warlockMemeDamage;
            //    this.voiceLine = "ganonScreamingWarlockPunch";
            //    this.pushForce = 100000f;
            //    this.swingSoundString = "memeExplosion";
            //    explosionRadius = 60f;

            this.swingSoundString = "warlockPunchSFX";
            this.hitSoundString = "superHitsVoice";
            this.voiceLine = "infernoKickStart";
            this.damageCoefficient = Modules.StaticValues.infernoGuillotineCoefficient;
            this.pushForce = maxWeight;
            this.procCoefficient = 1.5f;
            this.bonusForce = Vector3.up;
            this.baseDuration = 2.25f;
            this.attackStartTime = 1.45f;
            this.attackEndTime = 1.65f;
            this.baseEarlyExitTime = 2.20f;
            this.hitStopDuration = 0.1f;

            //Replace with particle effects later.
            //this.swingEffectPrefab = Modules.Assets.swordSwingEffect;
            //this.hitEffectPrefab = Modules.Assets.swordHitImpactEffect;
            this.impactSound = Modules.Assets.punchSFX.index;

            blastAttack = new BlastAttack();
            blastAttack.damageType = DamageType.Stun1s;
            blastAttack.attacker = base.gameObject;
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = base.teamComponent.teamIndex;
            blastAttack.baseDamage = this.damageCoefficient * this.damageStat * this.dmgMultiplier;
            blastAttack.procCoefficient = 1.0f;
            blastAttack.radius = explosionRadius;
            blastAttack.bonusForce = this.bonusForce;
            blastAttack.baseForce = this.pushForce;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.crit = base.RollCrit();

            this.duration = this.baseDuration; /*/ this.attackSpeedStat;*/ //doesn't scale at all.
            this.earlyExitTime = this.baseEarlyExitTime; /* / this.attackSpeedStat; doesn't scale at all*/

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = System.Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(),
                    (HitBoxGroup element) => element.groupName == "inferno");
            }

            this.attack = new OverlapAttack();
            this.attack.damageType = DamageType.Generic;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = Modules.StaticValues.infernoGuillotineAirborneCoefficient * this.damageStat;
            this.attack.procCoefficient = 1.0f;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;
            this.attack.forceVector = Vector3.down;
            this.attack.pushAwayForce = 100f;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}