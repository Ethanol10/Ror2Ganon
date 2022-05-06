using GanondorfMod.Modules.Survivors;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GanondorfMod.SkillStates.Ganondorf
{
    internal class ThrownSwordContainer : MonoBehaviour
    {
        public Vector3 targetPosition;
        public CharacterBody charBody;
        public float distanceToThrow;
        public float damageToDeal;
        public Ray aimRay;
        public Vector3 startingPosition;
        public bool isReal;

        private bool travellingBack = false;
        private float timeToTravel;
        private float stopwatch;
        private float hitStopwatch;
        private BlastAttack blastAttack;

        public void Awake() 
        {
            UpdateAimRay();
        }

        public void Start()
        {
            UpdateAimRay();
            stopwatch = 0f;
            hitStopwatch = 0f;
            timeToTravel = Modules.StaticValues.swordThrowMaxTimeOneWay;

            //Calculate final position of sword
            RaycastHit hit;
            Physics.Raycast(aimRay.origin, aimRay.direction, out hit, distanceToThrow, 1 << 11);

            //Found something, calculate to distance and setup travel time.
            if (hit.collider)
            {
                targetPosition = hit.point;
                float actualDistanceFromCollider = Vector3.Distance(targetPosition, startingPosition);

                //calculate the time to travel depending on how close ganon is.
                if (actualDistanceFromCollider > distanceToThrow) 
                {
                    timeToTravel = Modules.StaticValues.swordThrowMaxTimeOneWay;
                }
                else
                {
                    timeToTravel = actualDistanceFromCollider / distanceToThrow * Modules.StaticValues.swordThrowMaxTimeOneWay;
                }
            }
            //Otherwise use the default time
            else 
            {
                targetPosition = aimRay.GetPoint(distanceToThrow);
            }

            //setup blastattack
            blastAttack = new BlastAttack
            {
                attacker = charBody.gameObject,
                damageType = DamageType.BleedOnHit,
                canRejectForce = false,
                teamIndex = TeamIndex.Player,
                radius = Modules.StaticValues.swordThrowHitRadius,
                baseDamage = damageToDeal,
                baseForce = 1000f,
                inflictor = this.gameObject,
                procCoefficient = Modules.StaticValues.swordThrowProcCoefficient,
            };
        }

        public void Update() 
        {
            stopwatch += Time.fixedDeltaTime;
            SPIIIIiiiiIIIIIN();

            if (travellingBack) 
            {
                transform.position = Vector3.Lerp(targetPosition, startingPosition, stopwatch / timeToTravel);
                if (stopwatch > timeToTravel) 
                {
                    //Destroy, reset skills on ganondorf.
                    Destroy(this.gameObject);
                }
            }
            else
            {
                transform.position = Vector3.Lerp(startingPosition, targetPosition, stopwatch / timeToTravel);
            }

            if(stopwatch > timeToTravel && !travellingBack) 
            {
                travellingBack = true;
                stopwatch = 0f;
            }
           
        }

        public void FixedUpdate() 
        {
            if (isReal) 
            {
                hitStopwatch += Time.fixedDeltaTime;
                if (hitStopwatch > Modules.StaticValues.swordThrowTickFrequency) 
                {
                    hitStopwatch = 0f;
                    blastAttack.crit = charBody.RollCrit();
                    blastAttack.position = this.transform.position;
                    BlastAttack.Result result = blastAttack.Fire();
                    if (result.hitCount > 0) 
                    {
                        OnHitAuthority(result);
                    }
                }       
            }
        }

        public void OnDestroy() 
        {
            if (isReal) 
            {
                if (charBody) 
                {
                    GanondorfController ganoncon = charBody.gameObject.GetComponent<GanondorfController>();
                    ganoncon.ReenableSword();
                    ganoncon.SwapToSword();
                }
            }
        }

        public void OnHitAuthority(BlastAttack.Result result) 
        {
            foreach (BlastAttack.HitPoint hitPoint in result.hitPoints) 
            {
                EffectManager.SpawnEffect(Modules.Assets.swordHitImpactEffect, new EffectData
                {
                    origin = hitPoint.hitPosition,
                    scale = 3f,
                }, true);
            }
        }

        public void SPIIIIiiiiIIIIIN() 
        {
            if (travellingBack)
            {
                transform.Rotate(Vector3.right * (-1) * Modules.StaticValues.swordThrowRotationSpeed * Time.deltaTime);
            }
            else 
            {
                transform.Rotate(Vector3.right * Modules.StaticValues.swordThrowRotationSpeed * Time.deltaTime);
            }
        }

        public void UpdateAimRay() 
        {
            if (charBody) 
            {
                aimRay = charBody.inputBank.GetAimRay();
            }
        }
    }
}