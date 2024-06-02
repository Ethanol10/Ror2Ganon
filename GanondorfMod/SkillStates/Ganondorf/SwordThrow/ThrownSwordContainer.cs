using GanondorfMod.Modules;
using GanondorfMod.Modules.Networking;
using GanondorfMod.Modules.Survivors;
using R2API.Networking.Interfaces;
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
        public TriforceBuffComponent triforceBuffComponent;
        public float distanceToThrow;
        public float damageToDeal;
        public Ray aimRay;
        public Vector3 startingPosition;
        public bool isReal;
        public Transform meshObj;

        private bool travellingBack = false;
        private float timeToTravel;
        private float stopwatch;
        private float hitStopwatch;
        private BlastAttack blastAttack;
        private uint spinningSound;

        public void Awake() 
        {
            UpdateAimRay();
            meshObj = gameObject.transform.GetChild(0);
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

            spinningSound = AkSoundEngine.PostEvent(3308368998, this.gameObject);

            triforceBuffComponent = charBody.gameObject.GetComponent<TriforceBuffComponent>();
        }

        public void Update() 
        {
            stopwatch += Time.fixedDeltaTime;
            SPIIIIiiiiIIIIIN();

            if (travellingBack) 
            {
                transform.position = Vector3.Lerp(targetPosition, charBody.gameObject.transform.position, stopwatch / (timeToTravel/2.0f));
                if (stopwatch > timeToTravel / 2.0f) 
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
            new StopSoundEventNetworkRequest(spinningSound).Send(R2API.Networking.NetworkDestination.Clients);
            if (charBody)
            {
                GanondorfController ganoncon = charBody.gameObject.GetComponent<GanondorfController>();
                ganoncon.ReenableSword();
                ganoncon.SwapToSword();
            }
        }

        public void OnHitAuthority(BlastAttack.Result result) 
        {
            foreach (BlastAttack.HitPoint hitPoint in result.hitPoints) 
            {
                Util.PlaySound("swordHitSound", base.gameObject);
                EffectManager.SpawnEffect(Modules.Assets.swordHitImpactEffect, new EffectData
                {
                    origin = hitPoint.hitPosition,
                    scale = 3f,
                }, true);
                triforceBuffComponent.AddToBuffCount(1);
            }
        }

        public void SPIIIIiiiiIIIIIN() 
        {
            Vector3 dirToFace;
            dirToFace = targetPosition - startingPosition;
            transform.rotation = Quaternion.LookRotation(dirToFace, Vector3.up);
            float rot = Modules.StaticValues.swordThrowRotationSpeed * Time.deltaTime;
            meshObj.Rotate(rot, 0f, 0f, Space.Self);
            meshObj.localPosition = Vector3.zero;
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