using EntityStates;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Survivors;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates
{
    internal class ObliterateEnd : BaseSkillState
    {
        public float noOfExplosions;
        public float damage;
        public float distance;
        private float animTimer;
        public bool isUpgraded;
        public bool halfBoosted;
        public bool fullBoosted;
        public float distanceSub;
        public float distanceSpawned;
        public float triggerBlast;
        public Ray aimRay;
        public BlastAttack blastAttack;

        public bool blastCompleted;
        public int explosionsPerformed;

        public float blastStopwatch;
        public bool startTriggeringBlast;

        public bool enemyHit;

        public GanondorfController ganoncon;

        public override void OnEnter()
        {
            base.OnEnter();

            base.PlayCrossfade("FullBody, Override", "EndForwardSmash", 0.1f);
            animTimer = 1.55f;
            explosionsPerformed = 0;
            distanceSpawned = 1f;
            enemyHit = false;
            blastStopwatch = 0f;
            blastCompleted = false;
            triggerBlast = animTimer / 5.0f;
            isUpgraded = false;
            startTriggeringBlast = false;
            int randomNum = UnityEngine.Random.Range(1, 100);
            if (randomNum < 2)
            {
                AkSoundEngine.PostEvent(3169574177, base.gameObject);
                isUpgraded = true;
            }
            else {
                AkSoundEngine.PostEvent(2241524948, base.gameObject);
                isUpgraded = false;
            }

            ganoncon = base.GetComponent<GanondorfController>();

            int buffCount = base.characterBody.GetBuffCount(Modules.Buffs.triforceBuff);
            float damageMultiplier = 1.0f;
            if (buffCount < Modules.StaticValues.maxPowerStack / 2)
            {
                damageMultiplier = 1.0f;
            }
            else if (buffCount >= Modules.StaticValues.maxPowerStack / 2 && buffCount < Modules.StaticValues.maxPowerStack)
            {
                damageMultiplier = Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducer / 3;
                halfBoosted = true;
            }
            else if (buffCount >= Modules.StaticValues.maxPowerStack)
            {
                damageMultiplier = Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducer;
                fullBoosted = true;
            }

            if (isUpgraded)
            {
                damage = damage * damageMultiplier * Modules.StaticValues.warlockMemeDamage;
            }
            else 
            {
                damage = damage * damageMultiplier;
            }

            aimRay = base.GetAimRay();
            aimRay.direction = new Vector3(aimRay.direction.x, 0f, aimRay.direction.z);

            distanceSub = distance / noOfExplosions;

            blastAttack = new BlastAttack();
            blastAttack.damageType = DamageType.Generic;
            blastAttack.attacker = base.gameObject;
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = base.teamComponent.teamIndex;
            blastAttack.baseDamage = damage;
            blastAttack.procCoefficient = 1.0f;
            blastAttack.radius = Modules.StaticValues.obliterateRadiusPerExplosion;
            blastAttack.bonusForce = Vector3.up;
            blastAttack.baseForce = 10000f;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.crit = base.RollCrit();
        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayCrossfade("FullBody, Override", "BufferEmpty", 0.05f);
            if (enemyHit) 
            {
                //Wipe buff if hit.
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
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(base.fixedAge > triggerBlast && !blastCompleted)
            {
                startTriggeringBlast = true;
            }

            if (startTriggeringBlast) 
            {
                blastStopwatch += Time.fixedDeltaTime;
                if (blastStopwatch >= 0.05f)
                {
                    blastStopwatch = 0f;
                    //Trigger Blast at distanceSpawned along ray without X/Z rot.

                    Vector3 pos = aimRay.GetPoint(distanceSpawned);
                    Ray downRay = new Ray(pos, Vector3.down);
                    Ray upRay = new Ray(pos, Vector3.up);
                    RaycastHit downHit;
                    RaycastHit upHit;
                    Physics.Raycast(downRay, out downHit, 20f, 1 << 11);
                    Physics.Raycast(upRay, out upHit, 20f, 1 << 11);

                    //Compare distances from origin
                    blastAttack.position = (downHit.distance <= upHit.distance) ? upHit.point : downHit.point;

                    EffectManager.SpawnEffect(Modules.Assets.beetleGuardGroundSlamObliterate, new EffectData
                    {
                        origin = (downHit.distance <= upHit.distance) ? upHit.point : downHit.point,
                        scale = 3f,
                    }, true);
                    EffectManager.SpawnEffect(Modules.Assets.parentSlamEffectObliterate, new EffectData
                    {
                        origin = (downHit.distance <= upHit.distance) ? upHit.point : downHit.point,
                        scale = 3f,
                    }, true);
                    int hitCount = blastAttack.Fire().hitCount;
                    if (explosionsPerformed <= 0) 
                    {
                        blastAttack.position = base.transform.position;
                        blastAttack.Fire();
                    }
                    if (hitCount > 0) 
                    {
                        enemyHit = true;
                    }
                    //Increment Distance
                    distanceSpawned += distanceSub;
                    //incrementExplosions
                    explosionsPerformed++;
                    if (explosionsPerformed >= (int)noOfExplosions) 
                    {
                        blastCompleted = true;
                        startTriggeringBlast = false;
                    }
                }
            }

            if (base.fixedAge > animTimer && blastCompleted) 
            {
                base.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
        }
    }
}
